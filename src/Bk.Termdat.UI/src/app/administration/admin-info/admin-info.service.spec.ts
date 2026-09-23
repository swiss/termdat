import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {AdminInfoDetail} from 'api/AdminInfoDetail';
import {AdminInfoModification} from 'api/AdminInfoModification';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {AdminInfoService} from './admin-info.service';

describe('AdminInfoService', () => {
    let service: AdminInfoService;
    let httpMock: HttpTestingController;

    const summaryMock = {
        id: 11,
        code: 'code11',
        description: 'description11',
    };

    const detailMock = {
        id: 300,
        code: 'adminInfo',
        description: 'description',
        entryCount: 200,
        created: '2023-01-22T13:05:05.99' as unknown as Date,
        modified: '2023-09-29T09:05:05.99' as unknown as Date,
        createdBy: 'test@test.ch',
        modifiedBy: 'tester@tester.ch',
    };

    const adminInfoModification = {
        code: 'code',
        description: 'description',
        id: 1,
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(AdminInfoService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get admininfo summary', () => {
        let data: AdminInfoSummary[];
        service.getAdminInfos().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/admin-infos`);
        expect(req.request.method).toBe('GET');

        req.flush([summaryMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should get admininfo detail', () => {
        let data: AdminInfoDetail[];
        service.getAdminInfoDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/admin-infos/details`);
        expect(req.request.method).toBe('GET');

        req.flush([detailMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should create new admininfo', () => {
        let data: AdminInfoDetail;
        service.create(adminInfoModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/admin-infos`);
        expect(req.request.body).toEqual(adminInfoModification);
        expect(req.request.method).toBe('POST');

        req.flush(detailMock);

        expect(data!).toEqual(detailMock);
    });

    it('should delete admininfo', () => {
        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/admin-infos/${id}`);
        expect(req.request.method).toBe('DELETE');

        req.flush({});
    });

    it('should get admininfoModification', () => {
        let data: AdminInfoModification;
        service.get(1).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/admin-infos/1');
        expect(req.request.method).toBe('GET');

        req.flush(adminInfoModification);

        expect(data!).toEqual(adminInfoModification);
    });

    it('should update admininfo', () => {
        let data: AdminInfoDetail;
        service.update(adminInfoModification).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/admin-infos/1');
        expect(req.request.body).toEqual(adminInfoModification);
        expect(req.request.method).toBe('PUT');

        req.flush(detailMock);

        expect(data!).toEqual(detailMock);
    });
});
