import {Observable} from 'rxjs';

export interface CanLockedComponentDeactivate {
    canLockedDeactivate: () => Observable<boolean>;
    canLockedUnload: () => boolean;
}
