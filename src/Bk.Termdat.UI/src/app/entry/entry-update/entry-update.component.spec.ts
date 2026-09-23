import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {provideHttpClientTesting} from '@angular/common/http/testing';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, convertToParamMap, Router, RouterModule} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {LockResult} from 'api/LockResult';
import {Status} from 'api/Status';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {EntryLockedService} from '../entry-locked/entry-locked.service';
import {EntryTemplateCreateDialogComponent} from '../entry-template-create-dialog/entry-template-create-dialog.component';
import {EntryService} from '../entry.service';
import {EntryUpdateComponent} from './entry-update.component';

describe('EntryUpdateComponent', () => {
    let component: EntryUpdateComponent;
    let fixture: ComponentFixture<EntryUpdateComponent>;

    const entryServiceMock = {
        update: jest.fn(),
        getModification: jest.fn(),
        lock: jest.fn(),
        unlock: jest.fn(),
    };
    const entryTemplateServiceMock = {
        createEntryTemplate: jest.fn(),
    };
    const notificationServiceMock = {
        error: jest.fn(),
        success: jest.fn(),
        warning: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const unsavedChangesServiceMock = {
        watch: jest.fn(),
        unwatch: jest.fn(),
        createDeactivateConfirmDialog: jest.fn(),
    };
    const httpInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    let entry: EntryModificationDetail;
    const paramMapSubject = new Subject();
    const activatedRouteMock = {
        paramMap: paramMapSubject.asObservable(),
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
    };
    const formComponent = {
        getFormEntry: jest.fn(),
        getValidationNotifications: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    let locKResult: LockResult;
    const entryLockedServiceMock = {
        watch: jest.fn(),
        unwatch: jest.fn(),
    };
    const extendedSearchFilterServiceMock = {
        reloadExtendedSearchData$: {
            next: jest.fn(),
        },
    };
    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatButtonModule),
                ReactiveFormsModule,
                RouterModule,
                EntryUpdateComponent,
                MockComponent(EntryFormComponent),
                MockComponent(EntryTemplateCreateDialogComponent),
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: EntryService, useValue: entryServiceMock},
                {provide: EntryTemplateService, useValue: entryTemplateServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: UnsavedChangesService, useValue: unsavedChangesServiceMock},
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: EntryLockedService, useValue: entryLockedServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFilterServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
            ],
        }).compileComponents();

        entry = {
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
            concurrencyToken: 999,
        } as EntryModificationDetail;

        locKResult = {
            concurrencyToken: 777,
            isLocked: true,
        };

        entryServiceMock.update.mockReturnValue(of(entry));
        formComponent.getFormEntry.mockReturnValue({...entry, locked: undefined});
        formComponent.getValidationNotifications.mockReturnValue(['test']);
        entryServiceMock.getModification.mockReturnValue(of(entry));
        entryServiceMock.lock.mockReturnValue(of(locKResult));
        entryServiceMock.unlock.mockReturnValue(of(locKResult));
        dialogMock.open.mockReturnValue(dialogRefMock);

        fixture = TestBed.createComponent(EntryUpdateComponent);

        component = fixture.componentInstance;
        fixture.detectChanges();
        component.entryFormComponent = formComponent as unknown as EntryFormComponent;
        paramMapSubject.next(convertToParamMap({id: '1'}));
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should watch for unsaved changes', () => {
        expect(unsavedChangesServiceMock.watch).toHaveBeenCalledWith('entry-update', component, component.form);
    });

    it('should stop watch for unsaved changes on destroy', () => {
        component.ngOnDestroy();

        expect(unsavedChangesServiceMock.unwatch).toHaveBeenCalledWith('entry-update');
    });

    it('should watch for locked entry', () => {
        expect(entryLockedServiceMock.watch).toHaveBeenCalledWith('entry-update', component);
    });

    it('should stop watch for locked entry on destroy', () => {
        component.ngOnDestroy();

        expect(entryLockedServiceMock.unwatch).toHaveBeenCalledWith('entry-update');
    });

    it('returns true on canUnload when no change has been made', () => {
        expect(component.canUnload()).toEqual(true);
    });

    it('returns false on canUnload when changes have been made', () => {
        component.form.markAsDirty();

        expect(component.canUnload()).toEqual(false);
    });

    it('returns true on canDeactivate when no change has been made', done => {
        component.canDeactivate().subscribe(result => {
            expect(result).toEqual(true);
            expect(unsavedChangesServiceMock.createDeactivateConfirmDialog).not.toHaveBeenCalled();
            done();
        });
    });

    it('returns true on canDeactivate when changes have been made', done => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        jest.spyOn<any, any>(component, 'saveInternal');
        component.form.markAsDirty();
        unsavedChangesServiceMock.createDeactivateConfirmDialog.mockImplementation(onSave => {
            onSave();
            return of(true);
        });

        component.canDeactivate().subscribe(result => {
            expect(result).toEqual(true);
            expect(unsavedChangesServiceMock.createDeactivateConfirmDialog).toHaveBeenCalledTimes(1);
            // eslint-disable-next-line dot-notation
            expect(component['saveInternal']).toHaveBeenCalled();
            done();
        });
    });

    it('returns false on canDeactivate when changes have been made', done => {
        component.form.markAsDirty();
        unsavedChangesServiceMock.createDeactivateConfirmDialog.mockImplementation(() => of(false));

        component.canDeactivate().subscribe(result => {
            expect(result).toEqual(false);
            expect(unsavedChangesServiceMock.createDeactivateConfirmDialog).toHaveBeenCalledTimes(1);
            done();
        });
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('can update an entry', async () => {
        await fixture.whenStable();

        component.save();

        expect(entryServiceMock.update).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.update).toHaveBeenCalledWith({
            collection: {code: 'VAC1', id: 10, name: 'Value added collection'},
            id: 1,
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
            status: 1,
            concurrencyToken: 999,
        });
    });

    it('cannot save when invalid', async () => {
        await fixture.whenStable();

        component.form.addControl('test', new FormControl('', Validators.required));

        component.save();

        expect(entryServiceMock.update).not.toHaveBeenCalled();
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({message: 'test'});
    });

    it('loads entry on init and when id changes', () => {
        expect(component.entry).toEqual(entry);
        expect(entryServiceMock.getModification).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getModification).toHaveBeenCalledWith(1);

        paramMapSubject.next(convertToParamMap({id: 5}));
        expect(entryServiceMock.getModification).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.getModification).toHaveBeenCalledWith(5);

        paramMapSubject.next(convertToParamMap({id: -5}));
        expect(entryServiceMock.getModification).toHaveBeenCalledTimes(2);

        paramMapSubject.next(convertToParamMap({id: 'abc'}));
        expect(entryServiceMock.getModification).toHaveBeenCalledTimes(2);
    });

    it('handles http errors from loading entry', () => {
        const httpErrorResponse = {};
        entryServiceMock.getModification.mockReturnValue(throwError(() => httpErrorResponse));

        paramMapSubject.next(convertToParamMap({id: 5}));

        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledWith(httpErrorResponse, 'entry.notification.editError');
    });

    it('disables save button when form is pristine', async () => {
        await fixture.whenStable();

        expect(component.form.dirty).toBeFalsy();
        let buttons = fixture.debugElement.queryAll(By.css('.buttons-bar > button'));

        expect(buttons).toBeTruthy();
        expect(buttons.length).toEqual(2);

        let saveButton = buttons[1];

        expect(component.form.pristine).toEqual(true);

        component.form.markAsDirty();
        fixture.detectChanges();
        await fixture.whenStable();

        buttons = fixture.debugElement.queryAll(By.css('.buttons-bar > button'));

        saveButton = buttons[1];
        expect(component.form.pristine).toEqual(false);
        expect(saveButton).toBeTruthy();
    });

    it('should show an error when error is thrown', async () => {
        await fixture.whenStable();
        const httpErrorResponse = {};
        entryServiceMock.update.mockReturnValue(throwError(() => httpErrorResponse));

        component.save();

        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledWith(httpErrorResponse, 'entry.notification.editError');
    });

    it('should lock entry when entry is loaded', () => {
        expect(entryServiceMock.lock).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.lock).toHaveBeenCalledWith(entry.id);
        expect(entry.concurrencyToken).toEqual(locKResult.concurrencyToken);
        expect(entry.locked).toBeInstanceOf(Date);
    });

    it('should navigate away on close', () => {
        component.close();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', entry.id], {queryParams: undefined});
    });

    it('should get canLockedUnload', () => {
        expect(component.canLockedUnload()).toEqual(false);

        component.entry = undefined;

        expect(component.canLockedUnload()).toEqual(true);
    });

    it('should return false for canLockedDeactivate when unlock throws an error', () => {
        entryServiceMock.unlock.mockReturnValue(throwError(() => new Error('MUST BE CAUGHT')));
        entry.locked = new Date();

        let result: boolean;
        component.canLockedDeactivate().subscribe(value => (result = value));

        expect(result!).toEqual(false);
        expect(entryServiceMock.unlock).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.unlock).toHaveBeenCalledWith(entry!.id);
    });

    it('should unlock when entry is locked on canLockedDeactivate', () => {
        entry.locked = new Date();

        let result: boolean;
        component.canLockedDeactivate().subscribe(value => (result = value));

        expect(result!).toEqual(true);
        expect(entryServiceMock.unlock).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.unlock).toHaveBeenCalledWith(entry!.id);
    });

    it('should not unlock when entry is not locked on canLockedDeactivate', () => {
        component.entry = undefined;
        component.canLockedDeactivate().subscribe();

        expect(entryServiceMock.unlock).toHaveBeenCalledTimes(0);
    });

    it('should open dialog and call service when creating template', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.createEntryTemplate();

        expect(dialogMock.open).toHaveBeenCalled();

        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(entryTemplateServiceMock.createEntryTemplate).toHaveBeenCalledTimes(1);
    });
});
