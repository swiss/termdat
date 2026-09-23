import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {SourceTemplatesService} from './source-templates.service';
import {SourceTemplateDefaults} from './SourceTemplateDefaults';

describe('SourceTemplatesService', () => {
    let service: SourceTemplatesService;
    let httpMock: HttpTestingController;

    const sourceTemplateCreate = {
        textDe: 'Text DE',
        textFr: 'Text FR',
        textIt: 'Text IT',
        textEn: 'Text EN',
        textRm: 'Text RM',
    };

    const sourceTemplateUpdate = {
        id: 1,
        textDe: 'Text DE',
        textFr: 'Text FR',
        textIt: 'Text IT',
        textEn: 'Text EN',
        textRm: 'Text RM',
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(SourceTemplatesService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get default source templates', () => {
        let templates: SourceTemplateDefaults;
        service.getSourceTemplateDefaults().subscribe(value => (templates = value));

        const req = httpMock.expectOne(`/api/source-templates`);
        expect(req.request.method).toEqual('GET');
        req.flush({
            de: {},
        });
        httpMock.verify();

        expect(templates!).toBeDefined();
        expect(templates!.de).toBeDefined();
    });

    it('should only get default source templates once on multiple cals', () => {
        let templates1: SourceTemplateDefaults;
        let templates2: SourceTemplateDefaults;
        service.getSourceTemplateDefaults().subscribe(value => (templates1 = value));
        service.getSourceTemplateDefaults().subscribe(value => (templates2 = value));

        const req = httpMock.expectOne(`/api/source-templates`);
        expect(req.request.method).toEqual('GET');
        req.flush({
            de: {},
        });
        httpMock.verify();

        expect(templates1!).toBeDefined();
        expect(templates1!.de).toBeDefined();
        expect(templates2!).toBeDefined();
        expect(templates2!.de).toBeDefined();
    });

    it('should get default source templates', () => {
        let templates: string[];
        service.getSourceTemplateTexts('de').subscribe(value => (templates = value));

        const req = httpMock.expectOne('/api/source-templates/current-user/texts/de');
        expect(req.request.method).toEqual('GET');
        req.flush(['TextDe', 'TextDe2']);
        httpMock.verify();

        expect(templates!).toBeDefined();
        expect(templates![0]).toBeDefined();
        expect(templates![1]).toBeDefined();
    });

    it('should reload source template texts after creating a source template', () => {
        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush(['TextDe']);

        service.create(sourceTemplateCreate).subscribe();

        httpMock.expectOne('/api/source-templates').flush({});

        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush(['TextDe', 'TextDe2']);
        httpMock.verify();
    });

    it('should reload source template texts after updating a source template', () => {
        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush(['TextDe']);

        service.update(sourceTemplateUpdate).subscribe();

        httpMock.expectOne('/api/source-templates/1').flush({});

        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush(['TextDe2']);
        httpMock.verify();
    });

    it('should reload source template texts after deleting a source template', () => {
        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush(['TextDe']);

        service.delete(1).subscribe();

        httpMock.expectOne('/api/source-templates/1').flush({});

        service.getSourceTemplateTexts('de').subscribe();

        httpMock.expectOne('/api/source-templates/current-user/texts/de').flush([]);
        httpMock.verify();
    });

    it('should create new source template', () => {
        service.create(sourceTemplateCreate).subscribe();

        const req = httpMock.expectOne('/api/source-templates');
        expect(req.request.body).toEqual(sourceTemplateCreate);
        expect(req.request.method).toBe('POST');
    });

    it('should update new source template', () => {
        service.update(sourceTemplateUpdate).subscribe();

        const req = httpMock.expectOne('/api/source-templates/1');
        expect(req.request.body).toEqual(sourceTemplateUpdate);
        expect(req.request.method).toBe('PUT');
    });

    it('should delete source template', () => {
        service.delete(1).subscribe();

        const req = httpMock.expectOne('/api/source-templates/1');
        expect(req.request.method).toBe('DELETE');
    });
});
