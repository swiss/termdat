import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {NewsItemModification} from 'api/NewsItemModification';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {NewsService} from './news.service';

describe('NewsService', () => {
    let service: NewsService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [provideHttpClient(), provideHttpClientTesting()],
        });

        service = TestBed.inject(NewsService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should load news summaries and convert date strings to Date objects', () => {
        const response: NewsItemSummary[] = [
            {
                id: 1,
                title: 'News',
                created: new Date('2025-01-01T10:00:00Z'),
                createdBy: 'test',
                modified: new Date('2025-01-02T10:00:00Z'),
                modifiedBy: 'test',
                validFrom: new Date('2025-01-03T10:00:00Z'),
                validTo: new Date('2025-01-04T10:00:00Z'),
                isEnabled: false,
                text: 'Text',
                canMoveDown: true,
                canMoveUp: true,
            },
        ];

        let result: NewsItemSummary[] | undefined;

        service.getNewsSummaries().subscribe(data => {
            result = data;
        });

        const req = httpMock.expectOne('/api/news');
        expect(req.request.method).toBe('GET');

        req.flush(response);

        expect(result).toHaveLength(1);
        expect(result![0].created).toBeInstanceOf(Date);
        expect(result![0].modified).toBeInstanceOf(Date);
        expect(result![0].validFrom).toBeInstanceOf(Date);
        expect(result![0].validTo).toBeInstanceOf(Date);
    });

    it('should load active news summaries and convert date strings to Date objects', () => {
        const response: NewsItemSummary[] = [
            {
                id: 1,
                title: 'News',
                created: new Date('2025-01-01T10:00:00Z'),
                createdBy: 'test',
                modified: new Date('2025-01-02T10:00:00Z'),
                modifiedBy: 'test',
                validFrom: new Date('2025-01-03T10:00:00Z'),
                validTo: new Date('2025-01-04T10:00:00Z'),
                isEnabled: true,
                text: 'Text',
                canMoveDown: true,
                canMoveUp: true,
            },
        ];

        let result: NewsItemSummary[] | undefined;

        service.getActiveNewsSummaries().subscribe(data => {
            result = data;
        });

        const req = httpMock.expectOne('/api/news/active');
        expect(req.request.method).toBe('GET');

        req.flush(response);

        expect(result).toHaveLength(1);
        expect(result![0].created).toBeInstanceOf(Date);
        expect(result![0].modified).toBeInstanceOf(Date);
        expect(result![0].validFrom).toBeInstanceOf(Date);
        expect(result![0].validTo).toBeInstanceOf(Date);
    });

    it('should load news item', () => {
        const response: NewsItemModification = {
            id: 1,
            isEnabled: true,
            validFrom: new Date('2025-01-03T10:00:00Z'),
            validTo: new Date('2025-01-04T10:00:00Z'),
            titleDe: 'News',
            titleFr: 'News',
            titleIt: 'News',
            titleRm: 'News',
            titleEn: 'News',
            textDe: 'Text',
            textFr: 'Text',
            textIt: 'Text',
            textRm: 'Text',
            textEn: 'Text',
            created: new Date('2025-01-01T10:00:00Z'),
            createdBy: 'test',
            modified: new Date('2025-01-02T10:00:00Z'),
            modifiedBy: 'test',
        };

        let result: NewsItemModification | undefined;

        service.get(1).subscribe(data => {
            result = data;
        });

        const req = httpMock.expectOne('/api/news/1');
        expect(req.request.method).toBe('GET');

        req.flush(response);

        expect(result).toEqual(response);
    });

    it('should create news item', () => {
        const data: NewsItemModification = {
            isEnabled: true,
            validFrom: new Date('2025-01-03T10:00:00Z'),
            validTo: new Date('2025-01-04T10:00:00Z'),
            titleDe: 'News',
            titleFr: 'News',
            titleIt: 'News',
            titleRm: 'News',
            titleEn: 'News',
            textDe: 'Text',
            textFr: 'Text',
            textIt: 'Text',
            textRm: 'Text',
            textEn: 'Text',
        };

        service.create(data).subscribe();

        const req = httpMock.expectOne('/api/news');
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(data);

        req.flush({});
    });

    it('should update news item', () => {
        const data: NewsItemModification = {
            id: 1,
            isEnabled: true,
            validFrom: new Date('2025-01-03T10:00:00Z'),
            validTo: new Date('2025-01-04T10:00:00Z'),
            titleDe: 'Updated news',
            titleFr: 'Updated news',
            titleIt: 'Updated news',
            titleRm: 'Updated news',
            titleEn: 'Updated news',
            textDe: 'Updated text',
            textFr: 'Updated text',
            textIt: 'Updated text',
            textRm: 'Updated text',
            textEn: 'Updated text',
        };

        service.update(data).subscribe();

        const req = httpMock.expectOne('/api/news/1');
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(data);

        req.flush({});
    });

    it('should delete news item', () => {
        service.delete(1).subscribe();

        const req = httpMock.expectOne('/api/news/1');
        expect(req.request.method).toBe('DELETE');

        req.flush(null);
    });

    it('should move news item up', () => {
        service.moveUp(1).subscribe();

        const req = httpMock.expectOne('/api/news/1/move-up');
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual({});

        req.flush(null);
    });

    it('should move news item down', () => {
        service.moveDown(1).subscribe();

        const req = httpMock.expectOne('/api/news/1/move-down');
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual({});

        req.flush(null);
    });
});
