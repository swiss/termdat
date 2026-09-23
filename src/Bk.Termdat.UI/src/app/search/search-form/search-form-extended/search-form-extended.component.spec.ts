import {AsyncPipe} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatChipsModule} from '@angular/material/chips';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObInputClearModule} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {ClassificationSummary} from 'api/ClassificationSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {CommentStatus} from 'api/CommentStatus';
import {CommentStatusOption} from 'api/CommentStatusOption';
import {DomainSummary} from 'api/DomainSummary';
import {GlossarySummary} from 'api/GlossarySummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {SubjectSummary} from 'api/SubjectSummary';
import {UserAccountSummary} from 'api/UserAccountSummary';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject, of, Subject, throwError} from 'rxjs';
import {AuthService} from '../../../auth/auth.service';
import {EntryService} from '../../../entry/entry.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {SESSION_STORAGE, WINDOW} from '../../../shared/injection.tokens';
import {Option} from '../../../shared/Option';
import {ExtendedSearchFiltersService} from '../../extended-search-filters.service';
import {SearchFormExtendedComponent} from './search-form-extended.component';

describe('SearchFormExtendedComponent', () => {
    let component: SearchFormExtendedComponent;
    let fixture: ComponentFixture<SearchFormExtendedComponent>;

    const extendedSearchFiltersServiceMock = {
        getAdminInfos: jest.fn(),
        getClassifications: jest.fn(),
        getCollections: jest.fn(),
        getOffices: jest.fn(),
        getGlossaries: jest.fn(),
        getUserAccounts: jest.fn(),
        getStates: jest.fn(),
        getCommentStatusOptions: jest.fn(),
        getSubjects: jest.fn(),
        resetExtendedSearch$: new Subject(),
        reloadExtendedSearchData$: new Subject(),
        cleared$: new Subject(),
    };

    const entryServiceMock = {
        getReliabilities: jest.fn(),
    };

    const windowMock = {
        setTimeout: jest.fn().mockImplementation(fn => fn()),
        matchMedia: jest.fn().mockImplementation(() => ({
            matches: false,
            media: '',
            onchange: null,
            addListener: jest.fn(),
            removeListener: jest.fn(),
            addEventListener: jest.fn(),
            removeEventListener: jest.fn(),
            dispatchEvent: jest.fn(),
        })),
    };

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: onLangChangeSubject.asObservable(),
        instant: jest.fn(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const sessionStorageMock = {
        getItem: jest.fn().mockImplementation(() => null),
        removeItem: jest.fn().mockImplementation(),
        setItem: jest.fn().mockImplementation(),
        clear: jest.fn().mockImplementation(),
    };

    const isAdminSubject = new BehaviorSubject(false);
    const isSuperUserSubject = new BehaviorSubject(false);
    const isSuperReaderSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);
    const authServiceMock = {
        isSuperUser$: isSuperUserSubject.asObservable(),
        isSuperReader$: isSuperReaderSubject.asObservable(),
        isAdmin$: isAdminSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
    };

    let formBuilder: FormBuilder;

    let adminInfos: AdminInfoSummary[] = [];
    let adminInfoChips: Chip[] = [];
    let classifications: ClassificationSummary[] = [];
    let collections: CollectionSummary[] = [];
    let collectionChips: Chip[] = [];
    let offices: OfficeSummary[] = [];
    let officeChips: Chip[] = [];
    let glossaries: GlossarySummary[] = [];
    let glossaryChips: Chip[] = [];
    let userAccounts: UserAccountSummary[] = [];
    let states: Option[] = [];
    let commentStatusOptions: CommentStatusOption[] = [];
    let subjects: SubjectSummary[] = [];
    let subjectChips: Chip[] = [];
    let reliabilities: Option[] = [];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatSelectModule),
                MockModule(MatFormFieldModule),
                MockModule(MatInputModule),
                MockModule(MatAutocompleteModule),
                MockModule(MatButtonModule),
                MockModule(MatCheckboxModule),
                MockModule(MatIconModule),
                MockModule(MatChipsModule),
                MockModule(MatDatepickerModule),
                MockModule(MatTooltipModule),
                MockModule(MatSlideToggleModule),
                MockModule(ObInputClearModule),
                SearchFormExtendedComponent,
                MockPipe(TranslatePipe),
                MockComponent(ChiplistAutocompleteComponent),
                AsyncPipe,
            ],
            providers: [
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: WINDOW, useValue: windowMock},
                {provide: SESSION_STORAGE, useValue: sessionStorageMock},
            ],
        }).compileComponents();

        isAdminSubject.next(true);
        isSuperUserSubject.next(false);
        isSuperReaderSubject.next(false);
        isWriterSubject.next(false);

        formBuilder = TestBed.inject(FormBuilder);

        adminInfos = [{id: 1, code: 'admin'} as AdminInfoSummary, {id: 2, code: 'info'} as AdminInfoSummary];
        adminInfoChips = [
            {tag: 'admin', displayText: 'admin', value: 1, deleted: false, inactive: false},
            {tag: 'info', displayText: 'info', value: 2, deleted: false, inactive: false},
        ];
        classifications = [
            {id: 1, name: 'Classification1', code: 'CLA1'},
            {id: 2, name: 'Classification2', code: 'CLA2'},
        ];
        collections = [
            {id: 1, name: 'Collection1', code: 'COL1', active: true} as CollectionSummary,
            {id: 2, name: 'Collection2', code: 'COL2', active: true} as CollectionSummary,
        ];
        collectionChips = [
            {tag: 'COL1', displayText: 'COL1 - Collection1', value: 1, deleted: false, inactive: false},
            {tag: 'COL2', displayText: 'COL2 - Collection2', value: 2, deleted: false, inactive: false},
        ];

        offices = [
            {id: 1, name: 'Office1', code: 'OFF1'},
            {id: 2, name: 'Office2', code: 'OFF2'},
        ];
        officeChips = [
            {tag: 'OFF1', displayText: 'OFF1 - Office1', value: 1, deleted: false, inactive: false},
            {tag: 'OFF2', displayText: 'OFF2 - Office2', value: 2, deleted: false, inactive: false},
        ];
        glossaries = [
            {id: 1, name: 'Glossary1', code: 'GLO1'},
            {id: 2, name: 'Glossary2', code: 'GLO2'},
        ];
        glossaryChips = [
            {tag: 'GLO1', displayText: 'GLO1 - Glossary1', value: 1, deleted: false, inactive: false},
            {tag: 'GLO2', displayText: 'GLO2 - Glossary2', value: 2, deleted: false, inactive: false},
        ];

        userAccounts = [
            {userName: 'test@test.ch', firstName: 'First', lastName: 'Last'} as UserAccountSummary,
            {userName: 'user@user.ch', firstName: 'Hans', lastName: 'User'} as UserAccountSummary,
        ];
        states = [{id: 1, name: 'State1', code: 'ST1'} as Option, {id: 2, name: 'State2', code: 'ST2'} as Option];
        commentStatusOptions = [
            {id: 0, name: 'translatedValueFor0'},
            {id: 1, name: 'translatedValueFor1'},
        ];
        subjects = [
            {
                id: 1,
                name: 'Subject1',
                domains: [
                    {id: 10, name: 'Domain10'} as DomainSummary,
                    {id: 11, name: 'Domain11'} as DomainSummary,
                    {id: 12, name: 'Domain12 with a very long name that goes on and goes on and goes on very long indeed'} as DomainSummary,
                ],
            },
            {id: 2, name: 'Subject2'} as SubjectSummary,
        ];
        subjectChips = [
            {
                displayText: 'Subject1',
                tag: 'Subject1',
                value: '1',
            },
            {
                displayText: 'Subject1: Domain10',
                tag: 'Subject1: Domain10',
                value: '1-10',
            },
            {
                displayText: 'Subject1: Domain11',
                tag: 'Subject1: Domain11',
                value: '1-11',
            },
            {
                displayText: 'Subject1: Domain12 with a very long name that goes on and goes on and goes on very long indeed',
                tag: 'Subject1: Domain12 with a very long name that goes on and goes on and goes on very long indeed',
                value: '1-12',
            },
            {
                displayText: 'Subject2',
                tag: 'Subject2',
                value: '2',
            },
        ];
        reliabilities = [{id: 1, name: 'Reliability1', code: 'R1'} as Option, {id: 2, name: 'Reliability2', code: 'R2'} as Option];

        extendedSearchFiltersServiceMock.getAdminInfos.mockReturnValue(of(adminInfos));
        extendedSearchFiltersServiceMock.getClassifications.mockReturnValue(of(classifications));
        extendedSearchFiltersServiceMock.getCollections.mockReturnValue(of(collections));
        extendedSearchFiltersServiceMock.getOffices.mockReturnValue(of(offices));
        extendedSearchFiltersServiceMock.getGlossaries.mockReturnValue(of(glossaries));
        extendedSearchFiltersServiceMock.getUserAccounts.mockReturnValue(of(userAccounts));
        extendedSearchFiltersServiceMock.getStates.mockReturnValue(of(states));
        extendedSearchFiltersServiceMock.getCommentStatusOptions.mockReturnValue(of(commentStatusOptions));
        extendedSearchFiltersServiceMock.getSubjects.mockReturnValue(of(subjects));

        entryServiceMock.getReliabilities.mockReturnValue(of(reliabilities));

        fixture = TestBed.createComponent(SearchFormExtendedComponent);
        component = fixture.componentInstance;
        component.form = formBuilder.group({});

        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should render fields if isSuperUser is true', async () => {
        isAdminSubject.next(false);
        isSuperUserSubject.next(true);
        isSuperReaderSubject.next(false);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('[formControlName=adminInfo]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=commentStatus]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=relatedCollections]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=entryIds]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=createdBy]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedBy]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedFrom]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedTo]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=metadata]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=comment]'))).toBeDefined();
    });

    it('should render fields if isSuperReader is true', async () => {
        isAdminSubject.next(false);
        isSuperReaderSubject.next(true);
        isSuperUserSubject.next(false);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('[formControlName=adminInfo]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=commentStatus]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=entryIds]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=createdBy]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedBy]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedFrom]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedTo]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=metadata]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=comment]'))).toBeDefined();
    });

    it('should not render fields if not authorized', async () => {
        isAdminSubject.next(false);
        isSuperUserSubject.next(false);
        isSuperReaderSubject.next(false);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('[formControlName=adminInfo]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=commentStatus]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=realtedCollections]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=createdBy]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedBy]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedFrom]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedTo]'))).toBeNull();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=comment]'))).toBeNull();
    });

    it('should render admin fields when isAdmin is true', async () => {
        isAdminSubject.next(true);
        isSuperUserSubject.next(false);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('[formControlName=adminInfo]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=commentStatus]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=relatedCollections]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=entryIds]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=createdBy]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedBy]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedFrom]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('[formControlName=modifiedTo]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=metadata]'))).toBeDefined();
        expect(fixture.debugElement.query(By.css('mat-checkbox[formControlName=comment]'))).toBeDefined();
    });

    it('should load data on init for admin', () => {
        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);

        expect(component.adminInfoChips).toEqual(adminInfoChips);
        expect(component.collectionChips).toEqual(collectionChips);
        expect(component.relatedCollectionChips).toEqual(collectionChips);
        expect(component.officeChips).toEqual(officeChips);
        expect(component.glossaryChips).toEqual(glossaryChips);
        expect(component.userAccounts).toEqual(userAccounts);
        expect(component.states).toEqual(states);
        expect(component.commentStatusOptions).toEqual(commentStatusOptions);
        expect(component.subjectDomainChips).toEqual(subjectChips);
    });

    it('should load data on init for super user', () => {
        extendedSearchFiltersServiceMock.getAdminInfos.mockClear();
        extendedSearchFiltersServiceMock.getUserAccounts.mockClear();

        isSuperUserSubject.next(true);
        isSuperReaderSubject.next(false);
        isAdminSubject.next(false);
        isWriterSubject.next(false);

        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);

        expect(component.adminInfoChips).toEqual(adminInfoChips);
        expect(component.collectionChips).toEqual(collectionChips);
        expect(component.relatedCollectionChips).toEqual(collectionChips);
        expect(component.officeChips).toEqual(officeChips);
        expect(component.glossaryChips).toEqual(glossaryChips);
        expect(component.userAccounts).toEqual(userAccounts);
        expect(component.states).toEqual(states);
        expect(component.commentStatusOptions).toEqual(commentStatusOptions);
        expect(component.subjectDomainChips).toEqual(subjectChips);
    });

    it('should load data on init for super reader', () => {
        extendedSearchFiltersServiceMock.getAdminInfos.mockClear();
        extendedSearchFiltersServiceMock.getUserAccounts.mockClear();

        isSuperUserSubject.next(false);
        isSuperReaderSubject.next(true);
        isAdminSubject.next(false);
        isWriterSubject.next(false);

        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);

        expect(component.adminInfoChips).toEqual(adminInfoChips);
        expect(component.collectionChips).toEqual(collectionChips);
        expect(component.officeChips).toEqual(officeChips);
        expect(component.glossaryChips).toEqual(glossaryChips);
        expect(component.states).toEqual(states);
        expect(component.commentStatusOptions).toEqual(commentStatusOptions);
        expect(component.subjectDomainChips).toEqual(subjectChips);
    });

    it('should load data on init for non super user/admin', () => {
        component.userAccounts = [];

        isSuperUserSubject.next(false);
        isSuperReaderSubject.next(false);
        isAdminSubject.next(false);
        isWriterSubject.next(false);
        extendedSearchFiltersServiceMock.getAdminInfos.mockClear();
        extendedSearchFiltersServiceMock.getUserAccounts.mockClear();

        expect(extendedSearchFiltersServiceMock.getAdminInfos).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);

        expect(component.adminInfoChips).toEqual([]);
        expect(component.collectionChips).toEqual(collectionChips);
        expect(component.officeChips).toEqual(officeChips);
        expect(component.glossaryChips).toEqual(glossaryChips);
        expect(component.userAccounts).toEqual([]);
        expect(component.states).toEqual(states);
        expect(component.commentStatusOptions).toEqual(commentStatusOptions);
        expect(component.subjectDomainChips).toEqual(subjectChips);
    });

    it('should reload language specific search options on lang change', () => {
        extendedSearchFiltersServiceMock.getUserAccounts.mockClear();
        isAdminSubject.next(true);
        isSuperUserSubject.next(false);

        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);

        onLangChangeSubject.next(true);

        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(2);
    });

    it('should reload user accounts on search filter cleared', () => {
        extendedSearchFiltersServiceMock.getUserAccounts.mockClear();
        isAdminSubject.next(true);
        isSuperUserSubject.next(false);
        isSuperReaderSubject.next(false);

        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(1);

        extendedSearchFiltersServiceMock.cleared$.next(true);

        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(2);
    });

    it('should append (LINDAS) postfix to collections exported to LINDAS', () => {
        extendedSearchFiltersServiceMock.getCollections
            .mockReset()
            .mockReturnValue(
                of([
                    {id: 1, name: 'Collection1', code: 'COL1', active: true, exportLindas: true} as CollectionSummary,
                    {id: 2, name: 'Collection2', code: 'COL2', active: true, exportLindas: false} as CollectionSummary,
                ])
            );

        fixture = TestBed.createComponent(SearchFormExtendedComponent);
        component = fixture.componentInstance;
        component.form = formBuilder.group({});
        fixture.detectChanges();

        expect(component.collectionChips[0].displayText).toEqual('COL1 - Collection1 (LINDAS)');
        expect(component.collectionChips[1].displayText).toEqual('COL2 - Collection2');
        expect(component.relatedCollectionChips[0].displayText).toEqual('COL1 - Collection1 (LINDAS)');
        expect(component.relatedCollectionChips[1].displayText).toEqual('COL2 - Collection2');
    });

    it('should append (API) postfix to collections active for API when user is admin, super user or writer', () => {
        translateServiceMock.instant.mockReturnValue('API');
        isAdminSubject.next(true);
        isSuperUserSubject.next(false);
        isWriterSubject.next(false);
        extendedSearchFiltersServiceMock.getCollections
            .mockReset()
            .mockReturnValue(
                of([
                    {id: 1, name: 'Collection1', code: 'COL1', active: true, activeApi: true} as CollectionSummary,
                    {id: 2, name: 'Collection2', code: 'COL2', active: true, activeApi: false} as CollectionSummary,
                ])
            );

        fixture = TestBed.createComponent(SearchFormExtendedComponent);
        component = fixture.componentInstance;
        component.form = formBuilder.group({});
        fixture.detectChanges();

        expect(component.collectionChips[0].displayText).toEqual('COL1 - Collection1 (API)');
        expect(component.collectionChips[1].displayText).toEqual('COL2 - Collection2');
    });

    it('should not append (API) postfix when user is neither admin, super user nor writer', () => {
        isAdminSubject.next(false);
        isSuperUserSubject.next(false);
        isWriterSubject.next(false);
        extendedSearchFiltersServiceMock.getCollections
            .mockReset()
            .mockReturnValue(of([{id: 1, name: 'Collection1', code: 'COL1', active: true, activeApi: true} as CollectionSummary]));

        fixture = TestBed.createComponent(SearchFormExtendedComponent);
        component = fixture.componentInstance;
        component.form = formBuilder.group({});
        fixture.detectChanges();

        expect(component.collectionChips[0].displayText).toEqual('COL1 - Collection1');
    });

    it('should catch errors when data loading fails', async () => {
        const form = component.form;

        extendedSearchFiltersServiceMock.getAdminInfos.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getCollections.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getOffices.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getGlossaries.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getUserAccounts.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getStates.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getCommentStatusOptions.mockReset().mockReturnValue(throwError(() => ({})));
        extendedSearchFiltersServiceMock.getSubjects.mockReset().mockReturnValue(throwError(() => ({})));

        fixture = TestBed.createComponent(SearchFormExtendedComponent);
        component = fixture.componentInstance;
        component.form = form;
        fixture.detectChanges();

        await fixture.whenStable();

        expect(extendedSearchFiltersServiceMock.getAdminInfos).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getGlossaries).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getUserAccounts).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getCommentStatusOptions).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.getSubjects).toHaveBeenCalledTimes(1);

        expect(component.adminInfoChips).toEqual([]);
        expect(component.collectionChips).toEqual([]);
        expect(component.relatedCollectionChips).toEqual([]);
        expect(component.officeChips).toEqual([]);
        expect(component.glossaryChips).toEqual([]);
        expect(component.userAccounts).toEqual([]);
        expect(component.states).toEqual([]);
        expect(component.commentStatusOptions).toEqual([]);
        expect(component.subjectDomainChips).toEqual([]);
    });

    it('should emit onReset when reset is pressed', () => {
        let count = 0;
        extendedSearchFiltersServiceMock.resetExtendedSearch$.subscribe(() => {
            count++;
        });

        const form = fixture.debugElement.query(By.css('.extended-search-form'));

        expect(form).toBeTruthy();
        form.triggerEventHandler('reset', {});

        expect(count).toEqual(1);
    });

    it('should emit onSearch when search is pressed', () => {
        let count = 0;
        component.searched.subscribe(() => {
            count++;
        });

        const form = fixture.debugElement.query(By.css('.extended-search-form'));

        expect(form).toBeTruthy();
        form.triggerEventHandler('submit', {});

        expect(count).toEqual(1);
    });

    it('should filter User Accounts', () => {
        isAdminSubject.next(true);
        const allUserAccs = component.userAccounts;

        const filteredUserAccs = component.filterUserAccounts('test');

        expect(filteredUserAccs.length).toBeLessThan(allUserAccs.length);

        const unfilteredUserAccs = component.filterUserAccounts('');

        expect(unfilteredUserAccs.length).toBe(allUserAccs.length);
    });

    it.each([
        {isAdmin: false, isSuperUser: true},
        {isAdmin: true, isSuperUser: false},
    ])('should call filterUserAccounts', async ({isAdmin, isSuperUser}) => {
        component.ngOnInit();
        fixture.detectChanges();
        isAdminSubject.next(isAdmin);
        isSuperUserSubject.next(isSuperUser);

        await fixture.whenStable();

        jest.spyOn(component, 'filterUserAccounts');
        component.form!.controls.modifiedBy.setValue('test');
        expect(component.filterUserAccounts).toHaveBeenCalledTimes(1);
        component.form!.controls.createdBy.setValue('test');
        expect(component.filterUserAccounts).toHaveBeenCalledTimes(2);
    });

    it('should setup extended form correctly', () => {
        expect(component.form?.value).toEqual({
            adminInfos: [],
            adminInfosExclude: false,
            adminInfosPriority: false,
            collections: [],
            collectionsExclude: false,
            collectionsPriority: false,
            relatedCollections: [],
            createdBy: null,
            createdByExclude: false,
            createdByUser: false,
            createdByPriority: false,
            deleted: false,
            deletedPriority: false,
            hasMissingSource: false,
            entryIds: null,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            fields: {
                abbreviation: true,
                context: false,
                country: false,
                definition: false,
                metadata: true,
                name: true,
                note: false,
                phraseology: true,
                source: false,
                term: true,
                comment: false,
            },
            modifiedBy: null,
            modifiedByUser: false,
            modifiedByPriority: false,
            modifiedByExclude: false,
            modifiedFrom: null,
            modifiedPeriodPriority: false,
            modifiedPeriodExclude: false,
            modifiedTo: null,
            offices: [1],
            officesPriority: true,
            status: [1],
            statusPriority: true,
            commentStatus: [],
            glossaries: [],
            glossariesExclude: false,
            subjectDomains: [],
            subjectDomainsPriority: false,
            reliability: [],
            reliabilityPriority: false,
        });
    });

    it('should reset form back to defaults', () => {
        component.form?.patchValue({
            adminInfos,
            adminInfosPriority: true,
            collections: [1],
            collectionsPriority: true,
            realtedCollections: [1],
            createdBy: 'test@test.ch',
            createdByPriority: true,
            entryIds: null,
            lenochClassifications: [1, 2],
            lenochClassificationsPriority: true,
            fields: {
                abbreviation: false,
                context: true,
                country: true,
                definition: true,
                metadata: false,
                name: false,
                note: true,
                phraseology: false,
                source: true,
                term: false,
                comment: true,
            },
            modifiedBy: 'test@test.ch',
            modifiedByPriority: true,
            modifiedFrom: new Date(),
            modifiedPeriodPriority: true,
            modifiedTo: new Date(),
            offices: [],
            officesPriority: false,
            status: [],
            statusPriority: false,
            commentStatus: [CommentStatus.Filled],
            glossaries: [],
        });

        component.reset();

        expect(component.form?.value).toEqual({
            adminInfos: [],
            adminInfosExclude: false,
            adminInfosPriority: false,
            collections: [],
            collectionsExclude: false,
            collectionsPriority: false,
            relatedCollections: [],
            createdBy: null,
            createdByExclude: false,
            createdByUser: false,
            createdByPriority: false,
            entryIds: null,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            fields: {
                abbreviation: true,
                context: false,
                country: false,
                definition: false,
                metadata: true,
                name: true,
                note: false,
                phraseology: true,
                source: false,
                term: true,
                comment: false,
            },
            modifiedBy: null,
            modifiedByExclude: false,
            modifiedByUser: false,
            modifiedByPriority: false,
            modifiedFrom: null,
            modifiedPeriodExclude: false,
            modifiedPeriodPriority: false,
            modifiedTo: null,
            offices: [1],
            officesPriority: true,
            status: [1],
            statusPriority: true,
            commentStatus: [],
            glossaries: [],
            glossariesExclude: false,
            subjectDomains: [],
            subjectDomainsPriority: false,
            reliability: [],
            reliabilityPriority: false,
            deleted: false,
            deletedPriority: false,
            hasMissingSource: false,
        });
    });

    it('should look for extended search values in the session storage', () => {
        expect(sessionStorageMock.getItem).toHaveBeenCalledTimes(1);
    });

    it.each([
        {isAdmin: true, isSuperUser: false, isWriter: false},
        {isAdmin: true, isSuperUser: true, isWriter: false},
        {isAdmin: true, isSuperUser: true, isWriter: true},
        {isAdmin: false, isSuperUser: true, isWriter: false},
        {isAdmin: false, isSuperUser: true, isWriter: true},
        {isAdmin: false, isSuperUser: false, isWriter: true},
    ])('should set role specific variables: $#', ({isAdmin, isSuperUser, isWriter}) => {
        isAdminSubject.next(isAdmin);
        isSuperUserSubject.next(isSuperUser);
        isWriterSubject.next(isWriter);

        expect(component.isAdmin).toEqual(isAdmin);
        expect(component.isSuperUser).toEqual(isSuperUser);
        expect(component.isWriter).toEqual(isWriter);
    });

    it.each([
        {entryIds: undefined, createdBy: undefined, modifiedBy: undefined, modifiedFrom: undefined, modifiedTo: undefined, isDirty: false},
        {entryIds: null, createdBy: null, modifiedBy: null, modifiedFrom: null, modifiedTo: null, isDirty: true},
        {entryIds: '', createdBy: '', modifiedBy: '', modifiedFrom: null, modifiedTo: null, isDirty: true},
        {entryIds: 'x', createdBy: '', modifiedBy: '', modifiedFrom: null, modifiedTo: null, isDirty: true},
        {entryIds: '', createdBy: 'x', modifiedBy: '', modifiedFrom: null, modifiedTo: null, isDirty: true},
        {entryIds: '', createdBy: '', modifiedBy: 'x', modifiedFrom: null, modifiedTo: null, isDirty: true},
        {entryIds: '', createdBy: null, modifiedBy: '', modifiedFrom: 'x', modifiedTo: null, isDirty: true},
        {entryIds: '', createdBy: '', modifiedBy: null, modifiedFrom: null, modifiedTo: 'x', isDirty: true},
    ])(
        'should handle form-dirty flag of extended search form correctly: $createdBy, $modifiedBy, $modifiedFrom, $modifiedTo',
        ({entryIds, createdBy, modifiedBy, modifiedFrom, modifiedTo, isDirty}) => {
            component.form?.markAsDirty();
            component.form?.patchValue({
                adminInfos: [],
                adminInfosPriority: false,
                collections: [],
                collectionsPriority: false,
                relatedCollections: [],
                createdBy,
                createdByUser: false,
                createdByPriority: false,
                entryIds,
                lenochClassifications: [],
                lenochClassificationsPriority: false,
                fields: {
                    abbreviation: true,
                    context: false,
                    country: false,
                    definition: false,
                    metadata: true,
                    name: true,
                    note: false,
                    phraseology: true,
                    source: false,
                    term: true,
                    comment: false,
                },
                modifiedBy,
                modifiedByUser: false,
                modifiedByPriority: false,
                modifiedFrom,
                modifiedPeriodPriority: false,
                modifiedTo,
                offices: [1],
                officesPriority: true,
                status: [1],
                statusPriority: true,
                commentStatus: [],
                glossaries: [],
                subjectDomains: [],
                subjectDomainsPriority: false,
            });

            expect(component.form?.dirty).toEqual(isDirty);
        }
    );
});
