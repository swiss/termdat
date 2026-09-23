import {HttpClient, provideHttpClient, withInterceptors} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {Router} from '@angular/router';
import {authErrorInterceptor} from './auth-error.interceptor';

describe('AuthErrorInterceptor', () => {
    let httpClient: HttpClient;
    let httpTestingController: HttpTestingController;

    const routerMock = {
        navigate: jest.fn(),
        url: '/watchlist',
    };

    beforeEach(() => {
        routerMock.url = '/watchlist';
        TestBed.configureTestingModule({
            providers: [provideHttpClient(withInterceptors([authErrorInterceptor])), provideHttpClientTesting(), {provide: Router, useValue: routerMock}],
        });

        httpClient = TestBed.inject(HttpClient);
        httpTestingController = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpTestingController.verify();
        jest.resetAllMocks();
    });

    it('redirects to unauthorized for 401 API errors on protected routes', () => {
        httpClient.get('/api/search/search').subscribe({
            next: () => {
                throw new Error('Expected an error response');
            },
            error: error => {
                expect(error.status).toBe(401);
            },
        });

        const req = httpTestingController.expectOne('/api/search/search');
        req.flush(null, {status: 401, statusText: 'Unauthorized'});

        expect(routerMock.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it.each(['/', '/search', '/search/entry/123'])('does not redirect to unauthorized for 401 API errors on public route %s', route => {
        routerMock.url = route;

        httpClient.get('/api/languages/get-by-iso-code?isoCode=de').subscribe({
            next: () => {
                throw new Error('Expected an error response');
            },
            error: error => {
                expect(error.status).toBe(401);
            },
        });

        const req = httpTestingController.expectOne('/api/languages/get-by-iso-code?isoCode=de');
        req.flush(null, {status: 401, statusText: 'Unauthorized'});

        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('does not redirect for non-API 401 errors', () => {
        httpClient.get('/auth/user').subscribe({
            next: () => {
                throw new Error('Expected an error response');
            },
            error: error => {
                expect(error.status).toBe(401);
            },
        });

        const req = httpTestingController.expectOne('/auth/user');
        req.flush(null, {status: 401, statusText: 'Unauthorized'});

        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('does not redirect for non-401 API errors', () => {
        httpClient.get('/api/search/search').subscribe({
            next: () => {
                throw new Error('Expected an error response');
            },
            error: error => {
                expect(error.status).toBe(500);
            },
        });

        const req = httpTestingController.expectOne('/api/search/search');
        req.flush(null, {status: 500, statusText: 'Internal Server Error'});

        expect(routerMock.navigate).not.toHaveBeenCalled();
    });
});
