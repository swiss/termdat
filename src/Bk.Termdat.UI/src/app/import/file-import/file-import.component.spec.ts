import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatChip} from '@angular/material/chips';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {Router} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {FileImportResult} from 'api/FileImportResult';
import {GlossarySummary} from 'api/GlossarySummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {Subject, of} from 'rxjs';
import {CollectionService} from '../../administration/collection/collection.service';
import {GlossaryService} from '../../administration/glossary/glossary.service';
import {EntryService} from '../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';
import {FileSizePipe} from '../../shared/file-size.pipe';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {FileImportErrorComponent} from './file-import-error/file-import-error.component';
import {FileImportUploadComponent} from './file-import-upload/file-import-upload.component';
import {FileImportComponent} from './file-import.component';
import {FileImportService} from './file-import.service';

describe('FileImportComponent', () => {
    let component: FileImportComponent;
    let fixture: ComponentFixture<FileImportComponent>;

    let collections: CollectionSummary[] = [];
    let offices: OfficeSummary[] = [];
    let states: Option[] = [];
    let reliabilities: Option[] = [];
    let adminInfos: AdminInfoSummary[] = [];
    let glossaries: GlossarySummary[] = [];
    let formBuilder: FormBuilder;

    const fileImportResult: FileImportResult = {adminInfoCode: 'Test_Info'};

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
        instant: jest.fn(),
    };

    const errorServiceMock = {
        getControlError: jest.fn(),
        handleHttpError: jest.fn(),
    };

    const entryServiceMock = {
        getReliabilities: jest.fn(),
    };

    const collectionServiceMock = {
        getModificationCollections: jest.fn(),
    };

    const glossaryServiceMock = {
        getModificationGlossaries: jest.fn(),
    };

    const extendedSearchFiltersServiceMock = {
        getOffices: jest.fn(),
        getStates: jest.fn(),
        getAdminInfos: jest.fn(),
    };

    const fileImportServiceMock = {
        importFile: jest.fn(),
    };

    const notificationServiceMock = {
        success: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    const httpInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatOptionModule),
                MockModule(MatButtonModule),
                MockModule(MatInputModule),
                FileImportComponent,
                MockComponent(SubjectFormComponent),
                MockComponent(FileImportErrorComponent),
                MockComponent(FileImportUploadComponent),
                MockComponent(AutocompleteComponent),
                MockComponent(ChiplistAutocompleteComponent),
                MockComponent(ChiplistAutocompleteInputComponent),
                MockComponent(MatChip),
                MockPipe(FileSizePipe),
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: FileImportService, useValue: fileImportServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
            ],
        }).compileComponents();

        collections = [{id: 1, name: 'Collection1', code: 'COL1'} as CollectionSummary, {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary];
        offices = [
            {id: 1, name: 'Office1', code: 'OFF1'},
            {id: 2, name: 'Office2', code: 'OFF2'},
        ];
        states = [{id: 1, name: 'State1', code: 'ST1'} as Option, {id: 2, name: 'State2', code: 'ST2'} as Option];
        reliabilities = [{id: 1, name: 'Reliability1', code: 'R1'} as Option, {id: 2, name: 'Reliability2', code: 'R2'} as Option];
        adminInfos = [{id: 1, code: 'info1'} as AdminInfoSummary, {id: 2, code: 'info2'} as AdminInfoSummary];
        glossaries = [
            {id: 1, name: 'Glossary1', code: 'GLOS1'},
            {id: 2, name: 'Glossary2', code: 'GLOS2'},
        ];

        extendedSearchFiltersServiceMock.getOffices.mockReturnValue(of(offices));
        extendedSearchFiltersServiceMock.getStates.mockReturnValue(of(states));
        extendedSearchFiltersServiceMock.getAdminInfos.mockReturnValue(of(adminInfos));
        collectionServiceMock.getModificationCollections.mockReturnValue(of(collections));
        glossaryServiceMock.getModificationGlossaries.mockReturnValue(of(glossaries));
        entryServiceMock.getReliabilities.mockReturnValue(of(reliabilities));
        fileImportServiceMock.importFile.mockReturnValue(of(fileImportResult));

        formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(FileImportComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should navigate back on cancel', () => {
        component.cancel();
        expect(routerMock.navigate).toHaveBeenCalled();
    });

    it('should not call service when no file is selected', () => {
        component.form.patchValue({
            office: 1,
            collection: 2,
            reliability: 3,
            status: 4,
            comment: '5',
            glossaries: [],
            adminInfos: [],
        });

        component.form.setControl(
            'subjectDomains',
            formBuilder.array([
                {subject: 1, domains: [2, 3]},
                {subject: 5, domains: [6, 7]},
            ])
        );

        component.noFileSelected = true;

        expect(component.form.valid).toBe(true);

        component.import();

        expect(fileImportServiceMock.importFile).toHaveBeenCalledTimes(0);
    });

    it('should not call service when form is invalid', () => {
        component.form.patchValue({
            collection: 2,
            reliability: 3,
            status: 4,
            comment: '5',
            glossaries: [],
            adminInfos: [],
        });

        component.noFileSelected = false;

        expect(component.form.valid).toBe(false);

        component.import();

        expect(fileImportServiceMock.importFile).toHaveBeenCalledTimes(0);
    });

    it('should call service when a file is selected', () => {
        component.form.patchValue({
            office: 1,
            collection: 2,
            reliability: 3,
            status: 4,
            comment: '5',
            glossaries: [],
            adminInfos: [],
        });

        component.form.setControl(
            'subjectDomains',
            formBuilder.array([
                {subject: 1, domains: [2, 3]},
                {subject: 5, domains: [6, 7]},
            ])
        );

        expect(component.form.getRawValue()).toEqual({
            office: 1,
            collection: 2,
            reliability: 3,
            status: 4,
            comment: '5',
            glossaries: [],
            adminInfos: [],
            subjectDomains: [
                {domains: [2, 3], subject: 1},
                {domains: [6, 7], subject: 5},
            ],
        });

        component.noFileSelected = false;

        expect(component.form.valid).toBe(true);

        component.import();

        expect(fileImportServiceMock.importFile).toHaveBeenCalledTimes(1);
    });
});
