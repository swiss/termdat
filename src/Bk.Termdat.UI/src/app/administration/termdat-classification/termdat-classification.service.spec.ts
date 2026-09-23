import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {DomainModification} from 'api/DomainModification';
import {LanguageDescription} from 'api/LanguageDescription';
import {SubjectDetail} from 'api/SubjectDetail';
import {SubjectModification} from 'api/SubjectModification';
import {SubjectSummary} from 'api/SubjectSummary';
import {TermdatClassificationDetail} from 'api/TermdatClassificationDetail';
import {TermdatClassificationService} from './termdat-classification.service';

describe('TermdatClassificationService', () => {
    let service: TermdatClassificationService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(TermdatClassificationService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get subject summaries', done => {
        const data: SubjectSummary[] = [
            {id: 1, name: 'Test Name 1', domains: []},
            {id: 2, name: 'Test Name 2', domains: []},
        ];

        service.getSubjects().subscribe(response => {
            expect(response).toBeDefined();
            expect(response).toHaveLength(2);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: '/api/subjects'});

        req.flush(data);
    });

    it('should get subject details', done => {
        const detailMock: SubjectDetail = {
            id: 111,
            name: 'Fachgebiet 1',
            entryCount: 25,
            created: new Date('2021-01-22T13:05:05.99'),
            modified: new Date('2021-09-29T09:05:05.99'),
            createdBy: 'test@test.ch',
            modifiedBy: 'tester@tester.ch',
            domains: [
                {
                    id: 222,
                    name: 'sachgebiet 1',
                    subjectId: 111,
                    entryCount: 20,
                    created: new Date('2021-01-22T13:05:05.99'),
                    modified: new Date('2021-09-29T09:05:05.99'),
                    createdBy: 'test@test.ch',
                    modifiedBy: 'tester@tester.ch',
                },
            ],
        };

        service.getSubjectDetails().subscribe((result: TermdatClassificationDetail[]) => {
            expect(result).toBeDefined();
            expect(result).toHaveLength(2);
            const [tClassificationSubject, tClassifacation] = result;
            expect(tClassificationSubject.id).toBe(111);
            expect(tClassificationSubject.subjectName).toBe('Fachgebiet 1');
            expect(tClassificationSubject.domainId).toBeUndefined();
            expect(tClassificationSubject.domainName).toBeUndefined();
            expect(tClassifacation.id).toBe(111);
            expect(tClassifacation.subjectName).toBe('Fachgebiet 1');
            expect(tClassifacation.domainId).toBe(222);
            expect(tClassifacation.domainName).toBe('sachgebiet 1');
            done();
        });

        const req = httpMock.expectOne(`/api/subjects/details`);
        expect(req.request.method).toBe('GET');

        req.flush([detailMock]);
    });

    it('should get subject by id', done => {
        const data: SubjectModification = {
            id: 2,
            descriptionDe: 'DE Description 2',
            descriptionFr: 'FR Description 2',
            descriptionEn: 'EN Description 2',
            descriptionRm: 'RM Description 2',
            descriptionIt: 'IT Description 2',
            domains: [],
        };

        service.getSubject(2).subscribe(response => {
            expect(response.id).toEqual(2);
            expect(response.descriptionDe).toEqual(data.descriptionDe);
            expect(response.descriptionFr).toEqual(data.descriptionFr);
            expect(response.descriptionEn).toEqual(data.descriptionEn);
            expect(response.descriptionRm).toEqual(data.descriptionRm);
            expect(response.descriptionIt).toEqual(data.descriptionIt);
            expect(response.domains).toHaveLength(0);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: '/api/subjects/2'});

        req.flush(data);
    });

    it('should create new subject', done => {
        const data: SubjectModification = {
            descriptionDe: 'DE Description 21',
            descriptionFr: 'FR Description 21',
            descriptionEn: 'EN Description 21',
            descriptionRm: 'RM Description 21',
            descriptionIt: 'IT Description 21',
            domains: [
                {
                    descriptionDe: 'DE Domain Description 21',
                    descriptionFr: 'FR Domain Description 21',
                    descriptionEn: 'EN Domain Description 21',
                    descriptionRm: 'RM Domain Description 21',
                    descriptionIt: 'IT Domain Description 21',
                },
            ],
        };

        service.createSubject(data).subscribe(response => {
            expect(response).toBeDefined();
            expect(response.id).toEqual(42);
            expect(response.name).toEqual(data.descriptionDe);
            expect(response.domains).toHaveLength(1);
            expect(response.created).toBeInstanceOf(Date);
            expect(response.modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'POST', url: `/api/subjects`});

        req.flush({
            id: 42,
            name: 'DE Description 21',
            domains: [{}],
            entryCount: 0,
            created: '2013-02-22T23:05:05.78' as unknown as Date,
            createdBy: 'user@test.ch',
            modified: '2013-02-23T23:05:05.78' as unknown as Date,
            modifiedBy: 'user@test.ch',
        });
    });

    it('should update subject', done => {
        const data: SubjectModification = {
            id: 7,
            descriptionDe: 'DE Description 28',
            descriptionFr: 'FR Description 28',
            descriptionEn: 'EN Description 28',
            descriptionRm: 'RM Description 28',
            descriptionIt: 'IT Description 28',
            domains: [],
        };

        service.updateSubject(data).subscribe(response => {
            expect(response).toBeDefined();
            expect(response.id).toEqual(data.id);
            expect(response.name).toEqual(data.descriptionDe);
            expect(response.domains).toHaveLength(0);
            expect(response.created).toBeInstanceOf(Date);
            expect(response.modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'PUT', url: `/api/subjects/${data.id}`});

        req.flush({
            id: data.id,
            name: data.descriptionDe,
            entryCount: 0,
            created: '2013-02-22T23:05:05.78' as unknown as Date,
            createdBy: 'user@test.ch',
            modified: '2013-02-23T23:05:05.78' as unknown as Date,
            modifiedBy: 'user@test.ch',
            domains: [],
        });
    });

    it('should delete subject', () => {
        service.deleteSubject(111).subscribe();

        const req = httpMock.expectOne({method: 'DELETE', url: '/api/subjects/111'});

        req.flush({});
    });

    it('should get domain language descriptions', done => {
        const data: LanguageDescription[] = [
            {
                descriptionDe: 'DE Test Descrition 1',
                descriptionFr: 'FR Test Descrition 1',
                descriptionEn: 'EN Test Descrition 1',
                descriptionRm: 'RM Test Descrition 1',
                descriptionIt: 'IT Test Descrition 1',
            },
            {
                descriptionDe: 'DE Test Descrition 987',
                descriptionFr: 'FR Test Descrition 987',
                descriptionEn: 'EN Test Descrition 987',
                descriptionRm: 'RM Test Descrition 987',
                descriptionIt: 'IT Test Descrition 987',
            },
            {
                descriptionDe: 'DE Test Descrition 42',
                descriptionFr: 'FR Test Descrition 42',
                descriptionEn: 'EN Test Descrition 42',
                descriptionRm: 'RM Test Descrition 42',
                descriptionIt: 'IT Test Descrition 42',
            },
        ];

        service.getDomainLanguageDescriptions().subscribe(response => {
            expect(response).toBeDefined();
            expect(response).toHaveLength(3);

            done();
        });

        const req = httpMock.expectOne({method: 'GET', url: '/api/domains/descriptions'});

        req.flush(data);
    });

    it('should update domain', done => {
        const data: DomainModification = {
            id: 200,
            descriptionDe: 'DE Domain Description 200',
            descriptionFr: 'FR Domain Description 200',
            descriptionEn: 'EN Domain Description 200',
            descriptionRm: 'RM Domain Description 200',
            descriptionIt: 'IT Domain Description 200',
        };

        service.updateDomain(data).subscribe(response => {
            expect(response).toBeDefined();
            expect(response.id).toEqual(data.id);
            expect(response.subjectId).toEqual(-1);
            expect(response.name).toEqual(data.descriptionDe);
            expect(response.created).toBeInstanceOf(Date);
            expect(response.modified).toBeInstanceOf(Date);

            done();
        });

        const req = httpMock.expectOne({method: 'PUT', url: `/api/domains/${data.id}`});

        req.flush({
            id: data.id,
            subjectId: -1,
            name: data.descriptionDe,
            entryCount: 0,
            created: '2013-02-22T23:05:05.78' as unknown as Date,
            createdBy: 'user@test.ch',
            modified: '2013-02-23T23:05:05.78' as unknown as Date,
            modifiedBy: 'user@test.ch',
        });
    });

    it('should delete domain', done => {
        const id = 111;

        service.deleteDomain(id).subscribe(() => {
            done();
        });

        const req = httpMock.expectOne({method: 'DELETE', url: `/api/domains/${id}`});

        req.flush({});
    });
});
