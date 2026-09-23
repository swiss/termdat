import {AsyncPipe, NgClass} from '@angular/common';
import {AfterViewInit, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, ViewChild, DOCUMENT, signal, DestroyRef} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {AbstractControl, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteTrigger, MatOption} from '@angular/material/autocomplete';
import {MatBadge} from '@angular/material/badge';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatPaginator, PageEvent} from '@angular/material/paginator';
import {MatSelect} from '@angular/material/select';
import {MatTooltip} from '@angular/material/tooltip';
import {ActivatedRoute, NavigationEnd, Params, Router, RouterLink, RouterOutlet} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObColumnLayoutComponent, ObColumnLayoutModule, ObHttpApiInterceptorEvents, ObInputClearDirective} from '@oblique/oblique';
import {EntrySummary} from 'api/EntrySummary';
import {LanguageSummary} from 'api/LanguageSummary';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {SearchPage} from 'api/SearchPage';
import {SearchQuery} from 'api/SearchQuery';
import {combineLatest, EMPTY, merge, Observable, of, Subject} from 'rxjs';
import {catchError, debounceTime, delay, distinctUntilChanged, filter, first, map, switchMap, takeUntil, tap} from 'rxjs/operators';
import {NewsItemBlockComponent} from '../../administration/news/news-item-block/news-item-block.component';
import {NewsService} from '../../administration/news/news.service';
import {AuthService} from '../../auth/auth.service';
import {EntryService} from '../../entry/entry.service';
import {LOCAL_STORAGE, LOCATION, SESSION_STORAGE} from '../../shared/injection.tokens';
import {LanguageService} from '../../shared/language.service';
import {PageSizeService} from '../../shared/page-size.service';
import {Pagination} from '../../shared/Pagination';
import {createDefaultPagination, mapToPagination, mapToPagingParameters} from '../../shared/PaginationMapper';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../../watchlist/watchlist.service';
import {ExtendedSearchFiltersService} from '../extended-search-filters.service';
import {SearchResultCardComponent} from '../search-result-card/search-result-card.component';
import {SearchService} from '../search.service';
import {StorageKeys} from '../StorageKeys';
import {SearchFormExtendedComponent} from './search-form-extended/search-form-extended.component';
import {SearchFormService} from './search-form.service';

@Component({
    selector: 'td-search-form',
    templateUrl: './search-form.component.html',
    styleUrls: ['./search-form.component.scss'],
    imports: [
        NgClass,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatAutocompleteTrigger,
        ObInputClearDirective,
        MatIcon,
        MatAutocomplete,
        MatOption,
        MatSelect,
        SelectAllComponent,
        MatButton,
        ObButtonDirective,
        MatBadge,
        ObColumnLayoutModule,
        MatPaginator,
        MatIconButton,
        MatMenuTrigger,
        MatTooltip,
        MatMenu,
        MatMenuItem,
        RouterLink,
        SearchResultCardComponent,
        SearchFormExtendedComponent,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
        NewsItemBlockComponent,
    ],
})
export class SearchFormComponent implements OnInit, OnDestroy, AfterViewInit {
    hasSearched = signal(false);
    extendedSearchForm?: UntypedFormGroup;
    form: UntypedFormGroup;
    sourceLanguageControl: AbstractControl;
    targetLanguageControl: AbstractControl;
    isAdmin = false;
    isSuperUser = false;
    isSuperReader = false;
    isWriter = false;
    languageOptions: LanguageSummary[] = [];
    languageOptionValues: number[] = [];
    canCreate = false;
    isAuthenticated$!: Observable<boolean>;
    results: EntrySummary[] | undefined;
    currentEntry: EntrySummary | undefined;
    resultLength = 0;
    pageSizeOptions: number[] = [];
    pagination: Pagination;
    filteredRecommendations!: Observable<string[]>;
    suppressRecommendationPanel = signal(false);
    readonly newsItems = signal<NewsItemSummary[]>([]);

    @ViewChild(MatAutocompleteTrigger) recommendation!: MatAutocompleteTrigger;
    @ViewChild(ObColumnLayoutComponent) columnLayout?: ObColumnLayoutComponent;
    public enablePagination = true;
    public isLeftColumnVisible = true;

