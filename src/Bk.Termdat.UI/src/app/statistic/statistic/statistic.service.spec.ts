import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {StatisticSearchResult} from 'api/StatisticSearchResult';
import {StatisticService} from './statistic.service';

describe('StatisticsService', () => {
    let service: StatisticService;

    let httpMock: HttpTestingController;

    const pagination = {
        pageIndex: 1,
        pageSize: 20,
    };

    const summaryMock = {
        id: 100,
        searchText: 'searchText',
        numberOfSearches: 134,
        languages: 'DE, FR, EN',
        searchFields: 'AB, Na, Md, PH, VE',
        dateLastSearch: '2023-01-22T13:05:05.99' as unknown as Date,
    };

    const searchResultMock = {
        statistics: [summaryMock],
        totalCount: 1,
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(StatisticService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get statistic search result', () => {
        let data: StatisticSearchResult;
        const fromDate = new Date('2022-06-22T14:15:00.00');
        const toDate = new Date('2023-06-23T15:15:00.00');

        service.getStatisticsBySearchQuery(pagination, 'sort', 'phrase', fromDate, toDate).subscribe(result => (data = result));

        const req = httpMock.expectOne(
            `/api/statistics?pageIndex=1&pageSize=20&sort=sort&phrase=phrase&fromDate=2022-06-22T12:15:00.000Z&toDate=2023-06-23T13:15:00.000Z`
        );
        expect(req.request.method).toBe('GET');

        req.flush([searchResultMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
        httpMock.verify();
    });

    it('should delete statistic', () => {
        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/statistics/${id}`);
        expect(req.request.method).toBe('DELETE');

        req.flush({});
        httpMock.verify();
    });

    it('should delete selection', () => {
        const fromDate = new Date('2022-06-22T14:15:00.00');
        const toDate = new Date('2023-06-23T15:15:00.00');

        service.deleteSelection(pagination, 'sort', 'phrase', fromDate, toDate).subscribe(() => {});

        const req = httpMock.expectOne(
            `/api/statistics?pageIndex=1&pageSize=20&sort=sort&phrase=phrase&fromDate=2022-06-22T12:15:00.000Z&toDate=2023-06-23T13:15:00.000Z`
        );
        expect(req.request.method).toBe('DELETE');

        req.flush({});
        httpMock.verify();
    });
});
