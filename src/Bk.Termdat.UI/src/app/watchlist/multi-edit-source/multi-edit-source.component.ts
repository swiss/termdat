import {Location} from '@angular/common';
import {Component, OnDestroy, OnInit} from '@angular/core';
import {
    AbstractControl,
    FormGroup,
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    ValidationErrors,
    ValidatorFn,
    Validators,
} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatOption, MatSelect} from '@angular/material/select';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {EntrySourceUpdate} from 'api/EntrySourceUpdate';
import {LanguageSummary} from 'api/LanguageSummary';
import {Subject, takeUntil, tap} from 'rxjs';
import {MultiEditAction} from '../../../api/MultiEditAction';
import {EntryService} from '../../entry/entry.service';
import {ErrorService} from '../../shared/error.service';
import {LanguageService} from '../../shared/language.service';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-multi-edit-source',
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        RouterLink,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatSelect,
        SelectAllComponent,
        MatOption,
        MatError,
        MatInput,
        TranslatePipe,
        MatCheckboxModule,
        ObUnsavedChangesDirective,
    ],
    templateUrl: './multi-edit-source.component.html',
    styleUrl: './multi-edit-source.component.scss',
})
export class MultiEditSourceComponent implements OnInit, OnDestroy {
    form: UntypedFormGroup;
    languageControl: AbstractControl;
    languageOptions: LanguageSummary[] = [];
    languageOptionValues: number[] = [];
    languageOptionDefaultValues: number[] = [];
    ids: number[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly languageService: LanguageService,
        private readonly watchlistService: WatchlistService,
        private readonly location: Location,
        private readonly router: Router,
        private readonly translateService: TranslateService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly entryService: EntryService,
        public readonly errorService: ErrorService
    ) {
        this.form = this.formBuilder.group({
            language: new UntypedFormControl(null, {validators: [Validators.required]}),
            sources: this.formBuilder.group(
                {
                    definitionSource: [false],
                    noteSource: [false],
                    contextSource: [false],
                    abbreviationSource: [false],
                    terminusSource: [false],
                    nameSource: [false],
                    metadataSource: [false],
                    phraseologySource: [false],
                },
                {validators: [atLeastOneCheckedValidator()]}
            ),
            source: new UntypedFormControl('', {validators: [Validators.required]}),
        });
        this.languageControl = this.form.controls.language;
    }

    ngOnInit(): void {
        const state = this.location.getState() as {ids: number[]};
        this.ids = state?.ids ?? [];

        if (this.ids.length === 0) {
            void this.router.navigate(['watchlist']);
        }
        this.languageService.languageSummaries$
            .pipe(
                tap(languageOptions => {
                    this.languageOptions = languageOptions;
                    this.languageOptionValues = this.languageOptions.map(lo => lo.id);
                    this.languageOptionDefaultValues = this.languageOptions
                        .filter(l => ['DE', 'FR', 'IT', 'EN', 'RM'].includes(l.isoCode.toUpperCase()))
                        .map(lo => lo.id);
                    this.form.controls.language.setValue(this.languageOptionDefaultValues);
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe();
    }
    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    add(): void {
        this.callUpdate(MultiEditAction.Add);
    }

    set(): void {
        this.callUpdate(MultiEditAction.Set);
    }

    callUpdate(action: MultiEditAction): void {
        this.form.markAllAsTouched();
        if (!this.form.valid) {
            return;
        }
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

        const formEntry = this.getFormEntry(action);

        this.entryService.updateMultipleSource(formEntry).subscribe(
            entries => {
                if (entries.lockInfos && entries.lockInfos.length > 0) {
                    this.watchlistService.displayUpdateInfoWarning(entries.lockInfos, 'entry.notification.lockedEntriesNotUpdated');
                }
                if (entries.dataWasChanged) {
                    this.notificationService.success(this.translateService.instant('entry.notification.editMultipleSuccess'));
                } else {
                    this.notificationService.warning(this.translateService.instant('entry.notification.noChanges'));
                }
                if (entries.notFullyUpdatedEntryIds && entries.notFullyUpdatedEntryIds.length > 0) {
                    this.notificationService.warning(
                        this.translateService.instant('watchlist.multiEditSource.notFullyUpdatedEntries', {
                            ids: `\n${entries.notFullyUpdatedEntryIds.join(',')}`,
                        })
                    );
                }
            },
            () => this.notificationService.error(this.translateService.instant('entry.notification.editMultipleError'))
        );
    }

    getFormEntry(action: MultiEditAction): EntrySourceUpdate {
        const formValues = this.form.getRawValue();
        const sourcesGroup = this.form.get('sources')?.value;

        const selectedSources: string[] = Object.keys(sourcesGroup).filter(key => sourcesGroup[key]);

        return {
            ids: this.ids,
            updateValue: formValues.source,
            targetFields: selectedSources,
            languageIds: formValues.language,
            action,
        };
    }
}
export function atLeastOneCheckedValidator(): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
        if (!formGroup || !(formGroup instanceof Object)) {
            return null;
        }

        const controls = (formGroup as FormGroup).controls;
        const isAnyChecked = Object.keys(controls).some(key => controls[key].value === true);

        return isAnyChecked ? null : {atLeastOneRequired: true};
    };
}
