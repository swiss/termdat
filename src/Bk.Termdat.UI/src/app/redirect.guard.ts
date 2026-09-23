import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class RedirectGuard {
    constructor(private readonly router: Router) {}

    canActivate(route: ActivatedRouteSnapshot, _: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
        void this.router.navigate(['search', 'entry', route.params.id], {queryParams: {}});
        return false;
    }
}
