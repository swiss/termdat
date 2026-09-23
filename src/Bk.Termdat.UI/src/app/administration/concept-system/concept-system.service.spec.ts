import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {ConceptSystemDetail} from 'api/ConceptSystemDetail';
import {ConceptSystemModification} from 'api/ConceptSystemModification';
import {ConceptSystemSummary} from '../../../api/ConceptSystemSummary';
import {ConceptSystemService} from './concept-system.service';

describe('ConceptSystemService', () => {
    let service: ConceptSystemService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(ConceptSystemService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get concept system by id', done => {
        const data: ConceptSystemModification = {
            id: 2,
            title: 'Test Concept Syste Title',
            collectionId: -1,
            positions: [],
        };

        service.get(2).subscribe(response => {
            expect(response).toEqual(data);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: '/api/concept-systems/2'});

        req.flush(data);
    });

    it('should get concept systems', done => {
        const data: ConceptSystemSummary[] = [
            {
                id: 1,
                title: 'Some title',
            },
        ];

        service.getConceptSystems().subscribe(response => {
            expect(response).toBeDefined();
            expect(response).toHaveLength(1);
            expect(response[0].id).toEqual(1);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: `/api/concept-systems/`});

        req.flush(data);
    });

    it('should get concept system details', done => {
        const data: ConceptSystemDetail[] = [
            {
                id: 1,
                created: '2013-02-22T23:05:05.78' as unknown as Date,
                modified: '2013-02-23T23:05:05.78' as unknown as Date,
            } as ConceptSystemDetail,
        ];

        service.getConceptSystemDetails().subscribe(response => {
            expect(response).toBeDefined();
            expect(response).toHaveLength(1);
            expect(response[0].created).toBeInstanceOf(Date);
            expect(response[0].modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: `/api/concept-systems/details`});

        req.flush(data);
    });

    it('should update concept system', done => {
        const data: ConceptSystemModification = {
            id: 2,
            title: 'Test Concept System Update Title',
            collectionId: 42,
            positions: [{value: '1. Test Position'}],
        };

        service.update(data).subscribe(response => {
            expect(response).toBeDefined();
            expect(response.id).toEqual(data.id);
            expect(response.title).toEqual(data.title);
            expect(response.collection).toEqual('Test Collection Code');
            expect(response.created).toBeInstanceOf(Date);
            expect(response.modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'PUT', url: `/api/concept-systems/2`});

        req.flush({
            id: data.id,
            title: data.title,
            collection: 'Test Collection Code',
            created: '2013-02-22T23:05:05.78' as unknown as Date,
            createdBy: 'user@test.ch',
            modified: '2013-02-23T23:05:05.78' as unknown as Date,
            modifiedBy: 'user@test.ch',
        });
    });

    it('should create new concept system', done => {
        const data: ConceptSystemModification = {
            title: 'New Test Concept System',
            collectionId: 11,
        };

        service.create(data).subscribe(response => {
            expect(response).toBeDefined();
            expect(response.id).toEqual(7);
            expect(response.title).toEqual(data.title);
            expect(response.collection).toEqual('Test Collection Code');
            expect(response.created).toBeInstanceOf(Date);
            expect(response.modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'POST', url: `/api/concept-systems`});

        req.flush({
            id: 7,
            title: data.title,
            collection: 'Test Collection Code',
            created: '2013-02-22T23:05:05.78' as unknown as Date,
            createdBy: 'user@test.ch',
            modified: '2013-02-23T23:05:05.78' as unknown as Date,
            modifiedBy: 'user@test.ch',
        });
    });

    it('should delete concept system', done => {
        service.delete(5).subscribe(() => {
            done();
        });

        const req = httpMock.expectOne({method: 'DELETE', url: `/api/concept-systems/5`});

        req.flush(null);
    });
});
