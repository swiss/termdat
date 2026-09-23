import {ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {Observable, of} from 'rxjs';
import {EntryUpdateComponent} from '../../entry/entry-update/entry-update.component';
import {UnsavedChangesAndEntryLockedGuard} from './unsaved-changes-and-entry-locked.guard';

describe('UnsavedChangesAndEntryLockedGuard', () => {
    let sut: UnsavedChangesAndEntryLockedGuard;

    const unsavedChangesGuardMock = {
        canDeactivate: jest.fn(),
    };

    const entryLockedGuard = {
        canDeactivate: jest.fn(),
    };

    beforeEach(() => {
        sut = new UnsavedChangesAndEntryLockedGuard(unsavedChangesGuardMock, entryLockedGuard);

        unsavedChangesGuardMock.canDeactivate.mockReturnValue(of(true));
        entryLockedGuard.canDeactivate.mockReturnValue(of(true));
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(sut).toBeTruthy();
    });

    it('should do nothing when component is not yet lazy loaded', () => {
        const component = null;

        const canDeactivate = sut.canDeactivate(
            component as unknown as EntryUpdateComponent,
            {} as ActivatedRouteSnapshot,
            {} as RouterStateSnapshot,
            undefined
        );

        expect(canDeactivate).toBe(true);
    });

    it('should call canDeactivate on UnsavedChangesGuard and EntryLockedGuard guards', () => {
        (sut.canDeactivate({} as EntryUpdateComponent, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined) as Observable<boolean>).subscribe();

        expect(unsavedChangesGuardMock.canDeactivate).toHaveBeenCalledTimes(1);
        expect(entryLockedGuard.canDeactivate).toHaveBeenCalledTimes(1);
    });

    it('should call canDeactivate on UnsavedChangesGuard but skip EntryLockedGuard guard', () => {
        unsavedChangesGuardMock.canDeactivate.mockReturnValue(of(false));

        (sut.canDeactivate({} as EntryUpdateComponent, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, undefined) as Observable<boolean>).subscribe();

        expect(unsavedChangesGuardMock.canDeactivate).toHaveBeenCalledTimes(1);
        expect(entryLockedGuard.canDeactivate).toHaveBeenCalledTimes(0);
    });
});
