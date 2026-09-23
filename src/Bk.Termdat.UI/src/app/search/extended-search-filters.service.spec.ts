import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {CollectionSummary} from 'api/CollectionSummary';
import {CommentStatus} from 'api/CommentStatus';
import {CommentStatusOption} from 'api/CommentStatusOption';
import {DomainSummary} from 'api/DomainSummary';
import {GlossarySummary} from 'api/GlossarySummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {Status} from 'api/Status';
import {SubjectSummary} from 'api/SubjectSummary';
import {UserAccountSummary} from 'api/UserAccountSummary';
import {of, Subject} from 'rxjs';
import {CollectionService} from '../administration/collection/collection.service';
import {GlossaryService} from '../administration/glossary/glossary.service';
import {OfficeService} from '../administration/office/office.service';
import {TermdatClassificationService} from '../administration/termdat-classification/termdat-classification.service';
import {UserAccountService} from '../administration/user-administration/user-account.service';
import {Option} from '../shared/Option';
import {ExtendedSearchFiltersService} from './extended-search-filters.service';

describe('ExtendedSearchFiltersService', () => {
    let service: ExtendedSearchFiltersService;
    let httpMock: HttpTestingController;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: langChangeSubject.asObservable(),
    };

    let offices: OfficeSummary[] = [];
    let activeCollections: CollectionSummary[] = [];
    let allCollections: CollectionSummary[] = [];
    let subjects: SubjectSummary[] = [];
    let userAccountSummaries: UserAccountSummary[] = [];
    let glossarySummaries: GlossarySummary[] = [];
    const officeServiceMock = {
        getOffices: jest.fn(),
    };
    const collectionServiceMock = {
        getActiveCollections: jest.fn(),
        getAllCollections: jest.fn(),
    };
    const termdatClassificationServiceMock = {
        getSubjects: jest.fn(),
    };
    const userAccountServiceMock = {
        getUserAccountSummaries: jest.fn(),
    };

    const glossaryServiceMock = {
        getGlossarySummaries: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: OfficeService, useValue: officeServiceMock},
                {provide: TermdatClassificationService, useValue: termdatClassificationServiceMock},
                {provide: UserAccountService, useValue: userAccountServiceMock},
                {provide: GlossaryService, useValue: glossaryServiceMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
            ],
        });
        service = TestBed.inject(ExtendedSearchFiltersService);

        activeCollections = [
            {
                id: 10000,
                name: 'Collection1',
                code: 'COL1',
            } as CollectionSummary,
            {
                id: 20000,
                name: 'Collection2',
                code: 'COL2',
            } as CollectionSummary,
            {
                id: 30000,
                name: 'Collection3',
                code: 'COL3',
            } as CollectionSummary,
        ];

        allCollections = [
            {
                id: 10000,
                name: 'Collection1',
                code: 'COL1',
            } as CollectionSummary,
            {
                id: 20000,
                name: 'Collection2',
                code: 'COL2',
            } as CollectionSummary,
        ];

        offices = [
            {
                id: 10000,
                name: 'Office1',
                code: 'OFF1',
            },
            {
                id: 20000,
                name: 'Office2',
                code: 'OFF2',
            },
            {
                id: 30000,
                name: 'Office3',
                code: 'OFF3',
            },
        ];

        subjects = [
            {
                id: 10000,
                name: 'Subject1',
                domains: [
                    {
                        id: 10001,
                        name: 'Domain1',
                    } as DomainSummary,
                    {
                        id: 10002,
                        name: 'Domain2',
                    },
                ],
            } as SubjectSummary,
            {
                id: 20000,
                name: 'Subject2',
                domains: [],
            },
        ];

        userAccountSummaries = [
            {
                firstName: 'Dan',
                address: 'dm@net.com',
                lastName: 'man',
                organisationUnit: 'ou',
                phone: '0812886545',
                userName: 'dm',
            },
        ];

        glossarySummaries = [{id: 1, code: 'g1', name: 'glossar eins'}];

        service = TestBed.inject(ExtendedSearchFiltersService);
        officeServiceMock.getOffices.mockReturnValue(of(offices));
        collectionServiceMock.getActiveCollections.mockReturnValue(of(activeCollections));
        collectionServiceMock.getAllCollections.mockReturnValue(of(allCollections));
        termdatClassificationServiceMock.getSubjects.mockReturnValue(of(subjects));
        userAccountServiceMock.getUserAccountSummaries.mockReturnValue(of(userAccountSummaries));
        glossaryServiceMock.getGlossarySummaries.mockReturnValue(of(glossarySummaries));
        httpMock = TestBed.inject(HttpTestingController);

        translateServiceMock.instant.mockImplementation(key => key);
    });

    afterEach(() => {
        httpMock.verify();
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should load admin infos', () => {
        let data;
        service.getAdminInfos().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/admin-infos`);
        expect(req.request.method).toBe('GET');

        req.flush(['test']);

        expect(data).toHaveLength(1);
    });

    it('should load user accounts', () => {
        let data;
        service.getUserAccounts().subscribe(result => (data = result));

        expect(userAccountServiceMock.getUserAccountSummaries).toHaveBeenCalledTimes(1);

        expect(data).toHaveLength(1);
    });

    it('should cache user accounts after first request', () => {
        let data1, data2;
        service.getUserAccounts().subscribe(result => (data1 = result));

        service.getUserAccounts().subscribe(data => (data2 = data));

        expect(data1).toHaveLength(1);
        expect(data2).toHaveLength(1);
        expect(data2).toEqual(data1);
        expect(userAccountServiceMock.getUserAccountSummaries).toHaveBeenCalledTimes(1);
    });

    it('should load collections', () => {
        let data;
        service.getCollections().subscribe(result => (data = result));

        expect(collectionServiceMock.getAllCollections).toHaveBeenCalledTimes(1);

        expect(data).toHaveLength(2);
    });

    it('should load subjects', () => {
        let data;
        service.getSubjects().subscribe(result => (data = result));

        expect(termdatClassificationServiceMock.getSubjects).toHaveBeenCalledTimes(1);

        expect(data).toHaveLength(2);
    });

    it('should load offices', () => {
        let data;
        service.getOffices().subscribe(result => (data = result));

        expect(officeServiceMock.getOffices).toHaveBeenCalledTimes(1);

        expect(data).toHaveLength(3);
    });

    it('should load glossaries', () => {
        let data;
        service.getGlossaries().subscribe(result => (data = result));

        expect(glossaryServiceMock.getGlossarySummaries).toHaveBeenCalledTimes(1);

        expect(data).toHaveLength(1);
    });

    it('should load states', () => {
        let data: Option[];
        service.getStates().subscribe(result => (data = result));

        expect(translateServiceMock.instant).toHaveBeenCalledTimes(2);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.status.Validated');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.status.InProgress');

        expect(data!).toHaveLength(2);

        expect(data![0].id).toEqual(Status.Validated);
        expect(data![0].name).toEqual('field.status.Validated');
        expect(data![1].id).toEqual(Status.InProgress);
        expect(data![1].name).toEqual('field.status.InProgress');
    });

    it('should load commentStatusOptions', () => {
        let data: CommentStatusOption[];
        service.getCommentStatusOptions().subscribe(result => (data = result));

        expect(translateServiceMock.instant).toHaveBeenCalledTimes(2);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.commentStatus.Empty');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.commentStatus.Filled');

        expect(data!).toHaveLength(2);

        expect(data![0].id).toEqual(CommentStatus.Empty);
        expect(data![0].name).toEqual('field.commentStatus.Empty');
        expect(data![1].id).toEqual(CommentStatus.Filled);
        expect(data![1].name).toEqual('field.commentStatus.Filled');
    });

    it('caches collections after first request', () => {
        let data1, data2;
        service.getCollections().subscribe(result => (data1 = result));

        service.getCollections().subscribe(result => (data2 = result));

        expect(data1).toBeTruthy();
        expect(data1).toEqual(data2);
        expect(collectionServiceMock.getAllCollections).toHaveBeenCalledTimes(1);
    });

    it('caches subjects after first request', () => {
        let data1, data2;
        service.getSubjects().subscribe(result => (data1 = result));

        service.getSubjects().subscribe(result => (data2 = result));

        expect(data1).toBeTruthy();
        expect(data1).toEqual(data2);
        expect(termdatClassificationServiceMock.getSubjects).toHaveBeenCalledTimes(1);
    });

    it('caches offices after first request', () => {
        let data1, data2;
        service.getOffices().subscribe(result => (data1 = result));

        service.getOffices().subscribe(result => (data2 = result));

        expect(data1).toBeTruthy();
        expect(data1).toEqual(data2);
        expect(officeServiceMock.getOffices).toHaveBeenCalledTimes(1);
    });

    it('caches glossaries after first request', () => {
        let data1, data2;

        service.getGlossaries().subscribe(result => (data1 = result));

        service.getGlossaries().subscribe(result => (data2 = result));

        expect(data1).toBeTruthy();
        expect(data1).toEqual(data2);
        expect(glossaryServiceMock.getGlossarySummaries).toHaveBeenCalledTimes(1);
    });

    it('resets cached data correctly when lang changes', () => {
        service.getOffices().subscribe(() => {});
        expect(officeServiceMock.getOffices).toHaveBeenCalledTimes(1);

        service.getCollections().subscribe(() => {});
        expect(collectionServiceMock.getAllCollections).toHaveBeenCalledTimes(1);

        service.getGlossaries().subscribe(() => {});
        expect(glossaryServiceMock.getGlossarySummaries).toHaveBeenCalledTimes(1);

        let gotCleared = false;
        service.cleared$.subscribe(() => {
            gotCleared = true;
        });

        langChangeSubject.next(true);

        expect(gotCleared).toBe(true);

        service.getOffices().subscribe(() => {});
        expect(officeServiceMock.getOffices).toHaveBeenCalledTimes(2);

        service.getCollections().subscribe(() => {});
        expect(collectionServiceMock.getAllCollections).toHaveBeenCalledTimes(2);

        service.getGlossaries().subscribe(() => {});
        expect(glossaryServiceMock.getGlossarySummaries).toHaveBeenCalledTimes(2);
    });
});
