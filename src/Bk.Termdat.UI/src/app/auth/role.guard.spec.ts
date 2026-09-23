import {TestBed} from '@angular/core/testing';
import {ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable, ReplaySubject} from 'rxjs';
import {CONSOLE} from '../shared/injection.tokens';
import {AuthService} from './auth.service';
import {Role} from './Role';
import {RoleGuard} from './role.guard';

describe('RoleGuard', () => {
    let guard: RoleGuard;

    const routerMock = {
        createUrlTree: jest.fn(),
    };

    let authCompletedSubject: ReplaySubject<boolean>;
    let authServiceMock: {
        isAuthenticated: jest.Mock;
        roles: jest.Mock;
        authCompleted$: Observable<boolean>;
        login: jest.Mock;
    };

    const routeMock = {} as ActivatedRouteSnapshot;
    const routeWithAdminRoleMock = {data: {roles: [Role.Admin]}} as unknown as ActivatedRouteSnapshot;
    const stateMock = {} as RouterStateSnapshot;
    const urlTree = {};
    const consoleMock = {
        error: jest.fn(),
    };

    beforeEach(() => {
        authCompletedSubject = new ReplaySubject<boolean>(1);
        authServiceMock = {
            isAuthenticated: jest.fn().mockReturnValue(false),
            roles: jest.fn().mockReturnValue([]),
            authCompleted$: authCompletedSubject.asObservable(),
            login: jest.fn(),
        };

        TestBed.configureTestingModule({
            providers: [
                {provide: Router, useValue: routerMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: CONSOLE, useValue: consoleMock},
            ],
        });
        guard = TestBed.inject(RoleGuard);
        routerMock.createUrlTree.mockReturnValue(urlTree);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should be created', () => {
        expect(guard).toBeTruthy();
    });

    it('waits for auth completion before evaluating roles', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authServiceMock.roles.mockReturnValue([Role.Admin]);

        const result = guard.canActivate(routeMock, stateMock) as Observable<boolean>;
        let emitted = false;

        result.subscribe(value => {
            emitted = true;
            expect(value).toBe(true);
            expect(routerMock.createUrlTree).not.toHaveBeenCalled();
            done();
        });

        expect(emitted).toBe(false);
        authCompletedSubject.next(true);
    });

    it('redirects to login when user is not authenticated', done => {
        // isAuthenticated returns false by default
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeMock, stateMock) as Observable<boolean>;

        result.subscribe(value => {
            expect(value).toBe(false);
            expect(authServiceMock.login).toHaveBeenCalledTimes(1);
            expect(routerMock.createUrlTree).not.toHaveBeenCalled();
            done();
        });
    });

    it('reroutes to unauthorized if authenticated user doesnt have any role', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeMock, stateMock) as Observable<UrlTree>;

        result.subscribe(value => {
            expect(value).toBe(urlTree);
            expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/unauthorized']);
            expect(consoleMock.error).toHaveBeenCalledTimes(1);
            expect(consoleMock.error).toHaveBeenCalledWith('User does not have any valid role configuration.');
            expect(authServiceMock.login).not.toHaveBeenCalled();
            done();
        });
    });

    it('doesnt reroute if user is admin', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authServiceMock.roles.mockReturnValue([Role.Admin]);
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeMock, stateMock) as Observable<boolean>;

        result.subscribe(value => {
            expect(value).toBe(true);
            expect(routerMock.createUrlTree).toHaveBeenCalledTimes(0);
            done();
        });
    });

    it('doesnt reroute if user is super user', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authServiceMock.roles.mockReturnValue([Role.SuperUser]);
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeMock, stateMock) as Observable<boolean>;
        result.subscribe(value => {
            expect(value).toBe(true);
            expect(routerMock.createUrlTree).toHaveBeenCalledTimes(0);
            done();
        });
    });

    it('doesnt reroute if user is writer', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authServiceMock.roles.mockReturnValue([Role.Writer]);
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeMock, stateMock) as Observable<boolean>;

        result.subscribe(value => {
            expect(value).toBe(true);
            expect(routerMock.createUrlTree).toHaveBeenCalledTimes(0);
            done();
        });
    });

    it('reroutes to unauthorized if authenticated user does not have required route role', done => {
        authServiceMock.isAuthenticated.mockReturnValue(true);
        authServiceMock.roles.mockReturnValue([Role.Writer]);
        authCompletedSubject.next(true);

        const result = guard.canActivate(routeWithAdminRoleMock, stateMock) as Observable<UrlTree>;

        result.subscribe(value => {
            expect(value).toBe(urlTree);
            expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/unauthorized']);
            expect(authServiceMock.login).not.toHaveBeenCalled();
            done();
        });
    });
});
