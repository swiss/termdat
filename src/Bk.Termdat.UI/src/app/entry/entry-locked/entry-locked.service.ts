import {Injectable, RendererFactory2} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {CanLockedComponentDeactivate} from './CanLockedComponentDeactivate';

@Injectable({
    providedIn: 'root',
})
export class EntryLockedService {
    private watchedComponents: {[key: string]: CanLockedComponentDeactivate} = {};

    constructor(
        private readonly translateService: TranslateService,
        private readonly rendererFactory2: RendererFactory2
    ) {
        const renderer = this.rendererFactory2.createRenderer(null, null);
        renderer.listen('window', 'beforeunload', event => this.onUnload(event));
        renderer.listen('window', 'unload', event => this.onUnload(event));
    }

    watch(id: string, component: CanLockedComponentDeactivate) {
        this.watchedComponents[id] = component;
    }

    unwatch(id: string) {
        delete this.watchedComponents[id];
    }

    hasLockedEntries(): boolean {
        return Object.entries(this.watchedComponents).some(([_, component]) => {
            return !component.canLockedUnload();
        });
    }

    private onUnload(event: BeforeUnloadEvent) {
        if (this.hasLockedEntries()) {
            const confirmationMessage = this.translateService.instant('entry.locked.quitMessage');
            event.returnValue = confirmationMessage;

            return confirmationMessage;
        }

        return null;
    }
}
