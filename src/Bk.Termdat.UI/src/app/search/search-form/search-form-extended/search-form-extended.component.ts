import {AsyncPipe, KeyValuePipe} from '@angular/common';
import {AfterViewInit, ChangeDetectorRef, Component, EventEmitter, Inject, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteTrigger, MatOption} from '@angular/material/autocomplete';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatDatepickerToggle, MatDateRangeInput, MatDateRangePicker, MatEndDate, MatStartDate} from '@angular/material/datepicker';
import {MatError, MatFormField, MatLabel, MatSuffix} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {MatSlideToggle} from '@angular/material/slide-toggle';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObInputClearDirective} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {CommentStatusOption} from 'api/CommentStatusOption';
import {OfficeSummary} from 'api/OfficeSummary';
import {SearchQuery} from 'api/SearchQuery';
import {SubjectSummary} from 'api/SubjectSummary';
import {UserAccountSummary} from 'api/UserAccountSummary';
import {isEqual} from 'lodash';
import {combineLatest, forkJoin, merge, Observable, of, Subject, zip} from 'rxjs';
import {catchError, filter, map, shareReplay, startWith, switchMap, takeUntil, withLatestFrom} from 'rxjs/operators';
import {AuthService} from '../../../auth/auth.service';
import {EntryService} from '../../../entry/entry.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {SESSION_STORAGE, WINDOW} from '../../../shared/injection.tokens';
import {toCodeNameLabel} from '../../../shared/LabelFormat';
import {Option} from '../../../shared/Option';
import {ExtendedSearchFiltersService} from '../../extended-search-filters.service';
import {StorageKeys} from '../../StorageKeys';
import {SearchFormService} from '../search-form.service';

@Component({
    selector: 'td-search-form-extended',
    templateUrl: './search-form-extended.component.html',
    styleUrls: ['./search-form-extended.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        ObInputClearDirective,
        MatIcon,
        MatError,
        ChiplistAutocompleteComponent,
        MatSlideToggle,
        MatSelect,
        MatOption,
        MatAutocompleteTrigger,
        MatAutocomplete,
        MatTooltip,
        MatCheckbox,
        MatDateRangeInput,
        MatStartDate,
        MatEndDate,
        MatDatepickerToggle,
        MatSuffix,
        MatDateRangePicker,
        MatButton,
        ObButtonDirective,
        AsyncPipe,
        KeyValuePipe,
        TranslatePipe,
    ],
})
export class SearchFormExtendedComponent implements OnInit, OnDestroy, AfterViewInit {
    get form(): FormGroup | undefined {
        return this._form;
    }
    @Input() set form(value: FormGroup | undefined) {
        this._form = value;
        if (this._form) {
            const savedValues = this.getSavedSearchValues();
            if (savedValues) {
                this._form.markAsDirty();
            }
            this._form = this.searchFormService.setupFormGroup(this._form, savedValues);
        }
    }
    @Output() readonly searched = new EventEmitter();
    adminInfoChips!: Chip[];
    collectionChips!: Chip[];
    relatedCollectionChips!: Chip[];
    subjectDomainChips!: Chip[];
    officeChips!: Chip[];
    glossaryChips!: Chip[];
    userAccounts!: UserAccountSummary[];
    states!: Option[];
    reliabilities!: Option[];
    commentStatusOptions!: CommentStatusOption[];
    offices!: OfficeSummary[];

    modifiedByUserAccounts$!: Observable<UserAccountSummary[]>;
    createdByUserAccounts$!: Observable<UserAccountSummary[]>;
    dataLoadedSubject = new Subject<boolean>();

    isAdmin = false;
    isSuperUser = false;
    isSuperReader = false;
    isWriter = false;
    showForm = true;

    private _form: FormGroup | undefined;
    private readonly unsubscribe = new Subject<void>();
    private formDefaultValues!: SearchQuery;

    constructor(
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly translateService: TranslateService,
        private readonly authService: AuthService,
        private readonly searchFormService: SearchFormService,
        private readonly entryService: EntryService,
        private readonly changeDetectorRef: ChangeDetectorRef,
        @Inject(WINDOW) private readonly window: Window,
        @Inject(SESSION_STORAGE) private readonly sessionStorage: Storage
    ) {}

    get showSlideToggleTooltip(): boolean {
        return this.window.matchMedia('(max-width: 1500px)').matches;
    }

