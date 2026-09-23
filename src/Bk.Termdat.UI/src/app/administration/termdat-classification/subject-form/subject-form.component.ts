import {AsyncPipe, KeyValuePipe} from '@angular/common';
import {ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatDivider} from '@angular/material/divider';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {DomainModification} from 'api/DomainModification';
import {LanguageDescription} from 'api/LanguageDescription';
import {SubjectModification} from 'api/SubjectModification';
import {Observable, of} from 'rxjs';
import {catchError, shareReplay, take} from 'rxjs/operators';
import {TermdatClassificationDomainFormComponent} from '../domain-form/domain-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';

@Component({
    selector: 'td-classification-subject-form',
    templateUrl: './subject-form.component.html',
    styleUrls: ['./subject-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatError,
        TermdatClassificationDomainFormComponent,
        MatDivider,
        AsyncPipe,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class TermdatClassificationSubjectFormComponent implements OnChanges {
    @Input() titleTranslateKey!: string;
    @Input() set subject(value: SubjectModification) {
        this.form.reset({
            id: value.id,
            descriptionDe: value.descriptionDe,
            descriptionFr: value.descriptionFr,
            descriptionEn: value.descriptionEn,
            descriptionRm: value.descriptionRm,
            descriptionIt: value.descriptionIt,
            domains: [],
        });

        value.domains.forEach(item => this.domains.push(this.createDomainForm(item)));

        this.handleFormState();
    }
    @Input() disabledSubjectForm = false;
    @Input() disabledDomainForm = false;
    @Output() readonly saved = new EventEmitter<SubjectModification>();
    @Output() readonly canceled = new EventEmitter<void>();

    readonly form: UntypedFormGroup;
    readonly descriptions$: Observable<LanguageDescription[]>;

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly termdatClassificationService: TermdatClassificationService
    ) {
        this.form = this.formBuilder.group({
            id: {value: undefined, disabled: true},
            descriptionDe: this.formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionFr: this.formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionEn: this.formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionRm: this.formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionIt: this.formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            domains: this.formBuilder.array([]),
        });

        this.descriptions$ = this.termdatClassificationService.getDomainLanguageDescriptions().pipe(
            catchError(() => of([])),
            shareReplay(1)
        );

        // Initially preload descriptions
        this.descriptions$.pipe(take(1)).subscribe();
    }

    ngOnChanges(): void {
        this.handleFormState();
    }

    get domains(): UntypedFormArray {
        return this.form.get('domains') as UntypedFormArray;
    }

    toUppercase(event: Event, controlsName: string) {
        const inputValue = (event.target as HTMLInputElement)?.value;
        this.form.controls[controlsName].patchValue(inputValue.toUpperCase());
    }

    save(): void {
        this.form.markAllAsTouched();
        if (this.form.invalid) {
            return;
        }
        this.saved.emit(this.form.getRawValue());
    }

    addDomain(): void {
        this.domains.push(
            this.createDomainForm({
                id: undefined,
                subjectId: undefined,
                descriptionDe: '',
                descriptionFr: '',
                descriptionEn: '',
                descriptionRm: '',
                descriptionIt: '',
            })
        );

        this.handleFormState();
    }

    removeDomain(index: number): void {
        const item = this.domains.at(index) as UntypedFormGroup;
        if (item.controls.id.value) {
            return;
        }

        this.domains.removeAt(index);
    }

    private handleFormState(): void {
        if (this.disabledSubjectForm) {
            this.form.disable();
        } else {
            this.form.enable();
            this.form.controls.id.disable(); // Id is never editable
        }

        this.domains.controls.forEach(item => {
            const control = item as UntypedFormGroup;
            if (this.disabledDomainForm && control.controls.id.value) {
                control.disable();
            } else {
                control.enable();

                // Ids are never editable
                control.controls.id.disable();
                control.controls.subjectId.disable();
            }
        });
    }

    private createDomainForm(domain: DomainModification) {
        return this.formBuilder.group({
            id: {value: domain.id, disabled: true},
            subjectId: {value: domain.subjectId, disabled: true},
            descriptionDe: this.formBuilder.control(domain.descriptionDe, [Validators.maxLength(1000), Validators.required]),
            descriptionFr: this.formBuilder.control(domain.descriptionFr, [Validators.maxLength(1000), Validators.required]),
            descriptionEn: this.formBuilder.control(domain.descriptionEn, [Validators.maxLength(1000), Validators.required]),
            descriptionRm: this.formBuilder.control(domain.descriptionRm, [Validators.maxLength(1000), Validators.required]),
            descriptionIt: this.formBuilder.control(domain.descriptionIt, [Validators.maxLength(1000), Validators.required]),
        });
    }
}
