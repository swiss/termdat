import {AsyncPipe, NgClass} from '@angular/common';
import {EventEmitter, ProviderToken} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormControl, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatBadgeModule} from '@angular/material/badge';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatOption} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatMenuModule} from '@angular/material/menu';
import {MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, convertToParamMap, NavigationEnd, ParamMap, Router, RouterLink, RouterModule} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {
    ObButtonModule,
    ObColumnLayoutComponent,
    ObColumnLayoutModule,
    ObColumnPanelDirective,
    ObHttpApiInterceptorEvents,
    ObInputClearModule,
    WINDOW,
} from '@oblique/oblique';
import {EntrySummary} from 'api/EntrySummary';
import {LanguageSummary} from 'api/LanguageSummary';
import {SearchPage} from 'api/SearchPage';
import {Status} from 'api/Status';
import {MockComponent, MockDirectives, MockModule, MockPipes} from 'ng-mocks';
import {BehaviorSubject, of, Subject} from 'rxjs';
import {NewsService} from '../../administration/news/news.service';
import {AuthService} from '../../auth/auth.service';
import {Role} from '../../auth/Role';
import {EntryDetailComponent} from '../../entry/entry-detail-form/entry-detail/entry-detail.component';
import {EntryHeaderComponent} from '../../entry/entry-header/entry-header.component';
import {EntryService} from '../../entry/entry.service';
import {DOCUMENT, LOCAL_STORAGE, LOCATION, SESSION_STORAGE} from '../../shared/injection.tokens';
import {LanguageService} from '../../shared/language.service';
import {PageSizeService} from '../../shared/page-size.service';
import {mapToPagingParameters} from '../../shared/PaginationMapper';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../../watchlist/watchlist.service';
import {ExtendedSearchFiltersService} from '../extended-search-filters.service';
import {SearchResultCardComponent} from '../search-result-card/search-result-card.component';
import {SearchService} from '../search.service';
import {StorageKeys} from '../StorageKeys';
import {SearchFormExtendedComponent} from './search-form-extended/search-form-extended.component';
import {SearchFormComponent} from './search-form.component';

