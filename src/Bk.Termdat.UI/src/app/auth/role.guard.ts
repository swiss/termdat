import {Inject, Injectable, OnDestroy} from '@angular/core';
import {ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable, Subject, map, take} from 'rxjs';
import {CONSOLE} from '../shared/injection.tokens';
import {AuthService} from './auth.service';
import {Role} from './Role';

@Injectable({
    providedIn: 'root',
})
export class RoleGuard implements OnDestroy {
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly router: Router,
        private readonly authService: AuthService,
        @Inject(CONSOLE) private readonly console: Console
    ) {}

    canActivate(_route: ActivatedRouteSnapshot, _: RouterStateSnapshot): Observable<boolean | UrlTree> {
        const roles = _route.data?.roles as Role[] | undefined;

        return this.authService.authCompleted$.pipe(
            take(1),
            // Read signal values directly — authState is already finalized when authCompleted$ fires,
            // so we avoid the async timing dependency of toObservable/effects.
            map(() => {
                const isAuthenticated = this.authService.isAuthenticated();
                const rs = this.authService.roles();

                if (!isAuthenticated) {
                    this.authService.login();
                    return false;
                }
                if (roles) {
                    if (rs.some(r => roles.includes(r))) {
                        return true;
                    }
                    return this.router.createUrlTree(['/unauthorized']);
                } else if (rs.length > 0) {
                    return true;
                }
                this.console.error('User does not have any valid role configuration.');
                return this.router.createUrlTree(['/unauthorized']);
            })
        );
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }
}
