import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {FileImport} from 'api/FileImport';
import {FileImportResult} from 'api/FileImportResult';
import {FileImportService} from './file-import.service';

describe('FileImportService', () => {
    let service: FileImportService;
    let httpMock: HttpTestingController;

    const newFile = {
        name: 'test.tbx',
    } as File;

    const fileImport = {
        entryOffice: 1,
        entryCollection: 2,
        reliability: 3,
        status: 4,
        comment: 'Test',
        subjectIds: [11, 12],
        domainIds: [22, 23],
        glossaryIds: [33, 34],
        adminInfos: [],
        file: newFile,
    } as unknown as FileImport;

    const responseMock = {
        createdEntries: 1,
        numberOfErrors: 2,
        errorDetails: [],
    } as unknown as FileImportResult;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(FileImportService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should import a file', () => {
        let data: FileImportResult;
        service.importFile(fileImport).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/files/entries:import`);
        expect(req.request.method).toBe('POST');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });
});
