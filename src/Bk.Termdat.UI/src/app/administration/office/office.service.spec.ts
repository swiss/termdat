import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {OfficeDetail} from 'api/OfficeDetail';
import {OfficeSummary} from 'api/OfficeSummary';
import {OfficeModification} from '../../../api/OfficeModification';
import {OfficeService} from './office.service';

describe('OfficeService', () => {
    let service: OfficeService;
    let httpMock: HttpTestingController;

    const summaryMock = {
        id: 11,
        code: 'code11',
        name: 'name11',
    };

    const detailMock = {
        id: 22,
        code: 'code22',
        name: 'name22',
        entryCount: 200,
        created: '2021-01-22T13:05:05.99' as unknown as Date,
        modified: '2021-09-29T09:05:05.99' as unknown as Date,
        createdBy: 'test@test.ch',
        modifiedBy: 'tester@tester.ch',
    };

    const officeModification = {
        code: 'code',
        descriptionDe: 'de',
        descriptionEn: 'en',
        descriptionFr: 'fr',
        descriptionIt: 'it',
        descriptionRm: 'rm',
        id: 1,
    } as OfficeModification;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(OfficeService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get office summary', () => {
        let data: OfficeSummary[];
        service.getOffices().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/offices`);
        expect(req.request.method).toBe('GET');

        req.flush([summaryMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should get office detail', () => {
        let data: OfficeDetail[];
        service.getOfficeDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/offices/details`);
        expect(req.request.method).toBe('GET');

        req.flush([detailMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should create new office', () => {
        let data: OfficeDetail;
        service.create(officeModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/offices`);
        expect(req.request.body).toEqual(officeModification);
        expect(req.request.method).toBe('POST');

        req.flush(detailMock);

        expect(data!).toEqual(detailMock);
    });

    it('should delete office', () => {
        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/offices/${id}`);
        expect(req.request.method).toBe('DELETE');

        req.flush({});
    });

    it('should get officeModification', () => {
        let data: OfficeModification;
        service.get(1).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/offices/1');
        expect(req.request.method).toBe('GET');

        req.flush(officeModification);

        expect(data!).toEqual(officeModification);
    });

    it('should update office', () => {
        let data: OfficeDetail;
        service.update(officeModification).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/offices/1');
        expect(req.request.body).toEqual(officeModification);
        expect(req.request.method).toBe('PUT');

        req.flush(detailMock);

        expect(data!).toEqual(detailMock);
    });
});
