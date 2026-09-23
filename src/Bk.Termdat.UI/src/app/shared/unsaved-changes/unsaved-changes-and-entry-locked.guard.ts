import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable, concatMap, from, last, takeWhile} from 'rxjs';
import {EntryLockedGuard} from '../../entry/entry-locked/entry-locked.guard';
import {EntryUpdateComponent} from '../../entry/entry-update/entry-update.component';
import {UnsavedChangesGuard} from './unsaved-changes.guard';

type GuardReturnType = Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree;

@Injectable({
    providedIn: 'root',
})
export class UnsavedChangesAndEntryLockedGuard {
    /* eslint-disable @typescript-eslint/no-unused-vars */
    constructor(
        private readonly unsavedChangesGuard: UnsavedChangesGuard,
        private readonly entryLockedGuard: EntryLockedGuard
    ) {}
    canDeactivate(
        component: EntryUpdateComponent,
        currentRoute: ActivatedRouteSnapshot,
        currentState: RouterStateSnapshot,
        nextState?: RouterStateSnapshot
    ): GuardReturnType {
        if (!component) {
            return true;
        }

        return from([this.unsavedChangesGuard, this.entryLockedGuard]).pipe(
            concatMap(guard => guard.canDeactivate(component, currentRoute, currentState) as Observable<boolean>),
            takeWhile(guardResult => guardResult, true),
            last()
        );
    }
}