describe('SearchFormComponent', () => {
    let component: SearchFormComponent;
    let fixture: ComponentFixture<SearchFormComponent>;

    let formBuilder: FormBuilder;

    const phrase = 'suchen';
    const sourceLanguageIds = [1, 2, 3];
    const targetLanguageIds = [4, 5, 6];
    let languages: LanguageSummary[] = [];

    const phraseSession = 'suchenSession';
    const hasSearchedSession = 'true';
    const sourceLanguageIdsSession = [2, 3, 4];
    const targetLanguageIdsSession = [5, 6, 7];

    let searchResult: SearchPage;
    const recommendations = ['Test', 'Testvalue', 'Foo'];

    const languageFilterSubject = new Subject();
    const searchServiceMock = {
        getSearchResult: jest.fn(),
        getRecommendation: jest.fn(),
        changeSourceLanguages: jest.fn(),
        changeTargetLanguages: jest.fn(),
        getVisibleLanguages: jest.fn(),
        languageFilter$: languageFilterSubject.asObservable(),
    };

    const watchlistServiceMock = {
        add: jest.fn(),
        remove: jest.fn(),
    };

    const sessionStorageMock = {
        getItem: jest.fn(),
        setItem: jest.fn(),
        clear: jest.fn(),
        removeItem: jest.fn(),
    };

    const localStorageMock = {
        getItem: jest.fn(),
        setItem: jest.fn(),
        clear: jest.fn(),
    };

    const langChange$ = new Subject<{lang: string}>();
    const translateServiceMock = {
        onLangChange: langChange$.asObservable(),
    } as TranslateService;

    const newsServiceMock = {
        getActiveNewsSummaries: jest.fn().mockReturnValue(of([])),
    };

    const entryDeletedSubject = new Subject<number>();
    const entryUpdatedSubject = new Subject<number>();
    const entryServiceMock = {
        entryDeleted$: entryDeletedSubject.asObservable(),
        entryUpdated$: entryUpdatedSubject.asObservable(),
        selectEntry: jest.fn(),
    };

    const languageSubject = new Subject();
    const languageServiceMock = {
        getLanguageName: jest.fn(),
        languageSummaries$: languageSubject.asObservable(),
    };

    const routerEventsSubject = new Subject();
    const routerMock = {
        navigate: jest.fn(),
        events: routerEventsSubject,
        createUrlTree: jest.fn(),
    };

    const queryParamMap = new EventEmitter<ParamMap>();
    const activatedRouteMock = {
        snapshot: {
            queryParamMap: {
                get: jest.fn(),
                has: jest.fn(),
            },
        },
        queryParamMap,
    };

    const documentMock = {
        querySelector: jest.fn().mockReturnValue({scrollTo: jest.fn()}),
    };

    const roles: Role[] = [];
    const rolesSubject = new BehaviorSubject(roles);
    const isAdminSubject = new BehaviorSubject(false);
    const isSuperUserSubject = new BehaviorSubject(false);
    const isSuperReaderSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);
    const isAuthCompletetSubject = new BehaviorSubject(true);
    const authServiceMock = {
        roles$: rolesSubject.asObservable(),
        isAdmin$: isAdminSubject.asObservable(),
        isSuperUser$: isSuperUserSubject.asObservable(),
        isSuperReader$: isSuperReaderSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
        authCompleted$: isAuthCompletetSubject.asObservable(),
    };

    const httpApiInterceptorMock = {
        deactivateSpinnerOnNextAPICalls: jest.fn(),
    };

    const pageSizeSubject = new BehaviorSubject([25, 50]);
    const pageSizeServiceMock = {
        pageSize$: pageSizeSubject.asObservable(),
    };

    const toggleLeftIconSubject = new BehaviorSubject('right');
    const columnLayoutMock = {
        left: true,
        right: false,
        toggleLeft: jest.fn(),
        toggleLeftIcon$: toggleLeftIconSubject.asObservable(),
    };

    const resetExtendedSearchSubject = new Subject<void>();
    const extendedSearchFiltersServiceMock = {
        resetExtendedSearch$: resetExtendedSearchSubject,
    };
    const locationMock = {
        pathname: '/search/entry',
    };

    class ResizeObserver {
        constructor() {}
        observe(): void {}
        unobserve(): void {}
        disconnect(): void {}
    }
    window.ResizeObserver = ResizeObserver;

    beforeAll(() => {
        window.HTMLElement.prototype.scrollTo = jest.fn();
    });
    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatAutocompleteModule),
                MockModule(MatCheckboxModule),
                MockModule(MatBadgeModule),
                MockModule(MatButtonModule),
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                MockModule(MatInputModule),
                MockModule(MatMenuModule),
                MockModule(MatPaginatorModule),
                MockModule(MatSelectModule),
                MockModule(MatTooltipModule),
                MockModule(ObButtonModule),
                MockModule(ObInputClearModule),
                MockModule(ObColumnLayoutModule),
                ObColumnLayoutComponent,
                ObColumnPanelDirective,
                ReactiveFormsModule,
                RouterModule,
                SearchFormComponent,
                MockPipes(TranslatePipe),
                MockComponent(SearchResultCardComponent),
                MockComponent(EntryDetailComponent),
                MockComponent(EntryHeaderComponent),
                MockComponent(SearchFormExtendedComponent),
                MockComponent(SelectAllComponent),
                MockDirectives(RouterLink),
                NgClass,
                AsyncPipe,
            ],
            providers: [
                {provide: LOCAL_STORAGE, useValue: localStorageMock},
                {provide: SESSION_STORAGE, useValue: sessionStorageMock},
                {provide: SearchService, useValue: searchServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorMock},
                {provide: PageSizeService, useValue: pageSizeServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: NewsService, useValue: newsServiceMock},
                {provide: DOCUMENT, useValue: documentMock},
                {provide: LOCATION, useValue: locationMock},
                {provide: WINDOW, useValue: window},
            ],
        }).compileComponents();

        languages = [
            {
                id: 1,
                name: 'Deutsch',
                isUiLanguage: true,
                sortIndex: 1,
            } as LanguageSummary,
            {
                id: 2,
                name: 'Englisch',
                isUiLanguage: true,
                sortIndex: 2,
            } as LanguageSummary,
        ];
        searchResult = {
            totalEntryCount: 99,
            searchEntries: [
                {
                    id: 1,
                    status: Status.Validated,
                    deleted: false,
                    collection: {
                        id: 1,
                        code: 'COL',
                        name: 'Collection',
                        isObsolete: false,
                        deleted: false,
                        active: true,
                        activeApi: false,
                        exportLindas: false,
                    },
                    terms: [
                        {
                            languageId: 1,
                            terminus: 'Wert',
                            sequence: 1,
                        },
                        {
                            languageId: 2,
                            terminus: 'Value',
                            sequence: 2,
                        },
                    ],
                    isMainOffice: true,
                    canBeAddedToWatchList: true,
                },
            ],
        };

        sessionStorageMock.getItem.mockReset();
        sessionStorageMock.getItem.mockImplementation(key => {
            const store: Record<string, string> = {
                [StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY]: phraseSession,
                [StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY]: hasSearchedSession,
            };
            return store[key] ?? null;
        });

        localStorageMock.getItem.mockImplementation(x =>
            x === 'sourceLanguage' ? sourceLanguageIdsSession.toString() : x === 'targetLanguage' ? targetLanguageIdsSession.toString() : ''
        );

        languageServiceMock.getLanguageName.mockImplementation(
            languageId => languages.find(languageTranslation => languageTranslation.id === languageId)?.name
        );
        searchServiceMock.getSearchResult.mockReturnValue(of(searchResult));
        searchServiceMock.getRecommendation.mockReturnValue(of(recommendations));
        searchServiceMock.getVisibleLanguages.mockReturnValue([]);

        newsServiceMock.getActiveNewsSummaries.mockReturnValue(of([]));

        formBuilder = TestBed.inject(FormBuilder);
        fixture = TestBed.createComponent(SearchFormComponent);
        component = fixture.componentInstance;

        component.columnLayout = columnLayoutMock as unknown as ObColumnLayoutComponent;
        activatedRouteMock.snapshot.queryParamMap.has.mockReturnValue(false);

        jest.useFakeTimers();
        fixture.detectChanges();
        languageSubject.next(languages);
        jest.clearAllTimers();
    });

    afterEach(() => {
        component.ngOnDestroy();
        jest.resetAllMocks();
        jest.useRealTimers();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize properly', () => {
        expect(component.pagination.pageSize).toStrictEqual(25);
        expect(component.languageOptions).toStrictEqual(languages);
        expect(component.languageOptionValues).toEqual([1, 2]);
        expect(component.sourceLanguageControl).toBeInstanceOf(FormControl);
        expect(component.targetLanguageControl).toBeInstanceOf(FormControl);
        expect(component.form).toBeInstanceOf(FormGroup);
    });

    it('should clear a field', () => {
        component.form.get('phrase')?.patchValue('notNull');
        expect(component.form.get('phrase')?.value).not.toBeNull();
        component.clear('phrase');
        expect(component.form.get('phrase')?.value).toBeNull();
    });

    it('should search for a phrase', () => {
        component.form.get('phrase')?.patchValue(phrase);
        component.form.get('targetLanguage')?.patchValue(targetLanguageIds);
        component.form.get('sourceLanguage')?.patchValue(sourceLanguageIds);

        component.search();

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(
            phrase,
            mapToPagingParameters(component.pagination),
            sourceLanguageIds,
            targetLanguageIds,
            {
                adminInfos: [],
                adminInfosExclude: false,
                adminInfosPriority: false,
                collectionsExclude: false,
                commentStatus: [],
                createdBy: undefined,
                createdByExclude: false,
                createdByPriority: false,
                createdByUser: false,
                deleted: false,
                deletedPriority: false,
                glossariesExclude: false,
                hasMissingSource: false,
                lenochClassifications: [],
                lenochClassificationsPriority: false,
                modifiedBy: undefined,
                modifiedByExclude: false,
                modifiedByPriority: false,
                modifiedByUser: false,
                modifiedFrom: undefined,
                modifiedPeriodExclude: false,
                modifiedPeriodPriority: false,
                modifiedTo: undefined,
                reliability: [],
                reliabilityPriority: false,
                relatedCollections: [],
            }
        );
        expect(component.results).toEqual(searchResult.searchEntries);
        expect(component.currentEntry).toEqual(searchResult.searchEntries[0]);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledWith(1);
    });

    it('should save search parameters in storage', () => {
        queryParamMap.emit(convertToParamMap({s: null, sl: null, tl: null}));

        queryParamMap.emit(convertToParamMap({s: phraseSession, sl: sourceLanguageIdsSession.toString(), tl: targetLanguageIdsSession.toString()}));

        expect(sessionStorageMock.setItem).toHaveBeenNthCalledWith(1, StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY, hasSearchedSession);
        expect(sessionStorageMock.setItem).toHaveBeenNthCalledWith(2, StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY, phraseSession);
        expect(localStorageMock.setItem).toHaveBeenCalledWith('sourceLanguage', sourceLanguageIdsSession.toString());
        expect(localStorageMock.setItem).toHaveBeenCalledWith('targetLanguage', targetLanguageIdsSession.toString());
    });

    it('should set target- and sourcelanguage on first page load', () => {
        queryParamMap.emit(convertToParamMap({s: null, sl: null, tl: null}));

        expect(localStorageMock.getItem).toHaveBeenCalledWith('sourceLanguage');
        expect(localStorageMock.getItem).toHaveBeenCalledWith('targetLanguage');
    });

    it('should search for search parameters in session storage', () => {
        queryParamMap.emit(convertToParamMap({s: null, sl: null, tl: null}));

        expect(sessionStorageMock.getItem).toHaveBeenCalledWith(StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY);
        expect(sessionStorageMock.getItem).toHaveBeenCalledWith(StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY);
        expect(localStorageMock.getItem).toHaveBeenCalledWith('sourceLanguage');
        expect(localStorageMock.getItem).toHaveBeenCalledWith('targetLanguage');

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(
            phraseSession,
            {pageIndex: 1, pageSize: 25},
            sourceLanguageIdsSession,
            targetLanguageIdsSession,
            {
                adminInfos: [],
                adminInfosExclude: false,
                adminInfosPriority: false,
                collectionsExclude: false,
                commentStatus: [],
                createdBy: undefined,
                createdByExclude: false,
                createdByPriority: false,
                createdByUser: false,
                deleted: false,
                deletedPriority: false,
                glossariesExclude: false,
                hasMissingSource: false,
                lenochClassifications: [],
                lenochClassificationsPriority: false,
                modifiedBy: undefined,
                modifiedByExclude: false,
                modifiedByPriority: false,
                modifiedByUser: false,
                modifiedFrom: undefined,
                modifiedPeriodExclude: false,
                modifiedPeriodPriority: false,
                modifiedTo: undefined,
                reliability: [],
                reliabilityPriority: false,
                relatedCollections: [],
            }
        );
    });

    it('should toggle layout if left column is closed', () => {
        component.columnLayout = columnLayoutMock as unknown as ObColumnLayoutComponent;

        // eslint-disable-next-line dot-notation
        component['isLeftColumnVisible'] = false;

        component.form.get('phrase')?.patchValue(phrase);
        component.form.get('targetLanguage')?.patchValue(targetLanguageIds);
        component.form.get('sourceLanguage')?.patchValue(sourceLanguageIds);

        component.search();

        expect(columnLayoutMock.toggleLeft).toHaveBeenCalledTimes(1);
    });

    it('should not toggle layout if left column already open', () => {
        component.columnLayout = columnLayoutMock as unknown as ObColumnLayoutComponent;

        component.form.get('phrase')?.patchValue(phrase);
        component.form.get('targetLanguage')?.patchValue(targetLanguageIds);
        component.form.get('sourceLanguage')?.patchValue(sourceLanguageIds);
        component.isLeftColumnVisible = true;

        component.search();

        expect(columnLayoutMock.toggleLeft).toHaveBeenCalledTimes(0);
    });

    it('should search for a phrase in all fields when fields is dirty', () => {
        component.extendedSearchForm?.setControl('fields', formBuilder.group({}));

        component.form.get('extendedSearchForm.fields')?.markAsDirty();

        component.search();

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(null, mapToPagingParameters(component.pagination), null, null, {
            fields: {
                comment: false,
            },
            adminInfos: [],
            adminInfosExclude: false,
            adminInfosPriority: false,
            collectionsExclude: false,
            commentStatus: [],
            createdBy: undefined,
            createdByExclude: false,
            createdByPriority: false,
            createdByUser: false,
            deleted: false,
            deletedPriority: false,
            glossariesExclude: false,
            hasMissingSource: false,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            modifiedBy: undefined,
            modifiedByExclude: false,
            modifiedByPriority: false,
            modifiedByUser: false,
            modifiedFrom: undefined,
            modifiedPeriodExclude: false,
            modifiedPeriodPriority: false,
            modifiedTo: undefined,
            reliability: [],
            reliabilityPriority: false,
            relatedCollections: [],
        });
        expect(component.results).toEqual(searchResult.searchEntries);
    });

    it('should page', () => {
        const pageEvent: PageEvent = {
            pageSize: 33,
            pageIndex: 66,
            length: component.resultLength,
        };
        component.page(pageEvent);

        expect(component.pagination?.pageIndex).toBe(pageEvent.pageIndex);
        expect(component.pagination?.pageSize).toBe(pageEvent.pageSize);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledWith(1);
    });

    it('should apply term from query params and perform search', () => {
        expect(component.form.controls.phrase.value).toEqual(null);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(0);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(0);

        queryParamMap.emit(convertToParamMap({s: 'test'}));

        expect(component.form.controls.phrase.value).toEqual('test');
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith('test', {pageIndex: 1, pageSize: 25}, null, null, expect.anything());
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(0);

        queryParamMap.emit(convertToParamMap({s: 'test'}));

        expect(component.form.controls.phrase.value).toEqual('test');
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(0);
    });

    it('should apply term from query params and perform search with different values', () => {
        expect(component.form.controls.phrase.value).toEqual(null);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(0);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(0);

        queryParamMap.emit(convertToParamMap({s: 'abc123'}));

        expect(component.form.controls.phrase.value).toEqual('abc123');
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith('abc123', {pageIndex: 1, pageSize: 25}, null, null, expect.anything());
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(0);
    });

    it('should add term and language Ids to query params', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'test', p: null, c: null, sl: '1,2', tl: '1,2'},
        });

        component.form.controls.phrase.setValue('abc123');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'abc123', p: null, c: null, sl: '1,2', tl: '1,2'},
        });
    });

    it('should add pagination to query params', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.page({pageIndex: 1, pageSize: 100} as PageEvent);

        expect(routerMock.navigate).toHaveBeenCalledWith([], {
            relativeTo: activatedRouteMock,
            queryParamsHandling: 'merge',
            queryParams: {s: 'test', p: 1, c: 100, sl: '1,2', tl: '1,2'},
        });
    });

    it('should add term to query params', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue(null);
        component.form.controls.targetLanguage.setValue(null);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'test', p: null, c: null, sl: null, tl: null},
        });
    });

    it('should add term and sourcelanguageId to query params', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue(null);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'test', p: null, c: null, sl: '1,2', tl: null},
        });
    });

    it('should add term and targetlanguageId to query params', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue(null);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'test', p: null, c: null, sl: null, tl: '1,2'},
        });
    });

    it('should remove pagination from query params if values are default', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');

        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);
        component.page({pageIndex: 0, pageSize: 100} as PageEvent);

        expect(routerMock.navigate).toHaveBeenCalledWith([], {
            relativeTo: activatedRouteMock,
            queryParamsHandling: 'merge',
            queryParams: {s: 'test', p: null, c: 100, sl: '1,2', tl: '1,2'},
        });

        component.page({pageIndex: 1, pageSize: 25} as PageEvent);

        expect(routerMock.navigate).toHaveBeenCalledWith([], {
            relativeTo: activatedRouteMock,
            queryParamsHandling: 'merge',
            queryParams: {s: 'test', p: 1, c: null, sl: '1,2', tl: '1,2'},
        });

        component.page({pageIndex: 0, pageSize: 25} as PageEvent);

        expect(routerMock.navigate).toHaveBeenCalledWith([], {
            relativeTo: activatedRouteMock,
            queryParamsHandling: 'merge',
            queryParams: {s: 'test', p: null, c: null, sl: '1,2', tl: '1,2'},
        });
    });

    it('should apply entry id from query params and select entry correctly', () => {
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue(null);
        component.form.controls.targetLanguage.setValue(null);

        component.search();

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith('test', {pageIndex: 1, pageSize: 25}, null, null, expect.anything());
        expect(routerMock.navigate);
    });

    it('should add pagination to query params when searching', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.page({pageIndex: 1, pageSize: 100} as PageEvent);

        expect(routerMock.navigate).toHaveBeenCalledWith([], {
            relativeTo: activatedRouteMock,
            queryParamsHandling: 'merge',
            queryParams: {s: 'test', p: 1, c: 100, sl: '1,2', tl: '1,2'},
        });
    });

    it('should add entry id to query params when selecting entry', () => {
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1], {
            queryParams: {s: 'test', p: null, c: null, sl: '1,2', tl: '1,2'},
        });
    });

    it('should remove entry id from query params if there are no entries found', () => {
        searchServiceMock.getSearchResult.mockReturnValue(of({totalEntryCount: 0, searchEntries: []} as SearchPage));

        expect(routerMock.navigate).toHaveBeenCalledTimes(0);

        component.form.controls.phrase.setValue('test');
        component.form.controls.sourceLanguage.setValue([1, 2]);
        component.form.controls.targetLanguage.setValue([1, 2]);

        component.search();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry'], {
            queryParams: {s: 'test', p: null, c: null, sl: '1,2', tl: '1,2'},
        });
    });

    it('should get pagination from query params', () => {
        expect(component.pagination.pageIndex).toEqual(0);
        expect(component.pagination.pageSize).toEqual(25);

        activatedRouteMock.snapshot.queryParamMap.has.mockReturnValue(true);
        activatedRouteMock.snapshot.queryParamMap.get.mockImplementation((arg: string) => (arg === 'p' ? 2 : 200));
        sessionStorageMock.getItem.mockReturnValue('200');

        fixture = TestBed.createComponent(SearchFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        expect(component.pagination.pageIndex).toEqual(2);
        expect(sessionStorageMock.setItem).toHaveBeenCalledWith('pageSize', '200');
        expect(component.pagination.pageSize).toEqual(200);
    });

    it('should create extended search form', () => {
        expect(component.extendedSearchForm).toBeDefined();
        expect(component.extendedSearchForm?.pristine).toBeTruthy();
        expect(component.extendedSearchForm?.touched).toBeFalsy();
    });

    it('should reset extended search form', () => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (component as any).preselectedLanguageIds = [1, 2];

        component.extendedSearchForm?.setControl('test', formBuilder.control('test'));
        component.extendedSearchForm?.setControl('fields', formBuilder.group({}));

        component.form.get('phrase')?.patchValue(phrase);
        component.form.get('targetLanguage')?.patchValue(targetLanguageIds);
        component.form.get('sourceLanguage')?.patchValue(sourceLanguageIds);
        component.form.get('extendSearch')?.patchValue(true);
        component.form.markAsDirty();
        component.form.markAsTouched();

        expect(component.form.value).toEqual({
            extendSearch: true,
            extendedSearchForm: {
                fields: {},
                test: 'test',
            },
            phrase: 'suchen',
            sourceLanguage: [1, 2, 3],
            targetLanguage: [4, 5, 6],
        });
        expect(component.form.touched).toEqual(true);
        expect(component.form.dirty).toEqual(true);

        component.resetExtendedSearch();

        expect(component.form.value).toEqual({
            extendSearch: false,
            extendedSearchForm: {
                fields: {},
                test: 'test',
            },
            phrase: null,
            sourceLanguage: [1, 2],
            targetLanguage: [1, 2],
        });
        expect(component.form.touched).toEqual(false);
        expect(component.form.dirty).toEqual(false);
        expect(sessionStorageMock.removeItem).toHaveBeenCalledWith(StorageKeys.SEARCHPHRASE_SESSION_STORAGE_KEY);
        expect(sessionStorageMock.removeItem).toHaveBeenCalledWith(StorageKeys.HASSEARCHED_SESSION_STORAGE_KEY);
    });

    it('should toggle extended search', () => {
        const extendSearchValue = component.form.controls.extendSearch.value;
        component.toggleExtendedSearch();
        expect(component.form.controls.extendSearch.value).toBe(!extendSearchValue);
    });

    it('should delete entry without admin rights', () => {
        isAdminSubject.next(false);
        const searchPhrase = 'test';
        component.form.get('phrase')?.patchValue(searchPhrase);
        entryDeletedSubject.next(1);

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        const call = searchServiceMock.getSearchResult.mock.calls[0];

        expect(call[0]).toBe(searchPhrase);
        expect(call[1]).toEqual({pageIndex: 1, pageSize: 25});
        expect(call[4]).toEqual(
            expect.objectContaining({
                adminInfos: [],
            })
        );
    });

    it('should update entry from search when deleted as admin', () => {
        isAdminSubject.next(true);
        component.results = searchResult.searchEntries;
        component.currentEntry = searchResult.searchEntries[0];
        component.form.get('phrase')?.patchValue('test');

        const entryId = searchResult.searchEntries[0].id;
        searchServiceMock.getSearchResult.mockReturnValue(
            of({
                searchEntries: [
                    {
                        id: 1,
                        status: Status.Validated,
                        deleted: true,
                    },
                ],
            })
        );
        entryDeletedSubject.next(entryId);

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(`EntryId:${entryId}`, {pageIndex: 1, pageSize: 1});
        expect(component.results[0].deleted).toEqual(true);
    });

    it('should search with phrase when deleted as admin and when no results for entry specific returned', () => {
        isAdminSubject.next(true);
        component.results = searchResult.searchEntries;
        component.currentEntry = searchResult.searchEntries[0];
        const searchPhrase = 'test';
        component.form.get('phrase')?.patchValue(searchPhrase);

        const entryId = searchResult.searchEntries[0].id;
        searchServiceMock.getSearchResult.mockReturnValue(
            of({
                searchEntries: [],
            })
        );
        entryDeletedSubject.next(entryId);

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(2);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(`EntryId:${entryId}`, {pageIndex: 1, pageSize: 1});
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(searchPhrase, {pageIndex: 1, pageSize: 25}, null, null, expect.anything());
    });

    it('should update entry from search when updated', () => {
        component.results = searchResult.searchEntries;
        component.currentEntry = searchResult.searchEntries[0];
        component.form.get('phrase')?.patchValue('test');

        const entryId = searchResult.searchEntries[0].id;
        searchServiceMock.getSearchResult.mockReturnValue(
            of({
                searchEntries: [
                    {
                        id: 1,
                        status: Status.InProgress,
                    },
                ],
            })
        );
        entryUpdatedSubject.next(entryId);

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(`EntryId:${entryId}`, {pageIndex: 1, pageSize: 1});
        expect(component.results[0].status).toEqual(Status.InProgress);
    });

    it('should search with phrase when updated and when no results for entry specific returned', () => {
        component.results = searchResult.searchEntries;
        component.currentEntry = searchResult.searchEntries[0];
        const searchPhrase = 'test';
        component.form.get('phrase')?.patchValue(searchPhrase);

        const entryId = searchResult.searchEntries[0].id;
        searchServiceMock.getSearchResult.mockReturnValue(
            of({
                searchEntries: [],
            })
        );
        entryUpdatedSubject.next(entryId);

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(2);

        const calls = searchServiceMock.getSearchResult.mock.calls;
        expect(calls[0][0]).toBe(`EntryId:${entryId}`);
        expect(calls[0][1]).toEqual({pageIndex: 1, pageSize: 1});
        expect(calls[1][0]).toBe(searchPhrase);
        expect(calls[1][1]).toEqual({pageIndex: 1, pageSize: 25});
    });

    it('should change source languages on search service when values change', () => {
        searchServiceMock.changeSourceLanguages.mockClear();
        searchServiceMock.changeTargetLanguages.mockClear();
        component.form.controls.sourceLanguage.setValue([1, 2]);

        expect(searchServiceMock.changeSourceLanguages).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.changeSourceLanguages).toHaveBeenCalledWith([1, 2]);
        expect(searchServiceMock.changeTargetLanguages).toHaveBeenCalledTimes(0);

        component.form.controls.sourceLanguage.setValue(null);
        expect(searchServiceMock.changeSourceLanguages).toHaveBeenLastCalledWith([]);
    });

    it('should change target languages on search service when values change', () => {
        searchServiceMock.changeSourceLanguages.mockClear();
        searchServiceMock.changeTargetLanguages.mockClear();
        component.form.controls.targetLanguage.setValue([1, 2]);

        expect(searchServiceMock.changeTargetLanguages).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.changeTargetLanguages).toHaveBeenCalledWith([1, 2]);
        expect(searchServiceMock.changeSourceLanguages).toHaveBeenCalledTimes(0);

        component.form.controls.targetLanguage.setValue(null);
        expect(searchServiceMock.changeTargetLanguages).toHaveBeenLastCalledWith([]);
    });

    it('can get recommendations based on search input', () => {
        component.form.controls.phrase.setValue('test');

        jest.advanceTimersByTime(400);
        fixture.detectChanges();

        const autoComplete = fixture.debugElement.query(By.directive(MatAutocomplete));
        let options = autoComplete.queryAll(By.directive(MatOption));
        expect(options.length).toEqual(3);

        expect(httpApiInterceptorMock.deactivateSpinnerOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledWith('test', null);

        component.form.controls.phrase.setValue('   tes     ');

        jest.advanceTimersByTime(400);
        fixture.detectChanges();

        options = autoComplete.queryAll(By.directive(MatOption));
        expect(options.length).toEqual(0);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(1);
    });

    it('uses sourceLanguage for recommendations when set', () => {
        component.form.controls.sourceLanguage.setValue([1, 2, 3]);
        component.form.controls.phrase.setValue('test');

        jest.advanceTimersByTime(400);

        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledWith('test', [1, 2, 3]);
    });

    it('re-enables recommendations when typing after a search', () => {
        component.form.controls.phrase.setValue('test');
        jest.advanceTimersByTime(400);

        component.search();

        expect(component.suppressRecommendationPanel()).toBe(true);

        component.form.controls.phrase.setValue('test2');
        jest.advanceTimersByTime(400);

        expect(component.suppressRecommendationPanel()).toBe(false);
        expect(searchServiceMock.getRecommendation).toHaveBeenCalledTimes(2);
        expect(searchServiceMock.getRecommendation).toHaveBeenLastCalledWith('test2', null);
    });

    it('deactivates extended search when navigation start is used', () => {
        component.form.controls.extendSearch.setValue(true);

        routerEventsSubject.next(new NavigationEnd(1, '/test', '/test/new'));

        expect(component.form.controls.extendSearch.value).toEqual(false);
    });

    it('resets site when navigation end at /search', () => {
        component.columnLayout = columnLayoutMock as unknown as ObColumnLayoutComponent;

        component.results = [
            {id: 1} as unknown as EntrySummary,
            {id: 2} as unknown as EntrySummary,
            {id: 3} as unknown as EntrySummary,
            {id: 4} as unknown as EntrySummary,
        ];
        component.resultLength = 4;
        // eslint-disable-next-line dot-notation
        component['isLeftColumnVisible'] = true;

        routerEventsSubject.next(new NavigationEnd(1, '/search', '/search'));

        expect(component.results).toEqual([]);
        expect(component.resultLength).toEqual(0);
        expect(columnLayoutMock.toggleLeft).toHaveBeenCalledTimes(1);
    });

    it('should add all entries to watchlist', () => {
        const result = [
            {
                id: 1,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: true,
            },
            {
                id: 9999990,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: true,
            },
            {
                id: 9999991,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: false,
            },
        ];
        component.results = result;

        component.addAllEntriesToWatchlist();

        expect(watchlistServiceMock.add).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.add).toHaveBeenCalledWith(1, 9999990);
    });

    it('should remove all entries from watchlist', () => {
        const result = [
            {
                id: 3,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: true,
            },
            {
                id: 1234567890,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: true,
            },
            {
                id: 1234567891,
                status: Status.Validated,
                deleted: false,
                collection: {
                    id: 1,
                    code: 'COL',
                    name: 'Collection',
                    isObsolete: false,
                    deleted: false,
                    active: true,
                    activeApi: false,
                    exportLindas: false,
                },
                terms: [],
                isMainOffice: true,
                canBeAddedToWatchList: false,
            },
        ];
        component.results = result;

        component.removeAllEntriesFromWatchlist();

        expect(watchlistServiceMock.remove).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.remove).toHaveBeenCalledWith(3, 1234567890);
    });

    it('should not throw on watchlist if result is missing', () => {
        component.results = undefined;

        component.addAllEntriesToWatchlist();
        component.removeAllEntriesFromWatchlist();

        expect(watchlistServiceMock.add).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.add).toHaveBeenCalledWith();
        expect(watchlistServiceMock.remove).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.remove).toHaveBeenCalledWith();
    });

    it('should set preselected language ids to source and target language', () => {
        languages.push({id: 3, sortIndex: 3, name: 'ui 1', isUiLanguage: true} as LanguageSummary);
        languages.push({id: 4, sortIndex: 4, name: 'ui 2', isUiLanguage: true} as LanguageSummary);
        languages.push({id: 5, sortIndex: 5, name: 'none ui 1', isUiLanguage: false} as LanguageSummary);
        languageSubject.next(languages);

        jest.advanceTimersByTime(25);

        expect(searchServiceMock.changeSourceLanguages).toHaveBeenLastCalledWith([1, 2, 3, 4]);
        expect(searchServiceMock.changeTargetLanguages).toHaveBeenLastCalledWith([1, 2, 3, 4]);
        expect(component.sourceLanguageControl.value).toEqual([1, 2, 3, 4]);
        expect(component.targetLanguageControl.value).toEqual([1, 2, 3, 4]);
    });

    it('should keep selected language on language changes ids to source and target language', () => {
        component.sourceLanguageControl.setValue([5]);
        component.targetLanguageControl.setValue([4]);

        languages.push({id: 3, sortIndex: 3, name: 'ui 1', isUiLanguage: true} as LanguageSummary);
        languages.push({id: 4, sortIndex: 4, name: 'ui 2', isUiLanguage: true} as LanguageSummary);
        languages.push({id: 5, sortIndex: 5, name: 'none ui 1', isUiLanguage: false} as LanguageSummary);
        languageSubject.next(languages);

        expect(searchServiceMock.changeSourceLanguages).toHaveBeenLastCalledWith([5]);
        expect(searchServiceMock.changeTargetLanguages).toHaveBeenLastCalledWith([4]);
        expect(component.sourceLanguageControl.value).toEqual([5]);
        expect(component.targetLanguageControl.value).toEqual([4]);
    });

    it('should not set selected languages when no languages are provided', () => {
        component.sourceLanguageControl.setValue(null);
        component.targetLanguageControl.setValue(null);

        languageSubject.next([]);

        expect(component.sourceLanguageControl.value).toEqual(null);
        expect(component.targetLanguageControl.value).toEqual(null);
    });

    it('sets page size', () => {
        expect(component.pageSizeOptions).toEqual([25, 50]);
    });

    it('updates page size when pageSize$ emits', () => {
        expect(component.pageSizeOptions).toEqual([25, 50]);

        pageSizeSubject.next([15, 30, 45]);

        expect(component.pageSizeOptions).toEqual([15, 30, 45]);
    });

    it('should search for domains and subjects correctly', () => {
        component.form.get('phrase')?.patchValue(phrase);
        component.form.get('targetLanguage')?.patchValue(targetLanguageIds);
        component.form.get('sourceLanguage')?.patchValue(sourceLanguageIds);
        component.extendedSearchForm!.setControl('subjectDomains', formBuilder.control(['1', '1-2', '2-3', '2-4', '3']));
        component.extendedSearchForm!.setControl('subjectDomainsPriority', formBuilder.control(true));

        component.search();

        expect(searchServiceMock.getSearchResult).toHaveBeenCalledTimes(1);
        expect(searchServiceMock.getSearchResult).toHaveBeenCalledWith(
            phrase,
            mapToPagingParameters(component.pagination),
            sourceLanguageIds,
            targetLanguageIds,
            {
                subjectDomains: ['1', '1-2', '2-3', '2-4', '3'],
                subjectDomainsPriority: true,
                createdByUser: false,
                modifiedByUser: false,
            }
        );
        expect(component.results).toEqual(searchResult.searchEntries);
        expect(component.currentEntry).toEqual(searchResult.searchEntries[0]);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.selectEntry).toHaveBeenCalledWith(1);
    });

    it('should save pagesize to sessionstorage if passed by query', () => {
        activatedRouteMock.snapshot.queryParamMap.has.mockReturnValue(true);
        activatedRouteMock.snapshot.queryParamMap.get.mockReturnValue('666');

        fixture = TestBed.createComponent(SearchFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        expect(sessionStorageMock.setItem).toHaveBeenCalledWith('pageSize', '666');
    });

    it('should save pagesize to sessionstore on page-event', () => {
        component.page({pageSize: 666} as PageEvent);

        expect(sessionStorageMock.setItem).toHaveBeenCalledWith('pageSize', '666');
    });

    it.each([
        ['/search/entry/37413', 0],
        ['/search/entry/37413/update', 0],
        ['/search/entry/37413/copy', 0],
        ['/search/entry', 1],
        ['/search', 1],
        ['/', 1],
        ['/search/entry/create', 0],
        ['/search/entry/37413/', 0],
        ['/search/entry/37413/update/', 0],
        ['/search/entry/37413/copy/', 0],
        ['/search/entry/', 1],
        ['/search/entry/34/', 0],
        ['/search/entry/4/update/', 0],
        ['/search/entry/34/copy/', 0],
        ['/search/entry/', 1],
        ['/search/', 1],
        ['', 1],
        ['/search/entry/create/', 0],
    ])('should only open first entry when current route is not "%s"', (pathname, expectedNavigateCount) => {
        routerMock.navigate.mockClear();

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const location = TestBed.inject<Location>(LOCATION as unknown as ProviderToken<any>);
        location.pathname = pathname;

        queryParamMap.emit(convertToParamMap({s: 'test'}));

        expect(routerMock.navigate).toHaveBeenCalledTimes(expectedNavigateCount);
    });

    it('reloads news on language change', () => {
        newsServiceMock.getActiveNewsSummaries.mockClear();

        langChange$.next({
            lang: 'de',
        });

        expect(newsServiceMock.getActiveNewsSummaries).toHaveBeenCalledTimes(1);
    });
});