    ngOnInit(): void {
        this.formDefaultValues = this.searchFormService.getFormDefaults();

        this.extendedSearchFilterService.resetExtendedSearch$
            .pipe(
                withLatestFrom(this.dataLoadedSubject.pipe(startWith(false))),
                filter(([_, dataLoaded]) => dataLoaded),
                takeUntil(this.unsubscribe)
            )
            .subscribe(() => {
                if (this.form) {
                    this.form.reset(this.formDefaultValues);
                    this.form.markAsPristine();
                    this.form.markAsUntouched();

                    // Note: Hide and show the form to force Angular to reset the form controls visually (checkboxes).
                    this.showForm = false;
                    this.changeDetectorRef.detectChanges();
                    this.showForm = true;
                }
            });

        this.initializeData();
        this.extendedSearchFilterService.reloadExtendedSearchData$.pipe(takeUntil(this.unsubscribe)).subscribe(() => this.initializeData());
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.dataLoadedSubject.complete();
    }

    ngAfterViewInit(): void {
        this.form?.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe((formValues: SearchQuery) => this.verifyIsEqualtoDefaultValues(formValues));
        this.setupExcludePriorityInteractions();
    }

    search(): void {
        if (this.form?.valid) {
            this.searched.emit();
        }
    }

    reset(): void {
        this.sessionStorage.removeItem(StorageKeys.SEARCH_SESSION_STORAGE_KEY);
        this.extendedSearchFilterService.resetExtendedSearch$.next();
    }

    filterUserAccounts(value: string): UserAccountSummary[] {
        if (value) {
            const filterValue = value.toUpperCase();

            return this.userAccounts.filter(
                userAccount =>
                    userAccount.lastName.toUpperCase().includes(filterValue) ||
                    userAccount.firstName.toUpperCase().includes(filterValue) ||
                    userAccount.userName.toUpperCase().includes(filterValue)
            );
        }

        return this.userAccounts.slice();
    }

    clear(fieldName: string) {
        this.form?.get(fieldName)?.patchValue(null);
    }

    private setupExcludePriorityInteractions(): void {
        const pairs: {exclude: string; priority: string}[] = [
            {exclude: 'collectionsExclude', priority: 'collectionsPriority'},
            {exclude: 'adminInfosExclude', priority: 'adminInfosPriority'},
            {exclude: 'createdByExclude', priority: 'createdByPriority'},
            {exclude: 'modifiedByExclude', priority: 'modifiedByPriority'},
            {exclude: 'modifiedPeriodExclude', priority: 'modifiedPeriodPriority'},
        ];

        pairs.forEach(({exclude, priority}) => {
            const excludeControl = this.form?.get(exclude);
            const priorityControl = this.form?.get(priority);
            if (!excludeControl || !priorityControl) {
                return;
            }

            if (excludeControl.value) {
                priorityControl.setValue(false, {emitEvent: false});
                priorityControl.disable({emitEvent: false});
            }

            excludeControl.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe((excludeValue: boolean) => {
                if (excludeValue) {
                    priorityControl.setValue(false, {emitEvent: false});
                    priorityControl.disable({emitEvent: false});
                } else {
                    priorityControl.enable({emitEvent: false});
                }
            });
        });
    }

    private verifyIsEqualtoDefaultValues(formValues: SearchQuery): void {
        const values = {...formValues};
        values.createdBy = values.createdBy === null || values.createdBy === '' ? undefined : values.createdBy;
        values.modifiedBy = values.modifiedBy === null || values.modifiedBy === '' ? undefined : values.modifiedBy;
        values.modifiedFrom = values.modifiedFrom === null ? undefined : values.modifiedFrom;
        values.modifiedTo = values.modifiedTo === null ? undefined : values.modifiedTo;

        if (isEqual(values, this.formDefaultValues)) {
            this.form?.markAsPristine();
        }
    }

    private createSubjectDomainChips(subjects: SubjectSummary[]): Chip[] {
        const mapped = subjects.flatMap(subject => {
            const subjectDomains =
                subject.domains?.map(domain => {
                    const name = `${subject.name}: ${domain.name}`;
                    return {
                        value: `${subject.id}-${domain.id}`,
                        displayText: name,
                        tag: name,
                    };
                }) ?? [];

            return [
                {
                    value: subject.id.toString(10),
                    displayText: subject.name,
                    tag: subject.name,
                },
                ...subjectDomains,
            ];
        });

        mapped.sort((a, b) => a.displayText.localeCompare(b.displayText, this.translateService.getCurrentLang(), {sensitivity: 'accent'}));

        return mapped;
    }

    private getSavedSearchValues(): SearchQuery | undefined {
        const sessionValues = this.sessionStorage.getItem(StorageKeys.SEARCH_SESSION_STORAGE_KEY);

        return sessionValues ? (JSON.parse(sessionValues) as SearchQuery) : undefined;
    }

