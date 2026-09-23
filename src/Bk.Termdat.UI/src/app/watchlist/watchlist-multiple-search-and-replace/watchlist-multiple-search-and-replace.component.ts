import {Location} from '@angular/common';
import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatRadioButton, MatRadioGroup} from '@angular/material/radio';
import {MatSelect} from '@angular/material/select';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {EntrySearchAndReplace} from 'api/EntrySearchAndReplace';
import {LanguageSummary} from 'api/LanguageSummary';
import {Subject} from 'rxjs';
import {takeUntil, tap} from 'rxjs/operators';
import {EntryService} from '../../entry/entry.service';
import {StorageKeys} from '../../search/StorageKeys';
import {ErrorService} from '../../shared/error.service';
import {LOCAL_STORAGE} from '../../shared/injection.tokens';
import {LanguageService} from '../../shared/language.service';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-watchlist-multiple-search-and-replace',
    templateUrl: './watchlist-multiple-search-and-replace.component.html',
    styleUrls: ['./watchlist-multiple-search-and-replace.component.scss'],
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
        MatRadioGroup,
        MatRadioButton,
        MatInput,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class WatchlistMultipleSearchAndReplaceComponent implements OnInit, OnDestroy {
    form: UntypedFormGroup;
    languageControl: AbstractControl;
    languageOptions: LanguageSummary[] = [];
    languageOptionValues: number[] = [];
    languageOptionDefaultValues: number[] = [];
    ids: number[] = [];
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        public readonly errorService: ErrorService,
        private readonly languageService: LanguageService,
        private readonly watchlistService: WatchlistService,
        private readonly location: Location,
        private readonly router: Router,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly entryService: EntryService,
        @Inject(LOCAL_STORAGE) private readonly localStorage: Storage
    ) {
        this.form = this.formBuilder.group({
            searchField: new UntypedFormControl('definition'),
            searchValue: new UntypedFormControl('', {validators: [Validators.required]}),
            replaceValue: '',
            language: new UntypedFormControl(null, {validators: [Validators.required]}),
        });
        this.languageControl = this.form.controls.language;
        this.languageControl.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe(value => {
            if (Array.isArray(value) && value.length > 0) {
                this.localStorage.setItem(StorageKeys.SELECTED_SEARCH_AND_REPLACE_LANGUAGES_LOCAL_STORAGE_KEY, JSON.stringify(value));
            }
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    save(): void {
        this.form.markAllAsTouched();
        if (!this.form.valid) {
            this.notificationService.error('entry.notification.editMultipleError');
        } else {
            this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

            const formEntry = this.getFormEntry();

            this.entryService.multipleSearchAndReplace(formEntry).subscribe(
                entry => {
                    if (entry.lockInfos && entry.lockInfos.length > 0) {
                        this.watchlistService.displayUpdateInfoWarning(entry.lockInfos, 'entry.notification.lockedEntriesNotUpdated');
                    }

                    if (entry.entryCount > 0) {
                        let messageId = 'entry.notification.searchAndReplaceSuccessMoreInMore';

                        if (entry.affectedOccurrences === 1 && entry.entryCount === 1) {
                            messageId = 'entry.notification.searchAndReplaceSuccessOneInOne';
                        } else if (entry.affectedOccurrences > 1 && entry.entryCount === 1) {
                            messageId = 'entry.notification.searchAndReplaceSuccessMoreInOne';
                        }

                        this.notificationService.success({
                            message: messageId,
                            messageParams: {occurrences: entry.affectedOccurrences, entries: entry.entryCount},
                        });
                    } else {
                        this.notificationService.warning({
                            message: 'entry.notification.searchAndReplaceNotFound',
                        });
                    }
                },
                () => this.notificationService.error('entry.notification.editMultipleError')
            );
        }
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
                    const storedLanguages = this.localStorage.getItem(StorageKeys.SELECTED_SEARCH_AND_REPLACE_LANGUAGES_LOCAL_STORAGE_KEY);
                    if (storedLanguages) {
                        const parsedLanguages = JSON.parse(storedLanguages);
                        this.form.controls.language.setValue(parsedLanguages);
                    } else {
                        this.form.controls.language.setValue(this.languageOptionDefaultValues);
                    }
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe();
    }

    getFormEntry(): EntrySearchAndReplace {
        const formValues = this.form.getRawValue();
        return {
            ids: this.ids,
            searchValue: formValues.searchValue,
            replaceValue: formValues.replaceValue,
            searchField: formValues.searchField,
            languageIds: formValues.language,
        };
    }
}
