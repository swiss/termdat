import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {CanComponentDeactivate} from './CanComponentDeactivate';

type GuardReturnType = Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree;

@Injectable({
    providedIn: 'root',
})
export class UnsavedChangesGuard {
    /* eslint-disable @typescript-eslint/no-unused-vars */
    canDeactivate(
        component: CanComponentDeactivate,
        currentRoute: ActivatedRouteSnapshot,
        currentState: RouterStateSnapshot,
        nextState?: RouterStateSnapshot
        /* eslint-enable */
    ): GuardReturnType {
        if (!component) {
            return true;
        }

        if (typeof component.canDeactivate !== 'function') {
            throw new Error('UnsavedChangesGuard can only be used for components that implement CanComponentDeactivate');
        }

        return component.canDeactivate();
    }
}
