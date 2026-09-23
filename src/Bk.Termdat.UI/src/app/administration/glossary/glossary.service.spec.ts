import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {GlossaryDetail} from 'api/GlossaryDetail';
import {GlossaryModification} from 'api/GlossaryModification';
import {GlossarySummary} from 'api/GlossarySummary';
import {GlossaryService} from './glossary.service';

describe('GlossaryService', () => {
    let service: GlossaryService;
    let httpMock: HttpTestingController;

    const glossaryModification: GlossaryModification = {
        id: 13,
        code: 'ABC123ZEHN',
        description: 'description',
        descriptionDe: 'de',
        descriptionFr: 'fr',
        descriptionIt: 'it',
        descriptionRm: 'rm',
        descriptionEn: 'en',
    };

    const responseMock = {
        id: 1,
        code: 'code1',
        name: 'name1',
        description: 'description',
        entryCount: 0,
        created: '2013-02-22T23:05:05.78' as unknown as Date,
        modified: '2013-02-22T23:05:05.78' as unknown as Date,
        createdBy: 'me',
        modifiedBy: 'me',
    };

    beforeEach(() => {
        TestBed.configureTestingModule({imports: [], providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]});
        service = TestBed.inject(GlossaryService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get glossary details', () => {
        let data: GlossaryDetail[];
        service.getGlossaryDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/glossaries/details`);
        expect(req.request.method).toBe('GET');

        req.flush([responseMock]);

        expect(data!).toBeDefined();
        expect(data!).toHaveLength(1);
    });

    it('should load glossary summaries', () => {
        let data;
        service.getGlossarySummaries().subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/glossaries');
        expect(req.request.method).toBe('GET');

        req.flush([{}]);

        expect(data).toHaveLength(1);
    });

    it('should create new glossary', () => {
        let data: GlossaryDetail;
        service.create(glossaryModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/glossaries`);
        expect(req.request.body).toEqual(glossaryModification);
        expect(req.request.method).toBe('POST');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });

    it('should update glossary', () => {
        let data: GlossaryDetail;
        service.update(glossaryModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/glossaries/${glossaryModification.id}`);
        expect(req.request.body).toEqual(glossaryModification);
        expect(req.request.method).toBe('PUT');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });

    it('should get delete glossary', () => {
        service.delete(1).subscribe();

        const req = httpMock.expectOne(`/api/glossaries/1`);
        expect(req.request.method).toBe('DELETE');
    });

    it('should get glossaries for modification', () => {
        let data: GlossarySummary[];
        service.getModificationGlossaries().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/glossaries/modification`);
        expect(req.request.method).toBe('GET');

        const expectedData = [{id: 1} as GlossarySummary, {id: 2} as GlossarySummary];
        req.flush(expectedData);

        expect(data!).toBeDefined();
        expect(data!).toEqual(expectedData);
    });
});