    private initializeData(): void {
        zip(this.authService.isAdmin$, this.authService.isSuperUser$, this.authService.isWriter$, this.authService.isSuperReader$)
            .pipe(
                switchMap(([isAdmin, isSuperUser, isWriter, isSuperReader]) => {
                    if (isAdmin || isSuperUser || isWriter || isSuperReader) {
                        return this.extendedSearchFilterService.getAdminInfos().pipe(catchError(() => of([])));
                    }
                    return of([]);
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe(adminInfos => {
                this.adminInfoChips = adminInfos.map(info => new Chip(info.code, info.code, info.id));
            });

        const userAccounts$ = merge(
            zip(this.authService.isAdmin$, this.authService.isSuperUser$).pipe(map(([isAdmin, isSuperUser]) => isAdmin || isSuperUser)),
            merge(this.translateService.onLangChange, this.extendedSearchFilterService.cleared$).pipe(switchMap(() => of(this.isAdmin || this.isSuperUser)))
        ).pipe(
            switchMap(isAuthorized => {
                if (isAuthorized) {
                    return this.extendedSearchFilterService.getUserAccounts().pipe(catchError(() => of([])));
                }
                return of([]);
            }),
            takeUntil(this.unsubscribe),
            shareReplay(1)
        );

        userAccounts$.subscribe(userAccounts => {
            this.userAccounts = userAccounts;
        });

        combineLatest([
            merge(of(true), this.translateService.onLangChange).pipe(
                switchMap(() =>
                    forkJoin({
                        collections: this.extendedSearchFilterService.getCollections().pipe(catchError(() => of([]))),
                        subjects: this.extendedSearchFilterService.getSubjects().pipe(catchError(() => of([]))),
                        offices: this.extendedSearchFilterService.getOffices().pipe(catchError(() => of([]))),
                        glossaries: this.extendedSearchFilterService.getGlossaries().pipe(catchError(() => of([]))),
                        states: this.extendedSearchFilterService.getStates().pipe(catchError(() => of([]))),
                        commentStatusOptions: this.extendedSearchFilterService.getCommentStatusOptions().pipe(catchError(() => of([]))),
                        reliabilities: this.entryService.getReliabilities().pipe(catchError(() => of([]))),
                    })
                )
            ),
            combineLatest([this.authService.isAdmin$, this.authService.isSuperUser$, this.authService.isWriter$]).pipe(
                map(([isAdmin, isSuperUser, isWriter]) => isAdmin || isSuperUser || isWriter)
            ),
        ])
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(([{collections, subjects, offices, glossaries, states, commentStatusOptions, reliabilities}, canSeeActiveApi]) => {
                const collectionDisplayName = (c: CollectionSummary): string => {
                    let name = c.name;
                    if (c.deleted) {
                        name += ` (${this.translateService.instant('field.deleted')})`;
                    } else if (!c.active) {
                        name += ` (${this.translateService.instant('field.inactive')})`;
                    }
                    if (c.activeApi && canSeeActiveApi) {
                        name += ` (${this.translateService.instant('administration.collection.field.activeApi')})`;
                    }
                    if (c.exportLindas) {
                        name += ' (LINDAS)';
                    }
                    return name;
                };
                this.collectionChips = collections.map(
                    c => new Chip(c.code, toCodeNameLabel({code: c.code, name: collectionDisplayName(c)}), c.id, c.deleted, !c.active)
                );
                this.relatedCollectionChips = collections.map(
                    c => new Chip(c.code, toCodeNameLabel({code: c.code, name: collectionDisplayName(c)}), c.id, c.deleted, !c.active)
                );
                this.subjectDomainChips = this.createSubjectDomainChips(subjects);
                this.offices = offices;
                this.officeChips = offices.map(Chip.fromOption);
                this.glossaryChips = glossaries.map(Chip.fromOption);
                this.states = states;
                this.commentStatusOptions = commentStatusOptions;
                this.reliabilities = reliabilities;

                this.dataLoadedSubject.next(true);
            });

        this.authService.isAdmin$.pipe(takeUntil(this.unsubscribe)).subscribe(isAdmin => {
            this.isAdmin = isAdmin;
        });

        this.authService.isSuperUser$.pipe(takeUntil(this.unsubscribe)).subscribe(isSuperUser => {
            this.isSuperUser = isSuperUser;
        });

        this.authService.isSuperReader$.pipe(takeUntil(this.unsubscribe)).subscribe(isSuperReader => {
            this.isSuperReader = isSuperReader;
        });

        this.authService.isWriter$.pipe(takeUntil(this.unsubscribe)).subscribe(isWriter => {
            this.isWriter = isWriter;
        });

        this.modifiedByUserAccounts$ = combineLatest([userAccounts$, this.form!.controls.modifiedBy.valueChanges.pipe(startWith(''))]).pipe(
            map(([_, value]) => this.filterUserAccounts(value))
        );

        this.createdByUserAccounts$ = combineLatest([userAccounts$, this.form!.controls.createdBy.valueChanges.pipe(startWith(''))]).pipe(
            map(([_, value]) => this.filterUserAccounts(value))
        );
    }
}
