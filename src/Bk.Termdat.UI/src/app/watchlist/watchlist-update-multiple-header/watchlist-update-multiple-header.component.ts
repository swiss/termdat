import {Location} from '@angular/common';
import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatRadioButton, MatRadioGroup} from '@angular/material/radio';
import {MatSelect} from '@angular/material/select';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryHeaderModification} from 'api/EntryHeaderModification';
import {OfficeSummary} from 'api/OfficeSummary';
import {forkJoin, merge, of, Subject} from 'rxjs';
import {catchError, switchMap, takeUntil, tap} from 'rxjs/operators';
import {CollectionService} from '../../administration/collection/collection.service';
import {EntryService} from '../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {ErrorService} from '../../shared/error.service';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-watchlist-update-multiple-header',
    templateUrl: './watchlist-update-multiple-header.component.html',
    styleUrls: ['./watchlist-update-multiple-header.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        RouterLink,
        ReactiveFormsModule,
        AutocompleteComponent,
        MatLabel,
        MatError,
        MatFormField,
        MatSelect,
        MatOption,
        TranslatePipe,
        ObUnsavedChangesDirective,
        SubjectFormComponent,
        MatRadioGroup,
        MatRadioButton,
    ],
})
export class WatchlistUpdateMultipleHeaderComponent implements OnInit, OnDestroy {
    @ViewChild(SubjectFormComponent) subjectForm!: SubjectFormComponent;
    offices: OfficeSummary[] = [];
    collections: CollectionSummary[] = [];
    reliabilities: Option[] = [];
    states: Option[] = [];

    form: UntypedFormGroup;

    private readonly unsubscribe = new Subject<void>();

    private idsToModify: number[] = [];

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly translateService: TranslateService,
        private readonly router: Router,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly entryService: EntryService,
        private readonly watchlistService: WatchlistService,
        private readonly collectionService: CollectionService,
        private readonly notificationService: ObNotificationService,
        public readonly errorService: ErrorService,
        private readonly location: Location
    ) {
        this.form = this.formBuilder.group({
            office: [''],
            collection: [''],
            reliability: [''],
            state: [''],
            subjectDomains: this.formBuilder.array([]),
            replaceSubjects: [false],
        });
    }

    ngOnInit(): void {
        const state = this.location.getState() as {ids: number[]};
        this.idsToModify = state?.ids ?? [];
        if (this.idsToModify.length === 0) {
            void this.router.navigate(['watchlist']);
        }

        merge(of({lang: null, translations: null}), this.translateService.onLangChange)
            .pipe(
                switchMap(langChangeEvent =>
                    forkJoin({
                        langChangeEvent: of(langChangeEvent),
                        collections: this.collectionService.getModificationCollections().pipe(catchError(() => of([]))),
                        offices: this.extendedSearchFilterService.getOffices().pipe(catchError(() => of([]))),
                        states: this.extendedSearchFilterService.getStates().pipe(catchError(() => of([]))),
                        reliabilities: this.entryService.getReliabilities().pipe(catchError(() => of([]))),
                    })
                ),
                takeUntil(this.unsubscribe),
                tap(({collections, offices, states, reliabilities}) => {
                    this.offices = offices;
                    this.states = states;
                    this.collections = collections.slice(0);
                    this.reliabilities = reliabilities;
                })
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
            this.notificationService.error(this.translateService.instant('entry.notification.editMultipleError'));
        } else {
            this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

            const formEntry = this.getFormEntry();

            this.entryService.updateMandatoryHeaderFields(formEntry).subscribe(
                entries => {
                    if (entries.lockInfos!.length > 0) {
                        this.watchlistService.displayUpdateInfoWarning(entries.lockInfos!, 'entry.notification.lockedEntriesNotUpdated');
                    } else {
                        if (entries.dataWasChanged) {
                            this.notificationService.success(this.translateService.instant('entry.notification.editMultipleSuccess'));
                        } else {
                            this.notificationService.warning(this.translateService.instant('entry.notification.noChanges'));
                        }
                    }

                    void this.router.navigate(['watchlist']);
                },
                () => this.notificationService.error(this.translateService.instant('entry.notification.editMultipleError'))
            );
        }
    }

    get subjectDomains(): UntypedFormArray {
        return this.form.get('subjectDomains') as UntypedFormArray;
    }

    getFormEntry(): EntryHeaderModification {
        const formValues = this.form.getRawValue();

        return {
            ids: this.idsToModify,
            status: formValues.state,
            reliability: formValues.reliability,
            officeId: isNaN(formValues.office) ? null : formValues.office,
            collectionId: isNaN(formValues.collection) ? null : formValues.collection,
            replaceSubjects: formValues.replaceSubjects,
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            subjectIds: formValues.subjectDomains.map((sub: any) => {
                return sub.subject;
            }),
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            domainIds: formValues.subjectDomains.flatMap((sub: any) => {
                return sub.domains;
            }),
        } as EntryHeaderModification;
    }
}
