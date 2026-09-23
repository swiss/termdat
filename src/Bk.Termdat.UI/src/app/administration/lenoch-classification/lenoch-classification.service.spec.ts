import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {LenochClassificationDetail} from 'api/LenochClassificationDetail';
import {LenochClassificationSummary} from 'api/LenochClassificationSummary';
import {LenochClassificationService} from './lenoch-classification.service';

describe('LenochClassificationService', () => {
    let service: LenochClassificationService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({imports: [], providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]});
        service = TestBed.inject(LenochClassificationService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get lenoch classifications', () => {
        const classifications = [{id: 1}, {id: 2}, {id: 3}];
        let data: LenochClassificationSummary[];
        service.getLenochClassifications().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/lenoch-classifications`);
        expect(req.request.method).toBe('GET');

        req.flush(classifications);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(3);
        expect(data!).toEqual(classifications);
    });

    it('should get lenoch classification details', () => {
        const details = [{id: 1}, {id: 2}];
        let data: LenochClassificationDetail[];
        service.getLenochClassificationDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/lenoch-classifications/details`);
        expect(req.request.method).toBe('GET');

        req.flush(details);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(2);
        expect(data!).toEqual(details);
    });
});
