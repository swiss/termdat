import {Location} from '@angular/common';
import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, UntypedFormGroup, ValidationErrors, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatOption, MatSelect} from '@angular/material/select';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {EntryContentMove} from 'api/EntryContentMove';
import {LanguageSummary} from 'api/LanguageSummary';
import {Subject, takeUntil, tap} from 'rxjs';
import {EntryService} from '../../entry/entry.service';
import {ErrorService} from '../../shared/error.service';
import {LanguageService} from '../../shared/language.service';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-multi-move-content',
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
        TranslatePipe,
        MatCheckboxModule,
        ObUnsavedChangesDirective,
    ],
    templateUrl: './multi-move-content.component.html',
    styleUrl: './multi-move-content.component.scss',
})
export class MultiMoveContentComponent implements OnInit, OnDestroy {
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
        this.form = this.formBuilder.group(
            {
                language: new UntypedFormControl(null, {validators: [Validators.required]}),
                sourceField: new UntypedFormControl('', {validators: [Validators.required]}),
                destinationField: new UntypedFormControl('', {validators: [Validators.required]}),
                forAllSynonyms: new UntypedFormControl(true, {validators: [Validators.required]}),
            },
            {validators: this.sourceDestinationDifferentValidator}
        );
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

    save(): void {
        this.form.markAllAsTouched();
        if (!this.form.valid) {
            return;
        }
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

        const formEntry = this.getFormEntry();
        this.entryService.moveContent(formEntry).subscribe(
            entries => {
                if (entries.lockInfos!.length > 0) {
                    this.watchlistService.displayUpdateInfoWarning(entries.lockInfos!, 'entry.notification.lockedEntriesNotUpdated');
                }
                if (entries.dataWasChanged) {
                    this.notificationService.success('entry.notification.editMultipleSuccess');
                    if (entries.nonAffectedEntryIds.length > 0) {
                        this.displayNonAffectedWarning(entries.nonAffectedEntryIds, 'watchlist.moveContent.nonAffectedEntries');
                    }
                } else {
                    this.notificationService.warning(this.translateService.instant('entry.notification.noChanges'));
                }
            },
            () => this.notificationService.error('entry.notification.editMultipleError')
        );
    }

    displayNonAffectedWarning(entryIds: number[], lockedMessage: string): void {
        if (entryIds.length > 0) {
            const lockedMessageText = this.translateService.instant(lockedMessage);
            this.notificationService.warning({
                message: `${lockedMessageText}\n${entryIds.join(', ')}`,
                timeout: 15000,
            });
        }
    }

    getFormEntry(): EntryContentMove {
        const formValues = this.form.getRawValue();

        return {
            ids: this.ids,
            sourceField: formValues.sourceField,
            destinationField: formValues.destinationField,
            languageIds: formValues.language,
            forAllSynonyms: formValues.forAllSynonyms,
        };
    }

    sourceDestinationDifferentValidator(group: AbstractControl): ValidationErrors | null {
        const source = group.get('sourceField')?.value;
        const destination = group.get('destinationField')?.value;

        if (source && destination && source === destination) {
            return {sameField: true};
        }

        return null;
    }
}
