import {Inject, Injectable} from '@angular/core';
import {DisplayMode} from '../../../shared/DisplayMode';
import {LOCAL_STORAGE} from '../../../shared/injection.tokens';

@Injectable({
    providedIn: 'root',
})
export class DisplayService {
    readonly storageName: string = 'DisplayMode';

    // This is stupid, but TS does not generate a reverse map for string enums
    private readonly reverseMap: {[key: string]: DisplayMode} = {
        [DisplayMode.Complete]: DisplayMode.Complete,
        [DisplayMode.Context]: DisplayMode.Context,
        [DisplayMode.Definition]: DisplayMode.Definition,
        [DisplayMode.Minimal]: DisplayMode.Minimal,
    };

    constructor(@Inject(LOCAL_STORAGE) private readonly localStorage: Storage) {}

    public setDisplayMode(value: DisplayMode): void {
        this.localStorage.setItem(this.storageName, value);
    }

    public getDisplayMode(): DisplayMode {
        const displayMode = this.localStorage.getItem(this.storageName) ?? DisplayMode.Complete;
        return this.reverseMap[displayMode] ?? DisplayMode.Complete;
    }
}
