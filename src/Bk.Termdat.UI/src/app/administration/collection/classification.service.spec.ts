import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {ClassificationSummary} from '../../../api/ClassificationSummary';
import {ClassificationService} from './classification.service';

describe('ClassificationService', () => {
    let service: ClassificationService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(ClassificationService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get classifications', () => {
        let data: ClassificationSummary[];
        service.getClassifications().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/classifications`);
        expect(req.request.method).toBe('GET');

        req.flush([
            {
                id: 1,
                code: 'code1',
                name: 'name1',
            },
            {
                id: 2,
                code: 'code2',
                name: 'name2',
            },
        ]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(2);
        expect(data![0].id).toEqual(1);
        expect(data![0].name).toEqual('name1');
        expect(data![0].code).toEqual('code1');
    });
});
