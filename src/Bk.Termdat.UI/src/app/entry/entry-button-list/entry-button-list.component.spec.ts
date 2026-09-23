import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryDetail} from 'api/EntryDetail';
import {Status} from 'api/Status';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {MockModule} from 'ng-mocks';
import {BehaviorSubject, of, ReplaySubject, throwError} from 'rxjs';
import {AuthService} from '../../auth/auth.service';
import {Role} from '../../auth/Role';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {EntryService} from '../entry.service';
import {EntryButtonListComponent} from './entry-button-list.component';

describe('EntryButtonListComponent', () => {
    let component: EntryButtonListComponent;
    let fixture: ComponentFixture<EntryButtonListComponent>;
    let entryDetail: EntryDetail | undefined;
    const userAccountDetail: UserAccountDetail = {
        email: 'email',
        firstName: 'paul',
        id: 1,
        lastName: 'paulson',
        glossaries: [],
        hasFullAccess: false,
        isWriter: true,
        collections: [],
        hasRole: false,
    };

    const isAdminSubject = new BehaviorSubject(false);
    const isSuperUserSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);
    const userAccountDetailSubject = new ReplaySubject<UserAccountDetail>(1);
    const roles: Role[] = [];
    const rolesSubject = new BehaviorSubject(roles);
    const authServiceMock = {
        roles$: rolesSubject.asObservable(),
        isSuperUser$: isSuperUserSubject.asObservable(),
        isAdmin$: isAdminSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
        userAccountDetail$: userAccountDetailSubject.asObservable(),
    };
    const entryServiceMock = {
        delete: jest.fn(),
        restore: jest.fn(),
        unlock: jest.fn(),
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };

    const notificationServiceMock = {
        success: jest.fn(),
        warning: jest.fn(),
        error: jest.fn(),
    };

    const translateServiceMock = {
        instant: jest.fn(),
    };

    const activatedRouteMock = {};

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MockModule(MatButtonModule), MockModule(TranslateModule), EntryButtonListComponent],
            providers: [
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        }).compileComponents();

        entryDetail = {
            id: 1,
            status: Status.Validated,
            collection: {
                id: 10,
                code: 'VAC1',
                name: 'Value added collection',
            } as CollectionSummary,
            languageDetails: [
                {
                    abbreviation: 'short',
                    definition: 'def',
                    languageId: 1,
                    name: 'Search Detail',
                    phraseology: 'phraseology',
                    sequence: 0,
                    terminus: 'term',
                },
            ],
        } as EntryDetail;

        entryServiceMock.delete.mockReturnValue(of(true));
        entryServiceMock.unlock.mockReturnValue(of(true));
        entryServiceMock.restore.mockReturnValue(of(true));
        dialogMock.open.mockReturnValue(dialogRefMock);
        isAdminSubject.next(false);
        isSuperUserSubject.next(false);
        isWriterSubject.next(false);
        userAccountDetailSubject.next(userAccountDetail);
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(EntryButtonListComponent);
        component = fixture.componentInstance;
        component.currentEntry = entryDetail;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set currentEntry', async () => {
        await fixture.whenStable();

        expect(component.currentEntry).toEqual(entryDetail);
    });

    it('should set canCreate to false for writer without collections', () => {
        isWriterSubject.next(true);

        userAccountDetailSubject.next({
            email: 'email',
            firstName: 'paul',
            id: 1,
            lastName: 'paulson',
            glossaries: [],
            hasFullAccess: false,
            isWriter: true,
            collections: [],
            hasRole: false,
        });

        expect(component.canCreate).toEqual(false);
    });

    it('should set canCreate to false for non-writer with collections', () => {
        isWriterSubject.next(false);

        userAccountDetailSubject.next({
            email: 'email',
            firstName: 'paul',
            id: 1,
            lastName: 'paulson',
            glossaries: [],
            hasFullAccess: false,
            isWriter: false,
            collections: [],
            hasRole: false,
        });

        expect(component.canCreate).toEqual(false);
    });

    it('should set canCreate to true for writer with collections', () => {
        isWriterSubject.next(true);

        userAccountDetailSubject.next({
            email: 'email',
            firstName: 'paul',
            id: 1,
            lastName: 'paulson',
            glossaries: [],
            hasFullAccess: false,
            isWriter: true,
            collections: [{code: 'code1', deleted: false, id: 1, isObsolete: false, name: 'col1', active: false, activeApi: false, exportLindas: false}],
            hasRole: false,
        });

        expect(component.canCreate).toEqual(true);
    });

    it('should set canCreate to true for admin', () => {
        isAdminSubject.next(true);
        expect(component.canCreate).toEqual(true);
    });

    it('should set canCreate to true for superUser', () => {
        isSuperUserSubject.next(true);

        expect(component.canCreate).toEqual(true);
    });

    describe('soft-delete-entry', () => {
        it('should open dialog on delete', () => {
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.softDeleteEntry(entryDetail!.id);

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'entry.delete.title',
                    text: 'entry.delete.text',
                    textValue: {id: entryDetail!.id},
                    confirm: 'entry.delete.confirm',
                },
            });
        });

        it('should delete when confirmed', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.softDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.delete).toHaveBeenCalledWith(1);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'search.result.deleted.successTitle',
                message: 'search.result.deleted.successMessage',
            });
        });

        it('should not delete when rejected', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(false));

            component.softDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).not.toHaveBeenCalled();
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
        });

        it('should update current entry after delete when user is admin', async () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            isAdminSubject.next(true);

            component.softDeleteEntry(1);

            await fixture.whenStable();

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(component.currentEntry!.deleted).toEqual(true);
        });

        it('should show warning when update-infos are returned', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.delete.mockReturnValue(of([{lockedEntryId: 42}]));

            component.softDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
        });

        it('should handle error on error', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.delete.mockReturnValue(throwError(() => ({})));

            component.softDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
            expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        });
    });

    it('should restore deleted entry', () => {
        entryServiceMock.restore.mockReturnValue(
            of({
                deleted: false,
                modified: new Date(),
            })
        );
        component.currentEntry!.deleted = true;

        component.restoreEntry(entryDetail!.id);

        expect(entryServiceMock.restore).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'entry.restore.successTitle',
            message: 'entry.restore.successMessage',
        });
        expect(component.currentEntry!.deleted).toBeFalsy();
        expect(component.currentEntry!.modified).not.toBeUndefined();
    });

    describe('hard-delete-entry', () => {
        it('should open dialog on final delete', () => {
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.hardDeleteEntry(entryDetail!.id);

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'entry.hardDelete.title',
                    text: 'entry.hardDelete.text',
                    textValue: {id: entryDetail!.id},
                    confirm: 'entry.hardDelete.confirm',
                },
            });
        });

        it('should final delete when confirmed', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.hardDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.delete).toHaveBeenCalledWith(1, false);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'search.result.deleted.successTitle',
                message: 'entry.hardDelete.successMessage',
            });
        });

        it('should not delete when rejected', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(false));

            component.hardDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).not.toHaveBeenCalled();
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
        });

        it('should show warning when locked-entry-update-infos are returned', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.delete.mockReturnValue(of([{lockedEntryId: 7}]));

            component.hardDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
        });

        it('should show warning when no-soft-delete-before-hard-delete-update-infos are returned', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.delete.mockReturnValue(of([{notSoftDeletedEntryId: 11}]));

            component.hardDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
        });

        it('should handle error on error', () => {
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.delete.mockReturnValue(throwError(() => ({})));

            component.hardDeleteEntry(entryDetail!.id);

            expect(entryServiceMock.delete).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).not.toHaveBeenCalled();
            expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        });
    });

    it('should unlock entry', () => {
        entryDetail!.locked = new Date();
        component.unlock(1);

        expect(entryServiceMock.unlock).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.unlock).toHaveBeenCalledWith(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'entry.unlock.successTitle',
            message: 'entry.unlock.successMessage',
        });

        expect(entryDetail!.locked).toBeUndefined();
    });
});
