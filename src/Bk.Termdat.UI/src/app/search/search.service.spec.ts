import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {CommentStatus} from 'api/CommentStatus';
import {PagingParameters} from 'api/PagingParameters';
import {SearchQuery} from 'api/SearchQuery';
import {Status} from 'api/Status';
import {SearchService} from './search.service';

describe('SearchService', () => {
    let service: SearchService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(SearchService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get a search result', () => {
        let data;
        const phrase = 'corona';
        const sourceLanguageIds = [1, 2, 3];
        const targetLanguageIds = [4, 5, 6];
        const paging: PagingParameters = {pageIndex: 1, pageSize: 50};

        let query = `?pageindex=${paging.pageIndex}`;
        query += `&pagesize=${paging.pageSize}`;
        query += `&phrase=${phrase}`;
        sourceLanguageIds.forEach(sourceLanguageId => {
            query += `&sourceLanguageIds=${sourceLanguageId}`;
        });
        targetLanguageIds.forEach(targetLanguageId => {
            query += `&targetLanguageIds=${targetLanguageId}`;
        });

        service.getSearchResult(phrase, paging, sourceLanguageIds, targetLanguageIds).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/search/search${query}`);
        expect(req.request.method).toBe('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it('should get search result without language ids', () => {
        let data;
        const phrase = 'corona';
        const paging: PagingParameters = {pageIndex: 1, pageSize: 50};

        let query = `?pageindex=${paging.pageIndex}`;
        query += `&pagesize=${paging.pageSize}`;
        query += `&phrase=${phrase}`;

        service.getSearchResult(phrase, paging).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/search/search${query}`);
        expect(req.request.method).toBe('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it.each([
        {
            param: 'entryIds',
            search: {entryIds: '10001,10002,10003'} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&entryIds=10001,10002,10003`,
        },
        {
            param: 'commentStatus',
            search: {commentStatus: [CommentStatus.Empty, CommentStatus.Filled]} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&commentStatus=0&commentStatus=1`,
        },
        {
            param: 'offices',
            search: {offices: [0, 1], officesPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&offices=0&offices=1&officesPriority=true`,
        },
        {
            param: 'offices',
            search: {offices: [0, 1], officesPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&offices=0&offices=1`,
        },
        {
            param: 'subjectDomains',
            search: {subjectDomains: ['0', '1-2'], subjectDomainsPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&subjectDomains=0&subjectDomains=1-2`,
        },
        {
            param: 'subjectDomains',
            search: {subjectDomains: ['0', '1-2'], subjectDomainsPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&subjectDomains=0&subjectDomains=1-2&subjectDomainsPriority=true`,
        },
        {
            param: 'lenochClassifications',
            search: {lenochClassifications: [0, 1], lenochClassificationsPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&lenochClassifications=0&lenochClassifications=1&lenochClassificationsPriority=true`,
        },
        {
            param: 'lenochClassifications',
            search: {lenochClassifications: [0, 1], lenochClassificationsPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&lenochClassifications=0&lenochClassifications=1`,
        },
        {
            param: 'collections',
            search: {collections: [0, 1], collectionsPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&collections=0&collections=1&collectionsPriority=true`,
        },
        {
            param: 'collections',
            search: {collections: [0, 1], collectionsPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&collections=0&collections=1`,
        },
        {
            param: 'glossaries',
            search: {glossaries: [0, 1]} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&glossaries=0&glossaries=1`,
        },
        {
            param: 'status',
            search: {status: [Status.Validated, Status.InProgress], statusPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&status=1&status=0&statusPriority=true`,
        },
        {
            param: 'status',
            search: {status: [Status.Validated, Status.InProgress], statusPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&status=1&status=0`,
        },
        {
            param: 'adminInfos',
            search: {adminInfos: [1, 2], adminInfosPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&adminInfos=1&adminInfos=2&adminInfosPriority=true`,
        },
        {
            param: 'adminInfos',
            search: {adminInfos: [1, 2], adminInfosPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&adminInfos=1&adminInfos=2`,
        },
        {
            param: 'createdBy',
            search: {createdBy: 'test@test.ch', createdByPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&createdBy=test@test.ch&createdByPriority=true`,
        },
        {
            param: 'createdBy',
            search: {createdBy: 'test@test.ch', createdByPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&createdBy=test@test.ch`,
        },
        {
            param: 'modifiedBy',
            search: {modifiedBy: 'test@test.ch', modifiedByPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedBy=test@test.ch&modifiedByPriority=true`,
        },
        {
            param: 'modifiedBy',
            search: {modifiedBy: 'test@test.ch', modifiedByPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedBy=test@test.ch`,
        },
        {
            param: 'modifiedFrom',
            search: {modifiedFrom: new Date('1970-01-01'), modifiedPeriodPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedFrom=1970-01-01T00:00:00.000Z&modifiedPeriodPriority=true`,
        },
        {
            param: 'modifiedFrom',
            search: {modifiedFrom: new Date('1970-01-01'), modifiedPeriodPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedFrom=1970-01-01T00:00:00.000Z`,
        },
        {
            param: 'modifiedTo',
            search: {modifiedTo: new Date('1970-01-02'), modifiedPeriodPriority: true} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedTo=1970-01-02T00:00:00.000Z&modifiedPeriodPriority=true`,
        },
        {
            param: 'modifiedTo',
            search: {modifiedTo: new Date('1970-01-02'), modifiedPeriodPriority: false} as SearchQuery,
            expected: `?pageindex=1&pagesize=50&phrase=corona&modifiedTo=1970-01-02T00:00:00.000Z`,
        },
        {
            param: 'fields',
            search: {
                officesPriority: false,
                lenochClassificationsPriority: false,
                collectionsPriority: false,
                statusPriority: false,
                adminInfosPriority: false,
                createdByPriority: false,
                modifiedByPriority: false,
                modifiedPeriodPriority: false,
                fields: {
                    term: true,
                    name: true,
                    abbreviation: true,
                    phraseology: true,
                    definition: false,
                    note: true,
                    context: false,
                    source: true,
                    metadata: true,
                    country: false,
                    comment: true,
                },
            } as SearchQuery,
            expected:
                '?pageindex=1&pagesize=50&phrase=corona&fields.term=true&fields.name=true&fields.abbreviation=true' +
                '&fields.phraseology=true&fields.definition=false&fields.note=true&fields.context=false&fields.source=true' +
                '&fields.metadata=true&fields.country=false&fields.comment=true',
        },
        {
            param: 'combo 1',
            search: {
                offices: [1],
                officesPriority: false,
                lenochClassifications: [2],
                lenochClassificationsPriority: false,
                collections: [3],
                collectionsPriority: false,
                glossaries: [4],
                status: [Status.InProgress],
                statusPriority: false,
                adminInfos: [1],
                adminInfosPriority: false,
                commentStatus: [CommentStatus.Empty],
                createdBy: 'ml@admin.ch',
                createdByPriority: false,
                modifiedBy: 'fk@admin.ch',
                modifiedByPriority: false,
                modifiedFrom: new Date('1970-01-01'),
                modifiedTo: new Date('1970-01-02'),
                modifiedPeriodPriority: false,
                fields: {
                    term: true,
                    name: true,
                    abbreviation: true,
                    phraseology: true,
                    definition: false,
                    note: true,
                    context: false,
                    source: true,
                    metadata: true,
                    country: false,
                    comment: false,
                },
            } as SearchQuery,
            expected:
                '?pageindex=1&pagesize=50&phrase=corona&offices=1&lenochClassifications=2&collections=3&glossaries=4&status=0&adminInfos=1' +
                '&commentStatus=0&createdBy=ml@admin.ch&modifiedBy=fk@admin.ch&modifiedFrom=1970-01-01T00:00:00.000Z' +
                '&modifiedTo=1970-01-02T00:00:00.000Z&fields.term=true&fields.name=true&fields.abbreviation=true&fields.phraseology=true' +
                '&fields.definition=false&fields.note=true&fields.context=false&fields.source=true' +
                '&fields.metadata=true&fields.country=false&fields.comment=false',
        },
        {
            param: 'combo 2',
            search: {
                offices: [1],
                officesPriority: true,
                lenochClassifications: [2],
                lenochClassificationsPriority: true,
                collections: [3],
                collectionsPriority: true,
                glossaries: [4],
                status: [Status.InProgress],
                statusPriority: true,
                adminInfos: [1],
                adminInfosPriority: true,
                commentStatus: [CommentStatus.Empty],
                createdBy: 'ml@admin.ch',
                createdByPriority: true,
                modifiedBy: 'fk@admin.ch',
                modifiedByPriority: true,
                modifiedFrom: new Date('1970-01-01'),
                modifiedTo: new Date('1970-01-02'),
                modifiedPeriodPriority: true,
                fields: {
                    term: true,
                    name: true,
                    abbreviation: true,
                    phraseology: true,
                    definition: false,
                    note: true,
                    context: false,
                    source: true,
                    metadata: true,
                    country: false,
                    comment: true,
                },
            } as SearchQuery,
            expected:
                '?pageindex=1&pagesize=50&phrase=corona&offices=1&officesPriority=true&lenochClassifications=2' +
                '&lenochClassificationsPriority=true&collections=3&collectionsPriority=true&' +
                'glossaries=4&status=0&statusPriority=true&adminInfos=1&adminInfosPriority=true&commentStatus=0&' +
                'createdBy=ml@admin.ch&createdByPriority=true&modifiedBy=fk@admin.ch&modifiedByPriority=true&modifiedFrom=1970-01-01T00:00:00.000Z' +
                '&modifiedTo=1970-01-02T00:00:00.000Z&modifiedPeriodPriority=true&fields.term=true&fields.name=true&fields.abbreviation=true&' +
                'fields.phraseology=true&fields.definition=false&fields.note=true&fields.context=false&fields.source=true' +
                '&fields.metadata=true&fields.country=false&fields.comment=true',
        },
        {
            param: 'combo 3',
            search: {
                offices: [1],
                officesPriority: true,
                lenochClassifications: [2],
                lenochClassificationsPriority: true,
                collections: [3],
                collectionsPriority: true,
                glossaries: [4],
                status: [Status.InProgress],
                statusPriority: true,
                adminInfos: [1, 2],
                adminInfosPriority: true,
                commentStatus: [CommentStatus.Empty],
                createdBy: 'ml@admin.ch',
                createdByPriority: true,
                modifiedBy: 'fk@admin.ch',
                modifiedByPriority: true,
                modifiedFrom: new Date('1970-01-01'),
                modifiedTo: new Date('1970-01-02'),
                modifiedPeriodPriority: true,
                subjectDomains: ['5', '6-1'],
                subjectDomainsPriority: true,
                fields: {
                    term: true,
                    name: true,
                    abbreviation: true,
                    phraseology: true,
                    definition: false,
                    note: true,
                    context: false,
                    source: true,
                    reference: false,
                    metadata: true,
                    country: false,
                    comment: true,
                },
            } as unknown as SearchQuery,
            expected:
                '?pageindex=1&pagesize=50&phrase=corona&offices=1&officesPriority=true&lenochClassifications=2' +
                '&lenochClassificationsPriority=true&subjectDomains=5&subjectDomains=6-1&subjectDomainsPriority=true' +
                '&collections=3&collectionsPriority=true&glossaries=4&status=0&statusPriority=true&adminInfos=1&adminInfos=2' +
                '&adminInfosPriority=true&commentStatus=0&createdBy=ml@admin.ch&createdByPriority=true' +
                '&modifiedBy=fk@admin.ch&modifiedByPriority=true&modifiedFrom=1970-01-01T00:00:00.000Z' +
                '&modifiedTo=1970-01-02T00:00:00.000Z&modifiedPeriodPriority=true&fields.term=true&fields.name=true' +
                '&fields.abbreviation=true&fields.phraseology=true&fields.definition=false&fields.note=true' +
                '&fields.context=false&fields.source=true&fields.metadata=true' +
                '&fields.country=false&fields.comment=true',
        },
        {
            param: 'combo 4',
            search: {
                offices: [1],
                officesPriority: true,
                lenochClassifications: [2],
                lenochClassificationsPriority: true,
                collections: [3],
                collectionsPriority: true,
                glossaries: [4],
                status: [Status.InProgress],
                statusPriority: true,
                adminInfos: [2],
                adminInfosPriority: true,
                commentStatus: [CommentStatus.Empty],
                createdBy: 'ml@admin.ch',
                createdByUser: true,
                createdByPriority: true,
                modifiedBy: 'fk@admin.ch',
                modifiedByUser: true,
                modifiedByPriority: true,
                modifiedFrom: new Date('1970-01-01'),
                modifiedTo: new Date('1970-01-02'),
                modifiedPeriodPriority: true,
                subjectDomains: ['5', '6-1'],
                subjectDomainsPriority: true,
                fields: {
                    term: true,
                    name: true,
                    abbreviation: true,
                    phraseology: true,
                    definition: false,
                    note: true,
                    context: false,
                    source: true,
                    reference: false,
                    metadata: true,
                    country: false,
                    comment: true,
                },
            } as unknown as SearchQuery,
            expected:
                '?pageindex=1&pagesize=50&phrase=corona&offices=1&officesPriority=true&lenochClassifications=2' +
                '&lenochClassificationsPriority=true&subjectDomains=5&subjectDomains=6-1&subjectDomainsPriority=true' +
                '&collections=3&collectionsPriority=true&glossaries=4&status=0&statusPriority=true&adminInfos=2' +
                '&adminInfosPriority=true&commentStatus=0&createdBy=ml@admin.ch&createdByUser=true&createdByPriority=true' +
                '&modifiedBy=fk@admin.ch&modifiedByUser=true&modifiedByPriority=true&modifiedFrom=1970-01-01T00:00:00.000Z' +
                '&modifiedTo=1970-01-02T00:00:00.000Z&modifiedPeriodPriority=true&fields.term=true&fields.name=true' +
                '&fields.abbreviation=true&fields.phraseology=true&fields.definition=false&fields.note=true' +
                '&fields.context=false&fields.source=true&fields.metadata=true' +
                '&fields.country=false&fields.comment=true',
        },
    ])('should get search result with extended search params: $param', ({search, expected}) => {
        let data;
        const phrase = 'corona';
        const paging: PagingParameters = {pageIndex: 1, pageSize: 50};

        service.getSearchResult(phrase, paging, [], [], search).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/search/search${expected}`);
        expect(req.request.method).toBe('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it('should get recommendations', () => {
        let data;
        const phrase = 'test';

        const query = `?phrase=${phrase}`;
        service.getRecommendation('test').subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/search/recommend${query}`);
        expect(req.request.method).toBe('GET');

        req.flush(['test']);
        httpMock.verify();

        expect(data).toHaveLength(1);
    });

    it('should get recommendations with specific source languages', () => {
        let data;
        const phrase = 'test';
        const sourceLanguageIds = [1, 2, 3];

        let query = `?phrase=${phrase}`;
        sourceLanguageIds.forEach(sourceLanguageId => {
            query += `&sourceLanguageIds=${sourceLanguageId}`;
        });

        service.getRecommendation('test', sourceLanguageIds).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/search/recommend${query}`);
        expect(req.request.method).toBe('GET');

        req.flush(['test']);
        httpMock.verify();

        expect(data).toHaveLength(1);
    });

    it('can change source languages', () => {
        service.changeSourceLanguages([1]);

        expect(service.getSourceLanguages()).toEqual([1]);
    });

    it('can change target languages', () => {
        service.changeTargetLanguages([1]);

        expect(service.getTargetLanguages()).toEqual([1]);
    });

    it('can get source languages', () => {
        expect(service.getSourceLanguages()).toEqual([]);
    });

    it('can get target languages', () => {
        expect(service.getTargetLanguages()).toEqual([]);
    });
});
