import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, convertToParamMap, Router, RouterModule} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObAlertComponent, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {EntryTemplateUpdateComponent} from './entry-template-update.component';

describe('EntryTemplateUpdateComponent', () => {
    let component: EntryTemplateUpdateComponent;
    let fixture: ComponentFixture<EntryTemplateUpdateComponent>;

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
        snapshot: {},
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
    };
    const formComponent = {
        getFormEntry: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
        getCurrentNavigation: jest.fn(),
        currentNavigation: jest.fn(() => ({extras: {queryParams: {}}})),
    };
    const entryTemplateServiceMock = {
        getTemplateById: jest.fn(),
        update: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatButtonModule),
                ReactiveFormsModule,
                RouterModule,
                EntryTemplateUpdateComponent,
                MockComponent(EntryFormComponent),
                MockComponent(ObAlertComponent),
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: UnsavedChangesService, useValue: unsavedChangesServiceMock},
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: Router, useValue: routerMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: EntryTemplateService, useValue: entryTemplateServiceMock},
            ],
        }).compileComponents();

        entry = {
            id: 42,
            status: Status.Validated,
            collection: {
                id: 10,
                code: 'VAC1',
                name: 'Value added collection',
            } as CollectionSummary,
            office: {id: 1, name: 'Office1', code: 'OFF1'},
            lenochClassifications: [{id: 1, name: 'Domain1', code: 'DO1'}],
            relatedCollections: [{id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary],
            glossaries: [{id: 2, name: 'Collection2', code: 'COL2'}],
            conceptSystemPosition: {id: 2, value: 'ConceptSystemPosition2'},
            reliability: Reliability.Tentative,
            references: [],
            languageDetails: [
                {
                    id: 100,
                    abbreviation: 'short',
                    definition: 'def',
                    languageId: 1,
                    name: 'Search Detail',
                    phraseology: 'phraseology',
                    sequence: 0,
                    terminus: 'term',
                },
            ],
        };

        entryTemplateServiceMock.update.mockReturnValue(of(undefined));
        entryTemplateServiceMock.getTemplateById.mockReturnValue(of(entry));
        formComponent.getFormEntry.mockReturnValue(entry);

        fixture = TestBed.createComponent(EntryTemplateUpdateComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();
        component.entryFormComponent = formComponent as unknown as EntryFormComponent;
        paramMapSubject.next(convertToParamMap({id: '42'}));
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should watch for unsaved changes', () => {
        expect(unsavedChangesServiceMock.watch).toHaveBeenCalledWith('entry-template-update', component, component.form);
    });

    it('should stop watch for unsaved changes on destroy', () => {
        component.ngOnDestroy();

        expect(unsavedChangesServiceMock.unwatch).toHaveBeenCalledWith('entry-template-update');
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

    it('should create component', () => {
        expect(component).toBeTruthy();
    });

    it('can update a template', async () => {
        await fixture.whenStable();

        component.save();

        expect(entryTemplateServiceMock.update).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['/user'], {queryParams: {}});
    });

    it('can update a template when form is invalid', async () => {
        await fixture.whenStable();

        component.form.addControl('test', new FormControl('', Validators.required));

        component.save();

        expect(entryTemplateServiceMock.update).toHaveBeenCalledTimes(1);
    });

    it('loads template on init and when id changes', () => {
        const expectedEntry = {...entry, id: 42, concurrencyToken: undefined, lenochClassifications: []};
        expect(component.entry).toEqual(expectedEntry);
        expect(entryTemplateServiceMock.getTemplateById).toHaveBeenCalledTimes(1);
        expect(entryTemplateServiceMock.getTemplateById).toHaveBeenCalledWith(42);

        paramMapSubject.next(convertToParamMap({id: 666}));
        expect(entryTemplateServiceMock.getTemplateById).toHaveBeenCalledTimes(2);
        expect(entryTemplateServiceMock.getTemplateById).toHaveBeenCalledWith(666);
    });

    it('disables save button when form is pristine', async () => {
        await fixture.whenStable();

        expect(component.form.dirty).toBeFalsy();
        let saveButton = fixture.debugElement.query(By.css('.buttons-bar > button'));
        expect(saveButton).toBeTruthy();
        expect(component.form.pristine).toEqual(true);

        component.form.markAsDirty();
        fixture.detectChanges();
        await fixture.whenStable();

        saveButton = fixture.debugElement.query(By.css('.buttons-bar > button'));
        expect(component.form.pristine).toEqual(false);
        expect(saveButton).toBeTruthy();
    });
});
