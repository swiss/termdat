import {TestBed} from '@angular/core/testing';
import {ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {CanLockedComponentDeactivate} from './CanLockedComponentDeactivate';
import {EntryLockedGuard} from './entry-locked.guard';

describe('EntryLockedGuard', () => {
    let guard: EntryLockedGuard;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        guard = TestBed.inject(EntryLockedGuard);
    });

    it('should be created', () => {
        expect(guard).toBeTruthy();
    });

    it('should call canDeactivate on component', () => {
        const component = {
            canLockedDeactivate: jest.fn().mockReturnValue(true),
            canLockedUnload: jest.fn(),
        } as CanLockedComponentDeactivate;

        const canDeactivate = guard.canDeactivate(component, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined);

        expect(component.canLockedDeactivate).toHaveBeenCalledTimes(1);
        expect(component.canLockedUnload).toHaveBeenCalledTimes(0);

        expect(canDeactivate).toBe(true);
    });

    it('should do nothing when component is not yet lazy loaded', () => {
        const component = null;

        const canDeactivate = guard.canDeactivate(
            component as unknown as CanLockedComponentDeactivate,
            {} as ActivatedRouteSnapshot,
            {} as RouterStateSnapshot,
            undefined
        );

        expect(canDeactivate).toBe(true);
    });
});
