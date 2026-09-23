import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {EntryModification} from 'api/EntryModification';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryTemplateService} from './entry-template.service';

describe('EntryTemplateService', () => {
    let service: EntryTemplateService;
    let httpMock: HttpTestingController;

    const entryModification = {
        id: 1,
        templateName: 'MyTemplate',
    } as EntryModification;

    const detailMock = {
        id: 22,
    } as unknown as EntryModificationDetail;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(EntryTemplateService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get entry templates for a user', () => {
        let data;

        service.getEntryTemplates().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entry-templates`);
        expect(req.request.method).toEqual('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it('should create new entry template', () => {
        let data: EntryModificationDetail;
        service.createEntryTemplate('MyTemplate', entryModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entry-templates`);
        expect(req.request.body).toEqual(entryModification);
        expect(req.request.method).toBe('POST');

        req.flush(detailMock);

        expect(data!).toEqual(detailMock);
    });

    it('should delete entry template', () => {
        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entry-templates/${id}`);
        expect(req.request.method).toBe('DELETE');

        req.flush({});
    });

    it('should return entry template by id', () => {
        let data: EntryModificationDetail;
        service.getTemplateById(1).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/entry-templates/1');
        expect(req.request.method).toBe('GET');

        req.flush(entryModification);

        expect(data!).toEqual(entryModification);
    });

    it('should update entry template', () => {
        service.update(entryModification).subscribe();

        const req = httpMock.expectOne('/api/entry-templates/1');
        expect(req.request.body).toEqual(entryModification);
        expect(req.request.method).toBe('PUT');
    });
});
