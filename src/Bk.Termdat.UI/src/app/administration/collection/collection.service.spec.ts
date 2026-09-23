import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {CollectionDetail} from 'api/CollectionDetail';
import {CollectionModification} from 'api/CollectionModification';
import {CollectionSummary} from 'api/CollectionSummary';
import {CollectionService} from './collection.service';

describe('CollectionService', () => {
    let service: CollectionService;
    let httpMock: HttpTestingController;

    const collectionModification: CollectionModification = {
        id: 13,
        active: true,
        activeApi: false,
        exportLindas: false,
        code: 'ABC123ZEHN',
        conceptSystem: {
            id: 2,
            title: 'test',
        },
        description: 'description',
        documentation: 'none',
        descriptionDe: 'de',
        descriptionFr: 'fr',
        descriptionIt: 'it',
        descriptionRm: 'rm',
        descriptionEn: 'en',
    };

    const responseMock: CollectionDetail = {
        id: 1,
        code: 'code1',
        name: 'name1',
        active: true,
        entryCount: 0,
        isObsolete: false,
        activeApi: true,
        exportLindas: false,
        deleted: false,
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(CollectionService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get collections details', () => {
        let data: CollectionDetail[];
        service.getCollectionDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/collections/details`);
        expect(req.request.method).toBe('GET');

        req.flush([responseMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should create new collection', () => {
        let data: CollectionDetail;
        service.create(collectionModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/collections`);
        expect(req.request.body).toEqual(collectionModification);
        expect(req.request.method).toBe('POST');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });

    it('should update collection', () => {
        let data: CollectionDetail;
        service.update(collectionModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/collections/${collectionModification.id}`);
        expect(req.request.body).toEqual(collectionModification);
        expect(req.request.method).toBe('PUT');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });

    it('should soft delete collection', done => {
        service.delete(1, true).subscribe(() => {
            done();
        });

        const req = httpMock.expectOne({method: 'DELETE', url: `/api/collections/1?soft=true`});

        req.flush({});
    });

    it('should delete collection', done => {
        service.delete(1, false).subscribe(() => {
            done();
        });

        const req = httpMock.expectOne({method: 'DELETE', url: `/api/collections/1?soft=false`});

        req.flush({});
    });

    it('should restore collection', () => {
        let data: CollectionDetail;
        service.restore(1).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/collections/restore`);
        expect(req.request.method).toBe('POST');

        req.flush({
            id: 1,
        });

        expect(data!).toBeDefined();
        expect(data!.id).toEqual(1);
    });

    it('should get collections for modification', () => {
        let data: CollectionSummary[];
        service.getModificationCollections().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/collections/modification`);
        expect(req.request.method).toBe('GET');

        const expectedData = [{id: 1} as CollectionSummary, {id: 2} as CollectionSummary];
        req.flush(expectedData);

        expect(data!).toBeDefined();
        expect(data!).toEqual(expectedData);
    });
});
