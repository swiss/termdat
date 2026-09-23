import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {ObNotificationService} from '@oblique/oblique';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {of, throwError} from 'rxjs';
import {first} from 'rxjs/operators';
import {UserAccountService} from '../administration/user-administration/user-account.service';
import {ConfigsService} from '../config.service';
import {ExtendedSearchFiltersService} from '../search/extended-search-filters.service';
import {SESSION_STORAGE, LOCATION} from '../shared/injection.tokens';
import {AuthService} from './auth.service';
import {AuthResponse} from './AuthResponse';

/* eslint-disable dot-notation */
describe('AuthService', () => {
    let service: AuthService;
    let httpTestingController: HttpTestingController;

    const userAccountDetail: UserAccountDetail = {
        email: 'p@peterson.com',
        firstName: 'paul',
        hasFullAccess: false,
        id: 1,
        isWriter: true,
        lastName: 'peterson',
        glossaries: [],
        collections: [{code: 'code1', deleted: false, id: 1, isObsolete: false, name: 'name', active: false, activeApi: false, exportLindas: false}],
        hasRole: false,
    };

    const sessionStorageMock = {
        clear: jest.fn(),
    };
    const locationMock = {href: '', pathname: '/watchlist', search: '?tab=details'};
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const userAccountServiceMock = {
        createOrUpdate: jest.fn(),
    };
    const notificationServiceMock = {
        error: jest.fn(),
    };
    const configServiceMock = {
        frontendConfig: {
            roles: {
                allow: 'ALLOW',
                admin: 'Admin',
                writer: 'Writer',
                superUser: 'SuperUser',
                superReader: 'SuperReader',
            },
        },
    };

    beforeEach(() => {
        locationMock.href = '';
        locationMock.pathname = '/watchlist';
        locationMock.search = '?tab=details';

        TestBed.configureTestingModule({
            providers: [
                {provide: UserAccountService, useValue: userAccountServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ConfigsService, useValue: configServiceMock},
                {provide: SESSION_STORAGE, useValue: sessionStorageMock},
                {provide: LOCATION, useValue: locationMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
            ],
        });

        service = TestBed.inject(AuthService);
        httpTestingController = TestBed.inject(HttpTestingController);
        userAccountServiceMock.createOrUpdate.mockReturnValue(of(userAccountDetail));
    });

    afterEach(() => {
        httpTestingController.verify();
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('Authentication state management via checkAuth', () => {
        it('should return authenticated as true when user is authorized', () => {
            const mockAuthResponse: AuthResponse = {
                isAuthenticated: true,
                claims: [],
            };

            let result: boolean;
            service
                .checkAuth()
                .pipe(first())
                .subscribe(isAuth => (result = isAuth));

            const req = httpTestingController.expectOne('/auth/user');
            expect(req.request.method).toBe('GET');
            req.flush(mockAuthResponse);

            expect(result!).toBe(true);
            expect(service.isAuthenticated()).toBe(true);
        });

        it('should return authenticated as false when user is unauthorized', () => {
            let result: boolean;
            service
                .checkAuth()
                .pipe(first())
                .subscribe(isAuth => (result = isAuth));

            const req = httpTestingController.expectOne('/auth/user');
            req.flush(null, {status: 401, statusText: 'Unauthorized'});

            expect(result!).toBe(false);
            expect(service.isAuthenticated()).toBe(false);
        });

        it('should handle error by setting authenticated to false', () => {
            let result: boolean;
            service
                .checkAuth()
                .pipe(first())
                .subscribe(isAuth => (result = isAuth));

            const req = httpTestingController.expectOne('/auth/user');
            req.flush(null, {status: 500, statusText: 'Internal Server Error'});

            expect(result!).toBe(false);
            expect(service.isAuthenticated()).toBe(false);
        });

        it('should call createOrUpdate and clear search filters when authenticated', () => {
            service.checkAuth().subscribe(result => {
                expect(result).toBe(true);
                expect(userAccountServiceMock.createOrUpdate).toHaveBeenCalledTimes(1);
                expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
            });

            const req = httpTestingController.expectOne('/auth/user');
            req.flush({isAuthenticated: true, claims: []});
        });

        it('should not call createOrUpdate when not authenticated', () => {
            service.checkAuth().subscribe(result => {
                expect(result).toBe(false);
                expect(userAccountServiceMock.createOrUpdate).not.toHaveBeenCalled();
            });

            const req = httpTestingController.expectOne('/auth/user');
            req.flush(null, {status: 401, statusText: 'Unauthorized'});
        });

        it('should notify on createOrUpdate error but still report authenticated', () => {
            userAccountServiceMock.createOrUpdate.mockReturnValue(throwError(() => ({})));

            service.checkAuth().subscribe(result => {
                expect(result).toBe(true);
                expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
                expect(notificationServiceMock.error).toHaveBeenCalledWith('userAccount.notification.createOrUpdateError');
                expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
            });

            const req = httpTestingController.expectOne('/auth/user');
            req.flush({isAuthenticated: true, claims: []});
        });

        it('should emit on authCompleted$ once checkAuth finishes', done => {
            service.authCompleted$.pipe(first()).subscribe(completed => {
                expect(completed).toBe(true);
                done();
            });

            service.checkAuth().subscribe();
            const req = httpTestingController.expectOne('/auth/user');
            req.flush({isAuthenticated: false, claims: []});
        });
    });

    describe('UserInfo and Roles signals in response to authentication', () => {
        it('should extract userInfo from claims when authenticated', () => {
            const mockAuthResponse: AuthResponse = {
                isAuthenticated: true,
                claims: [
                    {type: 'given_name', value: 'John'},
                    {type: 'family_name', value: 'Doe'},
                    {type: 'email', value: 'john.doe@example.com'},
                    {type: 'role', value: 'ALLOW'},
                    {type: 'role', value: 'Admin'},
                ],
            };
            service.checkAuth().subscribe();
            const req = httpTestingController.expectOne('/auth/user');
            req.flush(mockAuthResponse);

            const userInfo = service.userInfo();

            expect(userInfo.name).toBe('John Doe');
            expect(userInfo.email).toBe('john.doe@example.com');
            expect(userInfo.roles).toEqual(['ALLOW', 'Admin']);
        });

        it('should return empty userInfo when not authenticated', () => {
            const mockAuthResponse: AuthResponse = {
                isAuthenticated: false,
                claims: [],
            };
            service.checkAuth().subscribe();
            const req = httpTestingController.expectOne('/auth/user');
            req.flush(mockAuthResponse);

            const userInfo = service.userInfo();

            expect(userInfo).toEqual({});
        });

        it('should return empty roles when ALLOW role is missing', () => {
            const mockAuthResponse: AuthResponse = {
                isAuthenticated: true,
                claims: [
                    {type: 'role', value: 'Admin'},
                    {type: 'role', value: 'Writer'},
                ],
            };
            service.checkAuth().subscribe();
            const req = httpTestingController.expectOne('/auth/user');
            req.flush(mockAuthResponse);

            expect(service.roles()).toEqual([]);
        });

        it.each([
            [
                [
                    configServiceMock.frontendConfig.roles.allow,
                    configServiceMock.frontendConfig.roles.admin,
                    configServiceMock.frontendConfig.roles.superUser,
                    configServiceMock.frontendConfig.roles.superReader,
                    configServiceMock.frontendConfig.roles.writer,
                ],
                true,
                true,
                true,
                true,
            ],
            [[configServiceMock.frontendConfig.roles.allow, configServiceMock.frontendConfig.roles.superUser], false, true, false, false],
            [[configServiceMock.frontendConfig.roles.allow, configServiceMock.frontendConfig.roles.superReader], false, false, true, false],
            [[configServiceMock.frontendConfig.roles.allow, configServiceMock.frontendConfig.roles.writer], false, false, false, true],
            [[configServiceMock.frontendConfig.roles.allow, configServiceMock.frontendConfig.roles.admin], true, false, false, false],
            [[configServiceMock.frontendConfig.roles.allow], false, false, false, false],
        ])(
            'should compute isAdmin/isSuperUser/isSuperReader/isWriter for roles %s',
            (roles, expectedIsAdmin, expectedIsSuperUser, expectedIsSuperReader, expectedIsWriter) => {
                service.checkAuth().subscribe();
                const req = httpTestingController.expectOne('/auth/user');
                req.flush({isAuthenticated: true, claims: roles.map(role => ({type: 'role', value: role}))});

                service.isAdmin$.pipe(first()).subscribe(isAdmin => expect(isAdmin).toBe(expectedIsAdmin));
                service.isSuperUser$.pipe(first()).subscribe(isSuperUser => expect(isSuperUser).toBe(expectedIsSuperUser));
                service.isSuperReader$.pipe(first()).subscribe(isSuperReader => expect(isSuperReader).toBe(expectedIsSuperReader));
                service.isWriter$.pipe(first()).subscribe(isWriter => expect(isWriter).toBe(expectedIsWriter));
            }
        );
    });

    describe('Backend navigation methods', () => {
        it('should redirect to login endpoint with return URL and clear sessionStorage', () => {
            service.login();

            expect(locationMock.href).toBe('/auth/login?returnUrl=%2Fwatchlist%3Ftab%3Ddetails');
            expect(sessionStorageMock.clear).toHaveBeenCalledTimes(1);
        });

        it('should redirect to logout endpoint and clear sessionStorage', () => {
            service.logout();

            expect(locationMock.href).toBe('/auth/logout');
            expect(sessionStorageMock.clear).toHaveBeenCalledTimes(1);
        });
    });

    it('should unsubscribe', () => {
        jest.spyOn(service['unsubscribe'], 'next');
        jest.spyOn(service['unsubscribe'], 'complete');

        service.ngOnDestroy();

        expect(service['unsubscribe'].next).toHaveBeenCalled();
        expect(service['unsubscribe'].complete).toHaveBeenCalled();
    });
});
