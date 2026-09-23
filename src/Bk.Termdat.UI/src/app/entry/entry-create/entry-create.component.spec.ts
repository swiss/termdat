import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {provideHttpClientTesting} from '@angular/common/http/testing';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {EntryDetail} from 'api/EntryDetail';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {of} from 'rxjs';
import {ConfigsService} from '../../config.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {EntryTemplateCreateDialogComponent} from '../entry-template-create-dialog/entry-template-create-dialog.component';
import {EntryService} from '../entry.service';
import {EntryCreateComponent} from './entry-create.component';

describe('CreateComponent', () => {
    let component: EntryCreateComponent;
    let fixture: ComponentFixture<EntryCreateComponent>;

    const entryServiceMock = {
        create: jest.fn(),
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
        deactivateSpinnerOnNextAPICalls: jest.fn(),
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
        getCurrentNavigation: jest.fn(),
        currentNavigation: jest.fn(() => ({extras: {queryParams: {}}})),
    };
    let entry: EntryDetail;
    const formComponent = {
        getFormEntry: jest.fn(),
        getValidationNotifications: jest.fn(),
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

    const configServiceMock = {
        frontendConfig: {
            defaultCollectionId: 123,
            defaultOfficeId: 1,
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatButtonModule),
                ReactiveFormsModule,
                EntryCreateComponent,
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
                {provide: Router, useValue: routerMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFilterServiceMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                {provide: ConfigsService, useValue: configServiceMock},
            ],
        }).compileComponents();

        entry = {id: 1} as EntryDetail;
        formComponent.getFormEntry.mockReturnValue(entry);
        entryServiceMock.create.mockReturnValue(of(entry));
        formComponent.getValidationNotifications.mockReturnValue(['test']);
        dialogMock.open.mockReturnValue(dialogRefMock);

        fixture = TestBed.createComponent(EntryCreateComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();
        component.entryFormComponent = formComponent as unknown as EntryFormComponent;
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set correct defaultEntry', () => {
        expect(component.entryDefault!.collection.id).toEqual(123);
        expect(component.entryDefault!.office.id).toEqual(1);
        expect(component.entryDefault!.reliability).toEqual(Reliability.Tentative);
        expect(component.entryDefault!.status).toEqual(Status.InProgress);
    });

    it('returns true on canUnload when no change has been made', () => {
        expect(component.canUnload()).toEqual(true);
    });

    it('returns false on canUnload when changes have been made', () => {
        component.form.markAsDirty();

        expect(component.canUnload()).toEqual(false);
    });

    it('returns true on canDeactivate when no change has been made', done => {
        component.canDeactivate().subscribe(canDeactivate => {
            expect(canDeactivate).toEqual(true);
            done();
        });
    });

    it('returns createDeactivateConfirmDialog result on canDeactivate when changes have been made', done => {
        component.form.markAsDirty();
        const expectedReturn = of(true);
        unsavedChangesServiceMock.createDeactivateConfirmDialog.mockReturnValue(expectedReturn);

        component.canDeactivate().subscribe(result => {
            expect(result).toEqual(true);
            expect(unsavedChangesServiceMock.createDeactivateConfirmDialog).toHaveBeenCalledTimes(1);
            done();
        });
    });

    it('should watch for unsaved changes', () => {
        expect(unsavedChangesServiceMock.watch).toHaveBeenCalledWith('entry-create', component, component.form);
    });

    it('should stop watch for unsaved changes on destroy', () => {
        component.ngOnDestroy();

        expect(unsavedChangesServiceMock.unwatch).toHaveBeenCalledWith('entry-create');
    });

    it('can save a copied entry', async () => {
        await fixture.whenStable();

        component.save();

        expect(entryServiceMock.create).toHaveBeenCalledTimes(1);
    });

    it('cannot save when invalid', async () => {
        await fixture.whenStable();

        component.form.addControl('test', new FormControl('', Validators.required));

        component.save();

        expect(entryServiceMock.create).not.toHaveBeenCalled();
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({message: 'test'});
    });

    it('should open dialog and call service when creating template', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.createEntryTemplate();

        expect(dialogMock.open).toHaveBeenCalled();

        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(entryTemplateServiceMock.createEntryTemplate).toHaveBeenCalledTimes(1);
    });

    it('should open template-selection dialog and navigation should be triggered', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(42));

        component.applyEntryTemplate();

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('disables save button when form is pristine', async () => {
        await fixture.whenStable();

        expect(component.form.dirty).toBeFalsy();
        let buttons = fixture.debugElement.queryAll(By.css('.buttons-bar > button'));

        expect(buttons).toBeTruthy();
        expect(buttons.length).toEqual(4);

        let saveButton = buttons[2];

        expect(component.form.pristine).toEqual(true);

        component.form.markAsDirty();
        fixture.detectChanges();
        await fixture.whenStable();

        buttons = fixture.debugElement.queryAll(By.css('.buttons-bar > button'));

        saveButton = buttons[2];
        expect(component.form.pristine).toEqual(false);
        expect(saveButton).toBeTruthy();
    });
});
