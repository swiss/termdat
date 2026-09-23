import {TestBed} from '@angular/core/testing';
import {ActivatedRouteSnapshot, Router, RouterStateSnapshot} from '@angular/router';
import {RedirectGuard} from './redirect.guard';

describe('RedirectGuard', () => {
    let guard: RedirectGuard;
    const routerMock = {
        navigate: jest.fn(),
    };
    const routeMock = {
        params: {
            id: 666,
        },
        queryParams: {
            strLanguages: '2-6-7-8-3',
            isPartialView: 0,
        },
    } as unknown as ActivatedRouteSnapshot;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [{provide: Router, useValue: routerMock}],
        });
        guard = TestBed.inject(RedirectGuard);
    });

    it('should be created', () => {
        expect(guard).toBeTruthy();
    });

    it('will redirect without queryParams', () => {
        guard.canActivate(routeMock, {} as RouterStateSnapshot);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['search', 'entry', 666], {queryParams: {}});
    });
});
