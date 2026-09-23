import {TestBed} from '@angular/core/testing';
import {BehaviorSubject} from 'rxjs';
import {AuthService} from '../auth/auth.service';
import {Role} from '../auth/Role';
import {PageSizeService} from './page-size.service';
import {authenticatedPageSize, defaultPageSize, superReaderPageSize, writerPageSize} from './PaginationMapper';

describe('PageSizeService', () => {
    let service: PageSizeService;

    const rolesSubject = new BehaviorSubject<Role[]>([]);
    const authServiceMock = {
        roles$: rolesSubject.asObservable(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [{provide: AuthService, useValue: authServiceMock}],
        });

        service = TestBed.inject(PageSizeService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('returns default pageSize for pageSize$', done => {
        service.pageSize$.subscribe(pageSize => {
            expect(pageSize).toBeDefined();
            expect(pageSize).toEqual(defaultPageSize);

            done();
        });
    });

    it.each([
        {role: Role.Admin, pageSize: authenticatedPageSize},
        {role: Role.SuperUser, pageSize: authenticatedPageSize},
        {role: Role.Writer, pageSize: writerPageSize},
        {role: Role.SuperReader, pageSize: superReaderPageSize},
        {role: null, pageSize: defaultPageSize},
    ])('returns authenticatedPageSize for pageSize$ when user has role $role', ({role, pageSize}) => {
        let actualPageSize;
        service.pageSize$.subscribe(value => (actualPageSize = value));

        rolesSubject.next([role as Role]);

        expect(actualPageSize).toBeDefined();
        expect(actualPageSize).toEqual(pageSize);
    });

    it('resets to default pageSize for pageSize$ when role is removed', () => {
        let size;
        service.pageSize$.subscribe(pageSize => (size = pageSize));

        rolesSubject.next([Role.Admin]);

        expect(size).toBeDefined();
        expect(size).toEqual(authenticatedPageSize);

        rolesSubject.next([]);

        expect(size).toEqual(defaultPageSize);
    });
});
