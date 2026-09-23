import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {CollectionSummary} from 'api/CollectionSummary';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {UserAccountModification} from 'api/UserAccountModification';
import {UserAccountService} from './user-account.service';

describe('UserAccountService', () => {
    let service: UserAccountService;
    let httpMock: HttpTestingController;

    const responseMock = {
        firstName: 'Peter',
        lastName: 'Mann',
        email: 'p.m@bit.ch',
        role: 'Admin',
        lastAction: new Date('2023-04-24 06:39:49.101275+00'),
        collections: [{id: 1, code: 'code1', deleted: false, isObsolete: false, name: 'collection one'} as CollectionSummary],
        glossaries: [{id: 2, code: 'code2', name: 'glossary two'}],
        hasFullAccess: true,
    } as UserAccountDetail;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        service = TestBed.inject(UserAccountService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should load user account summaries', () => {
        let data;
        service.getUserAccountSummaries().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/user-accounts`);
        expect(req.request.method).toBe('GET');

        req.flush([{}]);

        expect(data).toHaveLength(1);
    });

    it('should load user account details', () => {
        let data;
        service.getUserAccountDetails().subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/user-accounts/details`);
        expect(req.request.method).toBe('GET');

        req.flush([{}]);

        expect(data).toHaveLength(1);
    });

    it('should get user account details', () => {
        let data: UserAccountDetail;
        service.get(1).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/user-accounts/1`);
        expect(req.request.method).toBe('GET');

        req.flush(responseMock);

        expect(data!).toEqual(responseMock);
    });

    it('should update user account details', () => {
        const userAccountModification = {
            id: 1,
            authorizedCollectionIds: [1, 2],
            authorizedGlossaryIds: [3, 4],
        } as UserAccountModification;
        service.update(userAccountModification).subscribe(() => {});

        const req = httpMock.expectOne(`/api/user-accounts`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            id: 1,
            authorizedCollectionIds: [1, 2],
            authorizedGlossaryIds: [3, 4],
        });
        req.flush({});
    });

    it('should create or update user', () => {
        let update: UserAccountDetail;
        service.createOrUpdate().subscribe(result => (update = result));

        const req = httpMock.expectOne(`/api/user-accounts`);
        expect(req.request.method).toBe('POST');

        req.flush(responseMock);

        expect(update!).toEqual(responseMock);
    });
});
