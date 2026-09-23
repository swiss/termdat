import {Injectable, RendererFactory2} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {TranslateService} from '@ngx-translate/core';
import {Observable, of, Subject, Subscription} from 'rxjs';
import {debounceTime, first, map, switchMap} from 'rxjs/operators';
import {CanComponentDeactivate} from './CanComponentDeactivate';
import {UnsavedChangeAction} from './unsaved-change-action';
import {UnsavedChangesComponent} from './unsaved-changes.component';

@Injectable({
    providedIn: 'root',
})
export class UnsavedChangesService {
    unsavedChanges$: Observable<boolean>;
    private watchedComponents: {[key: string]: {component: CanComponentDeactivate; subscription: Subscription}} = {};
    private readonly changeSubject = new Subject<void>();

    constructor(
        private readonly dialog: MatDialog,
        private readonly translateService: TranslateService,
        private readonly rendererFactory2: RendererFactory2
    ) {
        const renderer = this.rendererFactory2.createRenderer(null, null);
        renderer.listen('window', 'beforeunload', event => this.onUnload(event));
        renderer.listen('window', 'unload', event => this.onUnload(event));

        this.unsavedChanges$ = this.changeSubject.asObservable().pipe(
            debounceTime(50),
            map(() => this.hasUnconfirmedChanges())
        );
    }

    watch(id: string, component: CanComponentDeactivate, form: AbstractControl) {
        this.watchedComponents[id] = {
            component,
            subscription: form.valueChanges.subscribe(() => this.changeSubject.next()),
        };
    }

    unwatch(id: string) {
        this.watchedComponents[id].subscription.unsubscribe();
        delete this.watchedComponents[id];
        this.changeSubject.next();
    }

    createDeactivateConfirmDialog(
        onSave: () => Observable<boolean>,
        onDiscard: () => void = () => {
            /* intenionally empty */
        }
    ): Observable<boolean> {
        return this.dialog
            .open(UnsavedChangesComponent)
            .afterClosed()
            .pipe(
                first(),
                switchMap((result: UnsavedChangeAction) => {
                    switch (result) {
                        case UnsavedChangeAction.SAVE:
                            return onSave();
                        case UnsavedChangeAction.DISCARD:
                            onDiscard();
                            return of(true);
                        default:
                            return of(false);
                    }
                })
            );
    }

    hasUnconfirmedChanges(): boolean {
        return Object.values(this.watchedComponents).some(c => !c.component.canUnload());
    }

    private onUnload(event: BeforeUnloadEvent) {
        if (this.hasUnconfirmedChanges()) {
            const confirmationMessage = this.translateService.instant('i18n.validation.unsavedChanges');
            event.returnValue = confirmationMessage;

            return confirmationMessage;
        }

        return null;
    }
}
