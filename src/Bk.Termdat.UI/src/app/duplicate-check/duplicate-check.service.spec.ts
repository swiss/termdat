import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {DuplicateCheckRequest} from 'api/DuplicateCheckRequest';
import {DuplicateCheckResponse} from 'api/DuplicateCheckResponse';
import * as downloadHelper from '../shared/download.helper';
import {DuplicateCheckService} from './duplicate-check.service';

describe('DuplicateCheckService', () => {
    let service: DuplicateCheckService;
    let httpMock: HttpTestingController;

    const translateServiceMock = {
        instant: jest.fn(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [DuplicateCheckService, {provide: TranslateService, useValue: translateServiceMock}, provideHttpClient(), provideHttpClientTesting()],
        });

        service = TestBed.inject(DuplicateCheckService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should perform duplicate check', () => {
        const request: DuplicateCheckRequest = {
            languageId: 1,
            collectionId: 2,
        };

        const response: DuplicateCheckResponse = {
            ids: [1, 2, 3],
        };

        service.performDuplicateCheck(request).subscribe(result => {
            expect(result).toEqual(response);
        });

        const req = httpMock.expectOne('/api/search/duplicate-check');

        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);

        req.flush(response);
    });

    it('should perform duplicate check xlsx', () => {
        const request: DuplicateCheckRequest = {
            languageId: 1,
            collectionId: 2,
        };

        service.exportDuplicateCheckXlsx(request).subscribe();

        const req = httpMock.expectOne('/api/search/duplicate-check/xlsx');

        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);
        expect(req.request.responseType).toBe('blob');

        req.flush(new Blob());
    });

    it('should create txt export from entry ids', () => {
        const downloadTxtFileSpy = jest.spyOn(downloadHelper, 'downloadTxtFile').mockImplementation(() => undefined);

        translateServiceMock.instant.mockReturnValue('Eintrags-IDs_20260702.txt');

        service.exportDuplicateCheckTxt([1, 2, 3]);

        expect(downloadTxtFileSpy).toHaveBeenCalledWith('1,2,3', 'Eintrags-IDs_20260702.txt');
    });
});
