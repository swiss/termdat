import {TestBed} from '@angular/core/testing';
import {ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {CanComponentDeactivate} from './CanComponentDeactivate';
import {UnsavedChangesGuard} from './unsaved-changes.guard';

describe('UnsavedChangesGuard', () => {
    let guard: UnsavedChangesGuard;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        guard = TestBed.inject(UnsavedChangesGuard);
    });

    it('should be created', () => {
        expect(guard).toBeTruthy();
    });

    it('should call canDeactivate on component', () => {
        const component = {
            canDeactivate: jest.fn(),
            canUnload: jest.fn(),
        };
        guard.canDeactivate(component, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined);

        expect(component.canDeactivate).toHaveBeenCalledTimes(1);
        expect(component.canUnload).toHaveBeenCalledTimes(0);
    });

    it('should throw exception when canDeactivate is not a callable', () => {
        const component = {
            canDeactivate: undefined,
            canUnload: jest.fn(),
        };
        expect(() => {
            guard.canDeactivate(component as unknown as CanComponentDeactivate, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined);
        }).toThrow('UnsavedChangesGuard can only be used for components that implement CanComponentDeactivate');
    });

    it('should do nothing when component is not yet lazy loaded', () => {
        const component = null;

        const res = guard.canDeactivate(component as unknown as CanComponentDeactivate, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined);

        expect(res).toBe(true);
    });
});
