import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {LanguageHeaderInterceptor} from './language-header.interceptor';

describe('LanguageHeaderInterceptor', () => {
    let interceptor: LanguageHeaderInterceptor;

    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'fr'),
    };

    const headerMock = {
        append: jest.fn(),
        test: '',
    };

    const requestMock = {
        clone: jest.fn(),
        headers: headerMock,
    };

    const nextMock = {
        handle: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [LanguageHeaderInterceptor, {provide: TranslateService, useValue: translateServiceMock}],
        });

        headerMock.test = 'test';
        headerMock.append.mockImplementation(() => headerMock);
        requestMock.clone.mockImplementation(() => requestMock);

        interceptor = TestBed.inject(LanguageHeaderInterceptor);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should be created', () => {
        expect(interceptor).toBeTruthy();
    });

    it('should intercept and add the current language', () => {
        // eslint-disable-next-line
        interceptor.intercept(requestMock as any, nextMock);
        expect(requestMock.clone).toHaveBeenCalledTimes(1);
        expect(requestMock.clone).toHaveBeenCalledWith({headers: headerMock});
        expect(nextMock.handle).toHaveBeenCalledTimes(1);
        expect(nextMock.handle).toHaveBeenCalledWith(requestMock);
        expect(headerMock.append).toHaveBeenCalledTimes(1);
        expect(headerMock.append).toHaveBeenCalledWith('Accept-Language', 'fr');
    });

    it('should intercept and add de when no current language is set', () => {
        translateServiceMock.getCurrentLang = jest.fn(() => '');

        // eslint-disable-next-line
        interceptor.intercept(requestMock as any, nextMock);
        expect(requestMock.clone).toHaveBeenCalledTimes(1);
        expect(requestMock.clone).toHaveBeenCalledWith({headers: headerMock});
        expect(nextMock.handle).toHaveBeenCalledTimes(1);
        expect(nextMock.handle).toHaveBeenCalledWith(requestMock);
        expect(headerMock.append).toHaveBeenCalledTimes(1);
        expect(headerMock.append).toHaveBeenCalledWith('Accept-Language', 'de');
    });
});
