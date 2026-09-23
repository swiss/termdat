import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {CanLockedComponentDeactivate} from './CanLockedComponentDeactivate';

type GuardReturnType = Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree;

@Injectable({
    providedIn: 'root',
})
export class EntryLockedGuard {
    /* eslint-disable @typescript-eslint/no-unused-vars */
    canDeactivate(
        component: CanLockedComponentDeactivate,
        currentRoute: ActivatedRouteSnapshot,
        currentState: RouterStateSnapshot,
        nextState?: RouterStateSnapshot
        /* eslint-enable */
    ): GuardReturnType {
        if (!component) {
            return true;
        }

        return component.canLockedDeactivate();
    }
}