    private static readonly RECOMMENDATION_MIN_LENGTH = 4;
    private readonly unsubscribe = new Subject<void>();
    private readonly initComplete = new Subject<void>();
    private preselectedLanguageIds: number[] = [];

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly searchService: SearchService,
        private readonly entryService: EntryService,
        private readonly watchlistService: WatchlistService,
        private readonly languageService: LanguageService,
        private readonly authService: AuthService,
        private readonly router: Router,
        private readonly httpInterceptor: ObHttpApiInterceptorEvents,
        private readonly pageSizeService: PageSizeService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly searchFormService: SearchFormService,
        private readonly changeDetectorRef: ChangeDetectorRef,
        private readonly newsService: NewsService,
        private readonly translateService: TranslateService,
        private readonly destroyRef: DestroyRef,
        @Inject(DOCUMENT) private readonly document: Document,
        @Inject(SESSION_STORAGE) private readonly sessionStorage: Storage,
        @Inject(LOCAL_STORAGE) private readonly localStorage: Storage,
        @Inject(LOCATION) private readonly location: Location
    ) {
        this.extendedSearchForm = this.formBuilder.group({});
        this.form = this.formBuilder.group({
            phrase: null,
            sourceLanguage: null,
            targetLanguage: null,
            extendSearch: false,
            extendedSearchForm: this.extendedSearchForm,
        });
        this.sourceLanguageControl = this.form.controls.sourceLanguage;
        this.targetLanguageControl = this.form.controls.targetLanguage;
        this.pagination = this.getPaginationFromQueryParams();
    }

    ngOnInit(): void {
        this.loadNews();

        this.translateService.onLangChange.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.loadNews());

        combineLatest([this.authService.authCompleted$, this.initComplete, this.activatedRoute.queryParamMap])
            .pipe(
                map(([, , params]) =>
                    params.get('s') != null || params.get('sl') != null || params.get('tl') != null
                        ? {
                              phrase: params.get('s')?.trim() ?? '',
                              sourceLanguage: params.get('sl') ?? '',
                              targetLanguage: params.get('tl') ?? '',
                          }
                        : {
                              phrase: this.sessionStorage.getItem(StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY) ?? '',
                              sourceLanguage: this.localStorage.getItem('sourceLanguage') ?? '',
                              targetLanguage: this.localStorage.getItem('targetLanguage') ?? '',
                          }
                ),
                distinctUntilChanged((a, b) => a.phrase === b.phrase && a.sourceLanguage === b.sourceLanguage && a.targetLanguage === b.targetLanguage),
                filter(
                    params =>
                        this.form.controls.phrase.value !== params.phrase ||
                        this.form.controls.sourceLanguage.value !== params.sourceLanguage ||
                        this.form.controls.targetLanguage.value !== params.targetLanguage
                ),
                tap(params => {
                    this.form.controls.phrase.setValue(params.phrase, {emitEvent: false});
                    this.form.controls.sourceLanguage.setValue(
                        this.isNullOrEmpty(params.sourceLanguage) ? null : params.sourceLanguage.split(',').map(Number),
                        {
                            emitEvent: false,
                        }
                    );
                    this.form.controls.targetLanguage.setValue(
                        this.isNullOrEmpty(params.targetLanguage) ? null : params.targetLanguage.split(',').map(Number),
                        {
                            emitEvent: false,
                        }
                    );

                    this.changeDetectorRef.detectChanges();
                }),
                filter(() => this.sessionStorage.getItem(StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY) === 'true'),
                switchMap(() => {
                    return this.getSearchResult();
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe(page => {
                if (this.shouldOpenFirstEntry()) {
                    this.openFirstEntry(page);
                }
            });

        this.filteredRecommendations = merge(
            this.form.controls.sourceLanguage.valueChanges.pipe(switchMap(() => of([]))),
            this.form.controls.phrase.valueChanges.pipe(
                tap(() => this.suppressRecommendationPanel.set(false)),
                debounceTime(300),
                switchMap(value => {
                    const phrase = value?.toString() ?? '';
                    if (phrase.trim().length < SearchFormComponent.RECOMMENDATION_MIN_LENGTH) {
                        return of([]);
                    }
                    const sourceLanguage = this.form.controls.sourceLanguage.value;

                    this.httpInterceptor.deactivateSpinnerOnNextAPICalls(1);

                    return this.searchService.getRecommendation(phrase, sourceLanguage).pipe(catchError(() => of([])));
                })
            )
        ).pipe(takeUntil(this.unsubscribe));

        this.languageService.languageSummaries$
            .pipe(
                tap(languageOptions => {
                    this.languageOptions = languageOptions;
                    this.languageOptionValues = this.languageOptions.map(lo => lo.id);
                }),
                takeUntil(this.unsubscribe),
                // select-all component needs to get rendered in order for source & target controls events to register
                delay(25)
            )
            .subscribe(languageOptions => {
                if (!languageOptions?.length) {
                    return;
                }
                this.preselectedLanguageIds = languageOptions.filter(lc => lc.isUiLanguage).map(lc => lc.id);
                this.resetLanguages();
            });

        this.form.controls.sourceLanguage.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe(sourceLanguages => {
            this.searchService.changeSourceLanguages(sourceLanguages ?? []);
        });

        this.form.controls.targetLanguage.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe(targetLanguages => {
            this.searchService.changeTargetLanguages(targetLanguages ?? []);
        });

        this.entryService.entryDeleted$
            .pipe(
                switchMap(id => this.deleteFromSearchResult(id)),
                takeUntil(this.unsubscribe)
            )
            .subscribe();
        this.entryService.entryUpdated$
            .pipe(
                switchMap(id => this.updateFromSearchResult(id)),
                takeUntil(this.unsubscribe)
            )
            .subscribe();

        this.authService.roles$.pipe(takeUntil(this.unsubscribe)).subscribe(roles => {
            this.canCreate = roles.length > 0;
        });

        this.isAuthenticated$ = this.authService.isAuthenticated$;
        this.authService.isAdmin$.pipe(takeUntil(this.unsubscribe)).subscribe(isAdmin => (this.isAdmin = isAdmin));
        this.authService.isSuperUser$.pipe(takeUntil(this.unsubscribe)).subscribe(isSuperUser => (this.isSuperUser = isSuperUser));
        this.authService.isSuperReader$.pipe(takeUntil(this.unsubscribe)).subscribe(isSuperReader => (this.isSuperReader = isSuperReader));
        this.authService.isWriter$.pipe(takeUntil(this.unsubscribe)).subscribe(isWriter => (this.isWriter = isWriter));

        this.router.events
            .pipe(
                filter(event => event instanceof NavigationEnd),
                takeUntil(this.unsubscribe)
            )
            .subscribe(navigationEnd => {
                this.form.controls.extendSearch.patchValue(false);

                if (navigationEnd instanceof NavigationEnd && navigationEnd.url === '/search') {
                    this.results = [];
                    this.resultLength = 0;
                    if (this.isLeftColumnVisible) {
                        this.columnLayout?.toggleLeft();
                    }
                }
            });

        this.pageSizeService.pageSize$.pipe(takeUntil(this.unsubscribe)).subscribe(options => (this.pageSizeOptions = options));

        this.extendedSearchFilterService.resetExtendedSearch$.pipe(takeUntil(this.unsubscribe)).subscribe(() => this.resetExtendedSearch());
    }

    isNullOrEmpty(value: string | number | undefined | null): boolean {
        return value === undefined || value === null || value.toString().trim() === '';
    }

    isEntryRoute(): boolean {
        return this.location.pathname.startsWith('/search/entry');
    }

    ngAfterViewInit() {
        this.columnLayout?.toggleLeftIcon$.pipe(takeUntil(this.unsubscribe)).subscribe((direction: string) => {
            this.isLeftColumnVisible = direction === 'left';
        });

        this.columnLayout?.toggleLeftIcon$.pipe(first()).subscribe(() => {
            this.columnLayout?.toggleLeft();
        });

        this.initComplete.next();
    }

    ngOnDestroy(): void {
        this.initComplete.complete();
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    clear(fieldName: string) {
        this.form.get(fieldName)?.patchValue(null);
    }

    page(pageEvent: PageEvent): void {
        this.pagination = mapToPagination(pageEvent);
        this.sessionStorage.setItem('pageSize', this.pagination.pageSize.toString());
        this.getSearchResult().subscribe(() => {
            void this.router.navigate([], {
                relativeTo: this.activatedRoute,
                queryParamsHandling: 'merge',
                queryParams: this.getSearchQueryParams(),
            });
        });
    }

    search(): void {
        this.suppressRecommendationPanel.set(true);
        this.recommendation?.closePanel();
        this.searchObservable().subscribe();
    }

    onPhraseClick(): void {
        if (!this.suppressRecommendationPanel()) {
            return;
        }
        this.suppressRecommendationPanel.set(false);
        this.recommendation?.openPanel();
    }

    selectSearchEntry(entry: EntrySummary) {
        this.setCurrentEntry(entry);
        this.entryService.selectEntry(entry.id);
    }

    setCurrentEntry(entry?: EntrySummary) {
        this.currentEntry = entry;
    }

    resetExtendedSearch(): void {
        this.resetLanguages(true);
        this.clear('phrase');
        this.sessionStorage.removeItem(StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY);
        this.sessionStorage.removeItem(StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY);

        this.form.controls.extendSearch.patchValue(false);
        this.form.markAsPristine();
        this.form.markAsUntouched();

        void this.router.navigate(['search'], {queryParams: {}});
    }

    resetLanguages(hard = false): void {
        const sourceLanguageValue =
            this.sourceLanguageControl.value == null || hard ? this.preselectedLanguageIds : (this.sourceLanguageControl.value as number[]);
        const targetLanguageValue =
            this.targetLanguageControl.value == null || hard ? this.preselectedLanguageIds : (this.targetLanguageControl.value as number[]);
        this.sourceLanguageControl.setValue(sourceLanguageValue, {emitEvent: true});
        this.targetLanguageControl.setValue(targetLanguageValue, {emitEvent: true});
    }

    toggleExtendedSearch(): void {
        this.form.controls.extendSearch.setValue(!this.form.controls.extendSearch.value);
    }

    addAllEntriesToWatchlist(): void {
        this.watchlistService.add(...(this.results?.filter(y => y.canBeAddedToWatchList).map(e => e.id) ?? []));
    }

    removeAllEntriesFromWatchlist(): void {
        this.watchlistService.remove(...(this.results?.filter(y => y.canBeAddedToWatchList).map(e => e.id) ?? []));
    }

    trackByEntryId(index: number, entry: EntrySummary) {
        return entry.id;
    }

    private getSearchQueryParams(): Params {
        const defaultPagination = createDefaultPagination();
        const phrase = this.form.controls.phrase.value;
        const sourceLanguage = this.form.controls.sourceLanguage.value;
        const targetLanguage = this.form.controls.targetLanguage.value;

        const queryParams: {[k: string]: string | number | null} = {
            s: phrase,
            sl: sourceLanguage?.toString() ?? null,
            tl: targetLanguage?.toString() ?? null,
        };

        if (this.pagination.pageIndex !== defaultPagination.pageIndex) {
            queryParams.p = this.pagination.pageIndex;
        } else {
            queryParams.p = null;
        }

        if (this.pagination.pageSize !== defaultPagination.pageSize) {
            queryParams.c = this.pagination.pageSize;
        } else {
            queryParams.c = null;
        }

        return queryParams;
    }

    private searchObservable(): Observable<SearchPage> {
        this.recommendation?.closePanel();
        this.pagination.pageIndex = 0;

        return this.getSearchResult().pipe(
            tap(page => {
                this.openFirstEntry(page);
            })
        );
    }

    private openFirstEntry(page: SearchPage) {
        if (!page.searchEntries.length) {
            void this.router.navigate(['/search/entry'], {queryParams: this.getSearchQueryParams()});

            return;
        }

        const entryId = page.searchEntries[0].id;
        void this.router.navigate(['/search/entry', entryId], {queryParams: this.getSearchQueryParams()});
    }

    private getSearchResult(): Observable<SearchPage> {
        this.enablePagination = true;
        const phrase = this.form.controls.phrase.value;
        const sourceLanguage = this.form.controls.sourceLanguage.value;
        const targetLanguage = this.form.controls.targetLanguage.value;

        const extendedSearchValues = this.searchFormService.getSearchValues(
            this.extendedSearchForm?.value as SearchQuery,
            this.isAdmin,
            this.isSuperUser,
            this.isWriter,
            this.isSuperReader
        );

        this.sessionStorage.setItem(StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY, 'true');
        if (phrase != null) {
            this.sessionStorage.setItem(StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY, phrase);
        }

        if (sourceLanguage != null) {
            this.localStorage.setItem('sourceLanguage', sourceLanguage.toString());
        }

        if (targetLanguage != null) {
            this.localStorage.setItem('targetLanguage', targetLanguage.toString());
        }

        if (extendedSearchValues && this.extendedSearchForm?.dirty) {
            this.sessionStorage.setItem(StorageKeys.SEARCH_SESSION_STORAGE_KEY, JSON.stringify(extendedSearchValues));
        }

        if (extendedSearchValues?.entryIds) {
            if (this.isAdmin || this.isSuperUser) {
                this.pagination.pageSize = 1000;
            } else if (this.isWriter) {
                this.pagination.pageSize = 250;
            } else {
                this.pagination.pageSize = 100;
            }

            this.enablePagination = false;
        }

        return this.searchService.getSearchResult(phrase, mapToPagingParameters(this.pagination), sourceLanguage, targetLanguage, extendedSearchValues).pipe(
            tap(page => {
                this.hasSearched.set(true);
                if (!this.isLeftColumnVisible) {
                    this.columnLayout?.toggleLeft();
                }
                this.results = page.searchEntries;
                this.resultLength = page.totalEntryCount;

                if (page.searchEntries.length) {
                    const entry = page.searchEntries[0];
                    this.selectSearchEntry(entry);

                    // Resets scroll position on search and page change
                    this.document.querySelector('.result-list')?.scrollTo({top: 0});
                } else {
                    this.setCurrentEntry(undefined);
                }
            })
        );
    }

    private getPaginationFromQueryParams(): Pagination {
        const defaultPagination = createDefaultPagination();

        let pageIndex = parseInt(this.activatedRoute.snapshot.queryParamMap.get('p') ?? '', 10);

        if (this.activatedRoute.snapshot.queryParamMap.has('c')) {
            const pageSizeParam = this.activatedRoute.snapshot.queryParamMap.get('c');
            this.sessionStorage.setItem('pageSize', pageSizeParam!.toString());
        }
        let pageSize = parseInt(this.sessionStorage.getItem('pageSize') ?? '', 10);

        pageIndex = isNaN(pageIndex) ? defaultPagination.pageIndex : pageIndex;
        pageSize = isNaN(pageSize) ? defaultPagination.pageSize : pageSize;

        return {
            pageIndex,
            pageSize,
        };
    }

    private deleteFromSearchResult(entryId: number) {
        if (this.isAdmin) {
            return this.updateFromSearchResult(entryId);
        }
        return this.searchObservable();
    }

    private updateFromSearchResult(entryId: number) {
        return this.searchService.getSearchResult(`EntryId:${entryId}`, {pageIndex: 1, pageSize: 1}).pipe(
            switchMap(result => {
                if (result.searchEntries.length !== 1) {
                    return this.searchObservable();
                }

                const entry = result.searchEntries[0];
                this.setCurrentEntry(entry);
                this.replaceSearchEntry(entry);

                return EMPTY;
            })
        );
    }

    private replaceSearchEntry(entry: EntrySummary): void {
        const entryIndex = this.results?.findIndex(r => r.id === entry.id) ?? -1;

        if (entryIndex >= 0) {
            this.results![entryIndex] = entry;
        }
    }

    private shouldOpenFirstEntry(): boolean {
        const path = this.location.pathname;

        // Checks if current route is any of the following:
        // /search/entry/{:id}
        // /search/entry/{:id}/update
        // /search/entry/{:id}/copy
        // /search/entry/create
        const hasActiveRoute = path.match(/\/search\/entry\/([\d]+|create)/gi);

        return hasActiveRoute === null;
    }

    private loadNews(): void {
        this.newsService.getActiveNewsSummaries().subscribe(news => this.newsItems.set(news));
    }
}
