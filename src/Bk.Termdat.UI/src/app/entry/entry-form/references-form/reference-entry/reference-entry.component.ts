import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {MatTooltip} from '@angular/material/tooltip';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {EntryReferenceLanguageDetail} from 'api/EntryReferenceLanguageDetail';
import {ReferenceType} from 'api/ReferenceType';
import {merge, of, Subject} from 'rxjs';
import {catchError, debounceTime, map, switchMap, takeUntil, tap} from 'rxjs/operators';
import {ErrorService} from '../../../../shared/error.service';
import {LanguageService} from '../../../../shared/language.service';
import {Option} from '../../../../shared/Option';
import {EntryService} from '../../../entry.service';
import {ReferenceEntryMode} from './ReferenceEntryMode';

@Component({
    selector: 'td-reference-entry',
    templateUrl: './reference-entry.component.html',
    styleUrls: ['./reference-entry.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatSelect,
        MatOption,
        MatInput,
        MatError,
        RouterLink,
        MatIcon,
        MatIconButton,
        ObButtonDirective,
        MatTooltip,
        TranslatePipe,
    ],
})
export class ReferenceEntryComponent implements OnInit, OnDestroy {
    referenceTypes: Option[] = [];
    text = '';
    languages: EntryReferenceLanguageDetail[] = [];
    REFERENCE_ENTRY_MODE = ReferenceEntryMode;
    loading = false;

    @Input() mode: ReferenceEntryMode = ReferenceEntryMode.Create;
    @Input() _form!: UntypedFormGroup;

    @Input() set entryReference(value: EntryReferenceDetail | undefined) {
        this._entryReference = value;
        this.reset();
        this.languages = value?.languages ?? [];
        this.text = this.getTextFromLanguages();

        if (this.form) {
            this.setupForm(value);
        }
    }

    @Input()
    set form(value: UntypedFormGroup) {
        this._form = value;

        if (this._entryReference !== undefined) {
            this.setupForm(this._entryReference);
        }
    }

    get form(): UntypedFormGroup {
        return this._form;
    }

    @Output() readonly referenceRemoved = new EventEmitter<void>();
    @Output() readonly referenceCreated = new EventEmitter<EntryReferenceDetail>();

    private _entryReference?: EntryReferenceDetail;
    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        public readonly errorService: ErrorService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly translateService: TranslateService,
        private readonly entryService: EntryService,
        private readonly languageService: LanguageService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    ngOnInit(): void {
        this.setupForm(this._entryReference);

        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.entryService.getReferenceTypes()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(options => (this.referenceTypes = options));

        this.translateService.onLangChange.pipe(takeUntil(this.unsubscribe)).subscribe(() => (this.text = this.getTextFromLanguages()));

        this.form.controls.entryId.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe(() => {
            this.text = '';
            this.languages = [];
        });

        this.form.controls.entryId.valueChanges
            .pipe(
                tap(() => (this.loading = true)),
                debounceTime(300),
                switchMap(value => {
                    const id = parseInt(value, 10);

                    if (isNaN(id)) {
                        return of({entry: null, error: undefined});
                    }

                    this.httpApiInterceptorEvents.deactivateOnNextAPICalls(1);

                    return this.entryService.getReferenceEntry(id).pipe(
                        map(result => {
                            return {entry: result, error: false};
                        }),
                        catchError(() => {
                            return of({entry: null, error: true});
                        })
                    );
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe(({entry, error}) => {
                this.loading = false;
                if (error === true) {
                    this.form.controls.entryId.setErrors({unknownEntryId: true});
                    // mat-error only shows if control is touched
                    this.form.controls.entryId.markAsTouched();
                } else if (error === false) {
                    this.form.controls.entryId.setErrors(null);
                }

                if (!entry?.languages?.length) {
                    this.text = '';
                    this.languages = [];
                } else {
                    this.languages = entry.languages;
                    this.text = this.getTextFromLanguages();
                }
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    remove(): void {
        this.referenceRemoved.emit();
    }

    create(): void {
        this.form.markAllAsTouched();

        if (!this.form.valid || !this.form.value.entryId) {
            return;
        }

        const value = {
            ...this.form.value,
            languages: this.languages,
        } as EntryReferenceDetail;

        this.referenceCreated.emit(value);

        this.reset();
    }

    private getTextFromLanguages(): string {
        const languageId = this.languageService.getCurrentLanguageId();
        const languageHit = this.languages.find(lang => lang.languageId === languageId);

        if (languageHit) {
            return languageHit.text;
        }

        return this.languages.length > 0 ? this.languages[0].text : '';
    }

    private setupForm(reference?: EntryReferenceDetail): void {
        if (!this.form) {
            return;
        }

        this.form.setControl(
            'type',
            this.formBuilder.control(reference?.type ?? ReferenceType.None, {
                validators: [Validators.required],
            })
        );

        this.form.setControl(
            'entryId',
            this.formBuilder.control(reference?.entryId, {
                validators: [Validators.required, Validators.pattern(/[0-9]+/)],
                updateOn: 'change',
            })
        );

        this.form.setControl('id', this.formBuilder.control(reference?.id));
    }

    private reset(): void {
        this.text = '';
        this.languages = [];

        if (!this.form) {
            return;
        }

        this.form.reset(
            {
                type: ReferenceType.None,
                entryId: null,
            },
            {
                emitEvent: false,
            }
        );
    }
}
