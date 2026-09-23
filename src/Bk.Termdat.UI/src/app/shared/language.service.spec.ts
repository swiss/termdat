import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {EventEmitter} from '@angular/core';
import {TestBed} from '@angular/core/testing';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {LanguageDetail} from 'api/LanguageDetail';
import {LanguageModification} from 'api/LanguageModification';
import {LanguageSummary} from 'api/LanguageSummary';
import {LanguageService} from './language.service';

/* eslint-disable dot-notation */
describe('LanguageService', () => {
    let service: LanguageService;
    let httpMock: HttpTestingController;

    const languageSummaries: LanguageSummary[] = [
        {
            id: 1,
            name: 'Deutsch',
            isoCode: 'De',
        } as LanguageSummary,
        {
            id: 2,
            name: 'Englisch',
            isoCode: 'eN',
        } as LanguageSummary,
    ];

    const languageDetailMock = {
        id: 2,
        isoCode: 'de',
        name: 'deutsch',
        created: '2022-01-22T13:05:05.99' as unknown as Date,
        modified: '2022-09-29T09:05:05.99' as unknown as Date,
        createdBy: 'test@tester.ch',
        modifiedBy: 'tester@tester.ch',
    } as unknown as LanguageDetail;

    const languageModification = {
        isoCode: 'de',
        descriptionDe: 'de',
        descriptionEn: 'en',
        descriptionFr: 'fr',
        descriptionIt: 'it',
        descriptionRm: 'rm',
        id: 1,
        sortIndex: 5,
        isUiLanguage: true,
    } as LanguageModification;

    const maxSortIndex = 105;

    const isoCode = 'de';
    const langChangeEventEmitter = new EventEmitter<LangChangeEvent>();
    const translateServiceMock = {
        onLangChange: langChangeEventEmitter,
        getCurrentLang: jest.fn(() => 'de'),
        use: jest.fn(),
    };

    const httpApiInterceptorEventsMock = {
        deactivateSpinnerOnNextAPICalls: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
            ],
        });
        translateServiceMock.getCurrentLang = jest.fn(() => 'de');
        service = TestBed.inject(LanguageService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get current language id', () => {
        service['currentLanguageSummaries'] = languageSummaries;
        const currentLangId = service.getCurrentLanguageId();
        expect(currentLangId).toEqual(1);
    });

    it('should reload the language translations', () => {
        service.init();
        const req = httpMock.expectOne(`/api/languages/get-by-iso-code?isoCode=${isoCode}`);
        expect(req.request.method).toBe('GET');
        req.flush({});
        httpMock.verify();
    });

    it('should reload the language translations after a language change', () => {
        service.init();

        const initialReq = httpMock.expectOne(`/api/languages/get-by-iso-code?isoCode=${isoCode}`);
        initialReq.flush({});

        translateServiceMock.onLangChange.next({lang: 'fr'} as LangChangeEvent);

        const changedReq = httpMock.expectOne('/api/languages/get-by-iso-code?isoCode=fr');
        expect(changedReq.request.method).toBe('GET');
        changedReq.flush({});

        httpMock.verify();
    });

    it('should destroy properly', () => {
        jest.spyOn(service['unsubscribe'], 'next');
        jest.spyOn(service['unsubscribe'], 'complete');
        service.ngOnDestroy();
        expect(service['unsubscribe'].next).toHaveBeenCalledTimes(1);
        expect(service['unsubscribe'].complete).toHaveBeenCalledTimes(1);
    });

    it('should return languages as an observable', () => {
        let languages: LanguageSummary[];
        service.languageSummaries$.subscribe(data => (languages = data));

        service['languageSubject'].next(languageSummaries);

        expect(languages!).toEqual(languageSummaries);
    });

    it('should get the language name', () => {
        service['currentLanguageSummaries'] = languageSummaries;

        expect(service.getLanguageName(languageSummaries[0].id)).toBe(languageSummaries[0].name);
        expect(service.getLanguageName(5)).toBe('5');
    });

    it('should load current language translations', () => {
        let languages: LanguageSummary[];
        service.getCurrentLanguageSummaries().subscribe(data => (languages = data));

        const req = httpMock.expectOne(`/api/languages/get-by-iso-code?isoCode=de`);
        expect(req.request.method).toBe('GET');
        req.flush(languageSummaries);

        expect(languages!).toHaveLength(2);
        expect(languages![0].id).toEqual(1);
        expect(languages![1].id).toEqual(2);

        httpMock.verify();
    });

    it('should load requested language translations', () => {
        const languageId = 'fr';
        let languages: LanguageSummary[];
        service.getLanguage(languageId).subscribe(data => (languages = data));

        const req = httpMock.expectOne(`/api/languages/get-by-iso-code?isoCode=${languageId}`);
        expect(req.request.method).toBe('GET');
        req.flush(languageSummaries);

        expect(languages!).toHaveLength(2);
        expect(languages![0].id).toEqual(1);
        expect(languages![1].id).toEqual(2);

        httpMock.verify();
    });

    it('should get next sort index', () => {
        service.nextSort().subscribe(() => {});

        const req = httpMock.expectOne('/api/languages/next-sort');
        expect(req.request.method).toBe('GET');
        req.flush(maxSortIndex);

        expect(maxSortIndex).toEqual(105);

        httpMock.verify();
    });

    it('should create new office', () => {
        let data: LanguageDetail;
        service.create(languageModification).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/languages`);
        expect(req.request.body).toEqual(languageModification);
        expect(req.request.method).toBe('POST');

        req.flush(languageDetailMock);

        expect(data!).toEqual(languageDetailMock);
    });
    it('should get languageModification', () => {
        let data: LanguageModification;
        service.get(1).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/languages/1');
        expect(req.request.method).toBe('GET');

        req.flush(languageModification);

        expect(data!).toEqual(languageModification);
    });
    it('should update office', () => {
        let data: LanguageDetail;
        service.update(languageModification).subscribe(result => (data = result));

        const req = httpMock.expectOne('/api/languages/1');
        expect(req.request.body).toEqual(languageModification);
        expect(req.request.method).toBe('PUT');

        req.flush(languageDetailMock);

        expect(data!).toEqual(languageDetailMock);
    });

    it.each([
        ['de', true],
        ['fr', false],
    ])('should compare currentlang', (lang, expected) => {
        translateServiceMock.getCurrentLang = jest.fn(() => 'de');
        const result = service.isLangActive(lang);
        expect(result).toEqual(expected);
    });

    it('should change lang', () => {
        service.changeLang('lang');
        expect(translateServiceMock.use).toHaveBeenCalledWith('lang');
    });
});
