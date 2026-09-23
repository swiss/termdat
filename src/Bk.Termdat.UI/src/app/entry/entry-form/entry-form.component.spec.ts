import {TextFieldModule} from '@angular/cdk/text-field';
import {EventEmitter} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors} from '@angular/forms';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatBadgeModule} from '@angular/material/badge';
import {MatButtonModule} from '@angular/material/button';
import {MatChipGrid, MatChipInput, MatChipListbox, MatChipRow} from '@angular/material/chips';
import {MatDialog} from '@angular/material/dialog';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {ConceptSystemPositionSummary} from 'api/ConceptSystemPositionSummary';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {GlossarySummary} from 'api/GlossarySummary';
import {LanguageSummary} from 'api/LanguageSummary';
import {LenochClassificationSummary} from 'api/LenochClassificationSummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {PictureDetail} from 'api/PictureDetail';
import {ReferenceType} from 'api/ReferenceType';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {MockComponent, MockDirective, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {CollectionService} from '../../administration/collection/collection.service';
import {GlossaryService} from '../../administration/glossary/glossary.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';
import {WINDOW} from '../../shared/injection.tokens';
import {LanguageService} from '../../shared/language.service';
import {OpenOnFocusDirective} from '../../shared/open-on-focus.directive';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {ReferenceListComponent} from '../entry-detail-form/reference-list/reference-list.component';
import {EntryService} from '../entry.service';
import {AddLanguageDialogComponent} from './add-language-dialog/add-language-dialog.component';
import {EntryFormPictureComponent} from './entry-form-picture/entry-form-picture.component';
import {EntryFormSynonymComponent} from './entry-form-synonym/entry-form-synonym.component';
import {SynonymMoveDirection} from './entry-form-synonym/SynonymMoveDirection';
import {EntryFormComponent} from './entry-form.component';
import {EntryFormService} from './entry-form.service';
import {ExternalLinkEntryComponent} from './external-links-form/external-link-entry/external-link-entry.component';
import {ExternalLinksFormComponent} from './external-links-form/external-links-form.component';
import {ReferencesFormComponent} from './references-form/references-form.component';

describe('EntryFormComponent', () => {
    let component: EntryFormComponent;
    let fixture: ComponentFixture<EntryFormComponent>;

    let languages: LanguageSummary[] = [];
    let collections: CollectionSummary[] = [];
    let offices: OfficeSummary[] = [];
    let states: Option[] = [];
    let reliabilities: Option[] = [];
    let adminInfos: AdminInfoSummary[] = [];
    let conceptSystemPositions: ConceptSystemPositionSummary[] = [];
    let glossaries: GlossarySummary[] = [];

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: langChangeSubject.asObservable(),
    };
    const collectionServiceMock = {
        getActiveCollections: jest.fn(),
        getModificationCollections: jest.fn(),
    };
    const extendedSearchServiceMock = {
        getOffices: jest.fn(),
        getStates: jest.fn(),
        getAdminInfos: jest.fn(),
    };
    const languageServiceMock = {
        getCurrentLanguageSummaries: jest.fn(),
    };
    const glossaryServiceMock = {
        getModificationGlossaries: jest.fn(),
    };
    const entryServiceMock = {
        create: jest.fn(),
        getReliabilities: jest.fn(),
        getConceptSystemPositions: jest.fn(),
    };
    const afterClosedMock = new EventEmitter<LanguageSummary[]>();
    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };
    const httpInterceptorEventsMock = {
        deactivateSpinnerOnNextAPICalls: jest.fn(),
    };
    let formEntry: EntryModificationDetail;
    let formEntryWithDefaultCollectionId: EntryModificationDetail;
    let formBuilder: FormBuilder;
    let form: FormGroup;
    const windowMock = {
        setTimeout: jest.fn(),
    };
    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatButtonModule),
                MockModule(MatInputModule),
                MockModule(MatAutocompleteModule),
                MockModule(TranslateModule),
                MockModule(MatBadgeModule),
                MockModule(MatIconModule),
                MockModule(MatSelectModule),
                MockModule(MatExpansionModule),
                MockModule(TextFieldModule),
                MockModule(MatTooltipModule),
                EntryFormComponent,
                MockComponent(AutocompleteComponent),
                MockComponent(ChiplistAutocompleteComponent),
                MockComponent(ChiplistAutocompleteInputComponent),
                MockComponent(EntryFormPictureComponent),
                MockComponent(EntryFormSynonymComponent),
                MockComponent(ReferenceListComponent),
                MockComponent(ReferencesFormComponent),
                MockComponent(ExternalLinkEntryComponent),
                MockComponent(ExternalLinksFormComponent),
                MockComponent(SubjectFormComponent),
                MockComponent(MatChipListbox),
                MockComponent(MatChipGrid),
                MockComponent(MatChipRow),
                MockDirective(MatChipInput),
                MockDirective(OpenOnFocusDirective),
            ],
            providers: [
                EntryFormService,
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: WINDOW, useValue: windowMock},
                {provide: ErrorService, useValue: errorServiceMock},
            ],
        }).compileComponents();

        collections = [{id: 1, name: 'Collection1', code: 'COL1'} as CollectionSummary, {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary];
        offices = [
            {id: 1, name: 'Office1', code: 'OFF1'},
            {id: 2, name: 'Office2', code: 'OFF2'},
        ];
        glossaries = [
            {id: 1, name: 'Glossary1', code: 'GLO1'},
            {id: 2, name: 'Glossary2', code: 'GLO2'},
        ];
        languages = [{id: 1, name: 'Deutsch', sortIndex: 1, isUiLanguage: true, isoCode: 'de'}, {id: 2, name: 'Englisch', sortIndex: 2} as LanguageSummary];
        states = [{id: 1, name: 'State1', code: 'ST1'} as Option, {id: 2, name: 'State2', code: 'ST2'} as Option];
        reliabilities = [{id: 1, name: 'Reliability1', code: 'R1'} as Option, {id: 2, name: 'Reliability2', code: 'R2'} as Option];
        adminInfos = [{id: 1, code: 'info1'} as AdminInfoSummary, {id: 2, code: 'info2'} as AdminInfoSummary];

        conceptSystemPositions = [
            {id: 1, value: 'ConceptSystem1'},
            {id: 2, value: 'ConceptSystem2'},
        ];
        formEntry = {
            id: 1000,
            status: Status.InProgress,
            reliability: Reliability.FirstDraft,
            office: {id: 1} as OfficeSummary,
            collection: {id: 1, code: 'C1', active: true} as CollectionSummary,
            lenochClassifications: [{id: 1} as LenochClassificationSummary],
            relatedCollections: [{id: 1, active: true} as CollectionSummary],
            glossaries: [{id: 1} as GlossarySummary],
            comment: 'comment',
            identification: 10,
            conceptSystemPosition: {id: 1} as ConceptSystemPositionSummary,
            references: [],
            subjects: [{id: 1, name: 'name1', domains: [{id: 2, name: 'name2', subjectId: 1}]}],
            domains: [],
            canEditCollection: true,
            languageDetails: [
                {
                    id: 30,
                    languageId: 1,
                    sequence: 1,
                    terminus: 'terminus',
                    terminusSource: 'terminusSource',
                    name: 'name',
                    nameSource: 'nameSource',
                    abbreviation: 'abbreviation',
                    abbreviationSource: 'abbreviationSource',
                    phraseology: 'phraseology',
                    phraseologySource: 'phraseologySource',
                    definition: 'definition',
                    definitionSource: 'definitionSource',
                    note: 'note',
                    noteSource: 'noteSource',
                    context: 'context',
                    contextSource: 'contextSource',
                    country: 'country',
                    metadata: 'metadata',
                    metadataSource: 'metadataSource',
                },
                {
                    id: 31,
                    languageId: 1,
                    sequence: 2,
                    terminus: 'synonym',
                },
                {
                    id: 32,
                    languageId: 2,
                    sequence: 1,
                    terminus: 'Anderer Terminus',
                },
            ],
        };

        formEntryWithDefaultCollectionId = {
            id: 1000,
            status: Status.InProgress,
            reliability: Reliability.FirstDraft,
            office: {id: 1} as OfficeSummary,
            collection: {id: 1, active: true} as CollectionSummary,
            lenochClassifications: [{id: 1} as LenochClassificationSummary],
            relatedCollections: [{id: 1, active: true} as CollectionSummary],
            glossaries: [{id: 1} as GlossarySummary],
            comment: 'comment',
            identification: 10,
        };

        extendedSearchServiceMock.getOffices.mockReturnValue(of(offices));
        extendedSearchServiceMock.getStates.mockReturnValue(of(states));
        extendedSearchServiceMock.getAdminInfos.mockReturnValue(of(adminInfos));
        collectionServiceMock.getActiveCollections.mockReturnValue(of(collections));
        collectionServiceMock.getModificationCollections.mockReturnValue(of(collections));
        glossaryServiceMock.getModificationGlossaries.mockReturnValue(of(glossaries));
        entryServiceMock.getReliabilities.mockReturnValue(of(reliabilities));
        entryServiceMock.getConceptSystemPositions.mockReturnValue(of(conceptSystemPositions));
        languageServiceMock.getCurrentLanguageSummaries.mockReturnValue(of(languages));
        dialogMock.open.mockReturnValue(dialogRefMock);
        dialogRefMock.afterClosed.mockReturnValue(afterClosedMock);
        windowMock.setTimeout.mockImplementation(fn => fn());
        translateServiceMock.instant.mockImplementation(value => value);
        errorServiceMock.getControlError.mockImplementation((errors: ValidationErrors | null, label: string, labelPrefix: string) => {
            if (errors === null) {
                return '';
            }

            const errorKeys = Object.keys(errors);
            return `${labelPrefix}.${errorKeys[0]}`;
        });

        formBuilder = TestBed.inject(FormBuilder);
        form = formBuilder.group({});

        fixture = TestBed.createComponent(EntryFormComponent);
        component = fixture.componentInstance;
        component.form = form;

        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load required data on init', () => {
        expect(collectionServiceMock.getActiveCollections).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.getModificationCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(glossaryServiceMock.getModificationGlossaries).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);
        expect(languageServiceMock.getCurrentLanguageSummaries).toHaveBeenCalledTimes(1);

        expect(component.relatedCollectionChips).toHaveLength(2);
        expect(component.offices).toEqual(offices);
        expect(component.collections).toEqual(collections);
        expect(component.glossaryChips).toHaveLength(2);
        expect(component.states).toEqual(states);
        expect(component.reliabilities).toEqual(reliabilities);
        expect(component.languages).toEqual(languages);
        expect(Object.keys(component.languageMap).length).toEqual(component.languages.length);
        expect(Object.keys(component.sourceTemplateLanguageMap).length).toEqual(component.languages.length);
    });

    it('should append LINDAS and API markers to collection and related collection names', () => {
        collectionServiceMock.getModificationCollections.mockReturnValue(
            of([
                {id: 1, name: 'Collection1', code: 'COL1', exportLindas: true} as CollectionSummary,
                {id: 2, name: 'Collection2', code: 'COL2', activeApi: true} as CollectionSummary,
            ])
        );
        collectionServiceMock.getActiveCollections.mockReturnValue(
            of([
                {id: 3, name: 'Collection3', code: 'COL3', exportLindas: true} as CollectionSummary,
                {id: 4, name: 'Collection4', code: 'COL4', activeApi: true} as CollectionSummary,
            ])
        );

        langChangeSubject.next({lang: 'en'});

        expect(component.collections[0].name).toEqual('Collection1 (LINDAS)');
        expect(component.collections[1].name).toEqual('Collection2 (administration.collection.field.activeApi)');
        expect(component.relatedCollectionChips[0].displayText).toEqual('COL3 - Collection3 (LINDAS)');
        expect(component.relatedCollectionChips[1].displayText).toEqual('COL4 - Collection4 (administration.collection.field.activeApi)');
    });

    it('should reload required data on language change', () => {
        langChangeSubject.next({lang: 'en'});

        expect(collectionServiceMock.getActiveCollections).toHaveBeenCalledTimes(2);
        expect(collectionServiceMock.getModificationCollections).toHaveBeenCalledTimes(2);
        expect(extendedSearchServiceMock.getOffices).toHaveBeenCalledTimes(2);
        expect(extendedSearchServiceMock.getStates).toHaveBeenCalledTimes(2);
        expect(glossaryServiceMock.getModificationGlossaries).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(2);
        expect(languageServiceMock.getCurrentLanguageSummaries).toHaveBeenCalledTimes(2);

        expect(component.relatedCollectionChips).toHaveLength(2);
        expect(component.offices).toEqual(offices);
        expect(component.collections).toEqual(collections);
        expect(component.glossaryChips).toHaveLength(2);
        expect(component.states).toEqual(states);
        expect(component.reliabilities).toEqual(reliabilities);
        expect(component.languages).toEqual(languages);
        expect(Object.keys(component.languageMap).length).toEqual(component.languages.length);
        expect(Object.keys(component.sourceTemplateLanguageMap).length).toEqual(component.languages.length);
    });

    it('should catch errors when data loading fails', async () => {
        jest.resetAllMocks();
        collectionServiceMock.getActiveCollections.mockReturnValue(throwError(() => []));
        collectionServiceMock.getModificationCollections.mockReturnValue(throwError(() => []));
        extendedSearchServiceMock.getOffices.mockReturnValue(throwError(() => []));
        extendedSearchServiceMock.getStates.mockReturnValue(throwError(() => []));
        extendedSearchServiceMock.getAdminInfos.mockReturnValue(throwError(() => []));
        glossaryServiceMock.getModificationGlossaries.mockReturnValue(throwError(() => []));
        entryServiceMock.getReliabilities.mockReturnValue(throwError(() => []));
        languageServiceMock.getCurrentLanguageSummaries.mockReturnValue(throwError(() => []));

        fixture = TestBed.createComponent(EntryFormComponent);
        component = fixture.componentInstance;
        component.form = form;
        fixture.detectChanges();

        await fixture.whenStable();

        expect(collectionServiceMock.getActiveCollections).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.getModificationCollections).toHaveBeenCalledTimes(1);
        expect(extendedSearchServiceMock.getOffices).toHaveBeenCalledTimes(1);
        expect(extendedSearchServiceMock.getStates).toHaveBeenCalledTimes(1);
        expect(glossaryServiceMock.getModificationGlossaries).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReliabilities).toHaveBeenCalledTimes(1);
        expect(languageServiceMock.getCurrentLanguageSummaries).toHaveBeenCalledTimes(1);

        expect(component.relatedCollectionChips).toEqual([]);
        expect(component.collections).toEqual([]);
        expect(component.offices).toEqual([]);
        expect(component.glossaryChips).toEqual([]);
        expect(component.states).toEqual([]);
        expect(component.reliabilities).toEqual([]);
        expect(component.languages).toEqual([]);
        expect(Object.keys(component.languageMap)).toHaveLength(0);
        expect(Object.keys(component.sourceTemplateLanguageMap)).toHaveLength(0);
    });

    it('should create languageMap correctly after languageLoad', () => {
        expect(component.languageMap).toEqual({
            '1': {
                id: 1,
                sortIndex: 1,
                name: 'Deutsch',
                isUiLanguage: true,
                isoCode: 'de',
            },
            '2': {
                id: 2,
                sortIndex: 2,
                name: 'Englisch',
            },
        });
    });

    it('should create sourceTemplateLanguageMap correctly after languageLoad', () => {
        expect(component.sourceTemplateLanguageMap).toEqual({
            '1': 'de',
            '2': 'de',
        });
    });

    it('should recreate languageMap on language change', () => {
        languages = [{id: 1, name: 'German', sortIndex: 1, isUiLanguage: true} as LanguageSummary, {id: 2, name: 'English', sortIndex: 2} as LanguageSummary];

        languageServiceMock.getCurrentLanguageSummaries.mockReturnValue(of(languages));
        langChangeSubject.next({lang: 'en'});

        expect(component.languageMap[1].name).toEqual('German');
        expect(component.languageMap[2].name).toEqual('English');
    });

    it('should recreate sourceTemplateLanguageMap on language change', () => {
        languages = [
            {id: 1, name: 'German', sortIndex: 1, isUiLanguage: true} as LanguageSummary,
            {id: 2, name: 'English', sortIndex: 2, isUiLanguage: true, isoCode: 'en'},
            {id: 3, name: 'Spanish', sortIndex: 3, isoCode: 'ES'} as LanguageSummary,
        ];

        languageServiceMock.getCurrentLanguageSummaries.mockReturnValue(of(languages));
        langChangeSubject.next({lang: 'en'});

        expect(component.sourceTemplateLanguageMap).toEqual({
            '1': 'de',
            '2': 'en',
            '3': 'de',
        });
    });

    it('should create languages form after language load correctly', () => {
        const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        expect(languagesForm).toHaveLength(1);

        const form1 = languagesForm[0];

        expect(form1.controls.languageId.value).toEqual(1);
        expect(form1.controls.entries).toBeTruthy();
    });

    it('should not recreate languages form on language change', () => {
        let languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        expect(languagesForm).toHaveLength(1);

        const form1 = languagesForm[0];
        form1.markAsDirty();

        languages = [
            {id: 1, name: 'German', sortIndex: 1, isUiLanguage: true} as LanguageSummary,
            {id: 2, name: 'English', sortIndex: 2, isUiLanguage: true} as LanguageSummary,
        ];

        languageServiceMock.getCurrentLanguageSummaries.mockReturnValue(of(languages));
        langChangeSubject.next({lang: 'en'});

        languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        expect(languagesForm).toHaveLength(2);
        expect(languagesForm[0].dirty).toBeTruthy();
        expect(languagesForm[1].dirty).toBeFalsy();
    });

    it('should disable concept system by default', () => {
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(true);
        expect(entryServiceMock.getConceptSystemPositions).toHaveBeenCalledTimes(0);
    });

    it('should enable concept system if collection is selected and has concept systems', () => {
        component.headForm.controls.collection.setValue(2);

        expect(entryServiceMock.getConceptSystemPositions).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getConceptSystemPositions).toHaveBeenCalledWith(2);
        expect(httpInterceptorEventsMock.deactivateSpinnerOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(false);
    });

    it('should disable concept system if collection is selected and has NO concept systems', () => {
        component.headForm.controls.collection.setValue(2);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(false);

        entryServiceMock.getConceptSystemPositions.mockReturnValue(of([]));
        component.headForm.controls.collection.setValue(1);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(true);

        expect(httpInterceptorEventsMock.deactivateSpinnerOnNextAPICalls).toHaveBeenCalledTimes(2);
    });

    it('should disable concept system if collection is selected and concept systems cannot be requested', () => {
        component.headForm.controls.collection.setValue(2);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(false);

        entryServiceMock.getConceptSystemPositions.mockReturnValue(throwError(() => []));
        component.headForm.controls.collection.setValue(1);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(true);

        expect(httpInterceptorEventsMock.deactivateSpinnerOnNextAPICalls).toHaveBeenCalledTimes(2);
    });

    it('should disable concept system if collection cleared', () => {
        component.headForm.controls.collection.setValue(2);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(false);

        component.headForm.controls.collection.setValue(undefined);
        expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(true);

        expect(httpInterceptorEventsMock.deactivateSpinnerOnNextAPICalls).toHaveBeenCalledTimes(1);
    });

    it('should open dialog to add more languages', () => {
        component.openAddLanguageDialog();

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(AddLanguageDialogComponent, {data: [{id: 2, sortIndex: 2, name: 'Englisch'}]});

        expect(component.form.controls.languages.dirty).toBeFalsy();
        expect(component.form.controls.languages.touched).toBeFalsy();

        afterClosedMock.next([languages[1]]);

        const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        expect(languagesForm.length).toEqual(2);
        expect(component.form.controls.languages.value).toEqual([
            {
                entries: [],
                languageId: 1,
            },
            {
                entries: [
                    {
                        abbreviation: null,
                        abbreviationSource: null,
                        context: null,
                        contextSource: null,
                        country: null,
                        definition: null,
                        definitionSource: null,
                        metadata: null,
                        metadataSource: null,
                        name: null,
                        nameSource: null,
                        note: null,
                        noteSource: null,
                        phraseology: null,
                        phraseologySource: null,
                        terminus: null,
                        terminusSource: null,
                        trackingId: 0,
                    },
                ],
                languageId: 2,
            },
        ]);
        expect(component.form.controls.languages.dirty).toBeTruthy();
        expect(component.form.controls.languages.touched).toBeTruthy();
    });

    it('should add new entry languages', () => {
        const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        const languageEntries = languagesForm[0].controls.entries as FormArray;
        expect(languageEntries.controls).toHaveLength(0);
        expect(languageEntries.dirty).toBeFalsy();
        expect(languageEntries.touched).toBeFalsy();

        component.createEntryLanguage(0);

        expect(languageEntries.controls).toHaveLength(1);

        expect(languageEntries.dirty).toBeTruthy();
        expect(languageEntries.touched).toBeTruthy();
    });

    it('should delete entry languages', () => {
        const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        const languageEntries = languagesForm[0].controls.entries as FormArray;
        component.createEntryLanguage(0);
        expect(languageEntries.controls).toHaveLength(1);
        languageEntries.markAsUntouched();
        languageEntries.markAsPristine();

        component.deleteEntryLanguage(0, 0);
        expect(languageEntries.controls).toHaveLength(0);

        expect(languageEntries.dirty).toBeTruthy();
        expect(languageEntries.touched).toBeTruthy();
    });

    it('completely removes language when last entry language is removed for non ui languages', () => {
        component.openAddLanguageDialog();
        afterClosedMock.next([languages[1]]);

        const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
        expect(languagesForm).toHaveLength(2);
        const language1Entries = languagesForm[0].controls.entries as FormArray;
        const language2Entries = languagesForm[1].controls.entries as FormArray;

        component.createEntryLanguage(0);
        component.createEntryLanguage(1);

        expect(language1Entries.controls).toHaveLength(1);
        expect(language2Entries.controls).toHaveLength(2);

        component.deleteEntryLanguage(0, 0);
        expect(language1Entries.controls).toHaveLength(0);

        component.deleteEntryLanguage(1, 1);
        expect(language2Entries.controls).toHaveLength(1);
        component.deleteEntryLanguage(1, 0);
        expect(languagesForm).toHaveLength(1);
        expect(languagesForm[0].controls.languageId.value).toEqual(1);

        expect(component.form.dirty).toBeTruthy();
        expect(component.form.touched).toBeTruthy();
    });

    describe('create mode', () => {
        it('creates form correctly', () => {
            expect(component.form.value).toEqual({
                head: {
                    adminInfos: [],
                    collection: null,
                    comment: null,
                    lenochClassifications: [],
                    identification: null,
                    office: null,
                    relatedCollections: [],
                    glossaries: [],
                    reliability: null,
                    status: null,
                    references: [],
                    externalLinks: [],
                    subjectDomains: [],
                },
                languages: [
                    {
                        entries: [],
                        languageId: 1,
                    },
                ],
                picture: {},
            });

            expect(component.headForm.valid).toEqual(false);

            expect(component.headForm.controls.collection.valid).toEqual(false);
            expect(component.headForm.controls.office.valid).toEqual(false);
            expect(component.headForm.controls.reliability.valid).toEqual(false);
            expect(component.headForm.controls.status.valid).toEqual(false);
            expect(component.headForm.controls.adminInfos.valid).toEqual(true);
            expect(component.headForm.controls.comment.valid).toEqual(true);
            expect(component.headForm.controls.identification.valid).toEqual(true);
            expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(true);
            expect(component.headForm.controls.conceptSystemPosition.valid).toEqual(false);

            expect(component.form.controls.languages.valid).toEqual(false);
            const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
            expect(languagesForm[0].valid).toEqual(true);
        });

        it('returns formEntry correctly', () => {
            const entry = component.getFormEntry();
            expect(entry).toEqual({
                adminInfos: [],
                collectionId: null,
                comment: null,
                conceptSystemPositionId: null,
                concurrencyToken: undefined,
                domainIds: [],
                lenochClassificationIds: [],
                id: null,
                identification: null,
                languageDetails: [],
                officeId: null,
                relatedCollectionIds: [],
                glossaryIds: [],
                reliability: null,
                status: null,
                references: [],
                externalLinks: [],
                subjectIds: [],
            });
        });

        it('should get validation notifications', () => {
            expect(component.getValidationNotifications()).toEqual(['entry.notification.invalidHeadOrLanguage']);

            component.createEntryLanguage(0);

            expect(component.getValidationNotifications()).toEqual(['entry.notification.invalidHeadOrLanguage']);

            expect(component.getValidationNotifications()).toHaveLength(1);

            expect(translateServiceMock.instant).toHaveBeenLastCalledWith('entry.notification.invalidHeadOrLanguage');
        });

        it('should set canEditCollection to true', () => {
            formEntry.canEditCollection = false;
            formEntry.id = undefined;
            component.formEntry = formEntry;

            fixture.detectChanges();

            expect(component.canEditCollection).toEqual(true);
        });
    });

    describe('update mode', () => {
        beforeEach(async () => {
            component.formEntry = formEntry;

            fixture.detectChanges();

            const formGroup = formBuilder.group({
                subject: formBuilder.control(1),
                domains: formBuilder.control([]),
            });
            component.subjectDomainsForm.push(formGroup);
            await fixture.whenStable();
        });

        it('should create form correctly', () => {
            expect(component.form.value).toEqual({
                head: {
                    adminInfos: [],
                    collection: 1,
                    comment: 'comment',
                    conceptSystemPosition: 1,
                    lenochClassifications: [1],
                    identification: 10,
                    office: 1,
                    relatedCollections: [1],
                    glossaries: [1],
                    reliability: 0,
                    status: 0,
                    references: [],
                    externalLinks: [],
                    subjectDomains: [{domains: [], subject: 1}],
                },
                picture: {},
                languages: [
                    {
                        entries: [
                            {
                                abbreviation: 'abbreviation',
                                abbreviationSource: 'abbreviationSource',
                                context: 'context',
                                contextSource: 'contextSource',
                                country: 'country',
                                definition: 'definition',
                                definitionSource: 'definitionSource',
                                metadata: 'metadata',
                                metadataSource: 'metadataSource',
                                name: 'name',
                                nameSource: 'nameSource',
                                note: 'note',
                                noteSource: 'noteSource',
                                phraseology: 'phraseology',
                                phraseologySource: 'phraseologySource',
                                terminus: 'terminus',
                                terminusSource: 'terminusSource',
                                trackingId: 0,
                            },
                            {
                                abbreviation: null,
                                abbreviationSource: null,
                                context: null,
                                contextSource: null,
                                country: null,
                                definition: null,
                                definitionSource: null,
                                metadata: null,
                                metadataSource: null,
                                name: null,
                                nameSource: null,
                                note: null,
                                noteSource: null,
                                phraseology: null,
                                phraseologySource: null,
                                terminus: 'synonym',
                                terminusSource: null,
                                trackingId: 1,
                            },
                        ],
                        languageId: 1,
                    },
                    {
                        entries: [
                            {
                                abbreviation: null,
                                abbreviationSource: null,
                                context: null,
                                contextSource: null,
                                country: null,
                                definition: null,
                                definitionSource: null,
                                metadata: null,
                                metadataSource: null,
                                name: null,
                                nameSource: null,
                                note: null,
                                noteSource: null,
                                phraseology: null,
                                phraseologySource: null,
                                terminus: 'Anderer Terminus',
                                terminusSource: null,
                                trackingId: 2,
                            },
                        ],
                        languageId: 2,
                    },
                ],
            });

            expect(component.headForm.controls.collection.valid).toEqual(true);
            expect(component.headForm.controls.lenochClassifications.valid).toEqual(true);
            expect(component.headForm.controls.office.valid).toEqual(true);
            expect(component.headForm.controls.reliability.valid).toEqual(true);
            expect(component.headForm.controls.status.valid).toEqual(true);
            expect(component.headForm.controls.adminInfos.valid).toEqual(true);
            expect(component.headForm.controls.comment.valid).toEqual(true);
            expect(component.headForm.controls.identification.valid).toEqual(true);
            expect(component.headForm.controls.conceptSystemPosition.valid).toEqual(true);
            expect(component.headForm.controls.conceptSystemPosition.disabled).toEqual(false);

            expect(component.form.controls.languages.valid).toEqual(true);
            const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
            expect(languagesForm[0].valid).toEqual(true);
            expect(languagesForm[1].valid).toEqual(true);
        });

        it('should return formEntry correctly', () => {
            const entry = component.getFormEntry();
            expect(entry).toEqual({
                adminInfos: [],
                collectionId: 1,
                comment: 'comment',
                conceptSystemPositionId: 1,
                concurrencyToken: undefined,
                domainIds: [],
                lenochClassificationIds: [1],
                glossaryIds: [1],
                id: 1000,
                identification: 10,
                references: [],
                externalLinks: [],
                subjectIds: [1],
                languageDetails: [
                    {
                        id: 30,
                        abbreviation: 'abbreviation',
                        abbreviationSource: 'abbreviationSource',
                        concurrencyToken: undefined,
                        context: 'context',
                        contextSource: 'contextSource',
                        country: 'country',
                        definition: 'definition',
                        definitionSource: 'definitionSource',
                        languageId: 1,
                        metadata: 'metadata',
                        metadataSource: 'metadataSource',
                        name: 'name',
                        nameSource: 'nameSource',
                        note: 'note',
                        noteSource: 'noteSource',
                        phraseology: 'phraseology',
                        phraseologySource: 'phraseologySource',
                        sequence: 1,
                        terminus: 'terminus',
                        terminusSource: 'terminusSource',
                    },
                    {
                        id: 31,
                        abbreviation: null,
                        abbreviationSource: null,
                        concurrencyToken: undefined,
                        context: null,
                        contextSource: null,
                        country: null,
                        definition: null,
                        definitionSource: null,
                        languageId: 1,
                        metadata: null,
                        metadataSource: null,
                        name: null,
                        nameSource: null,
                        note: null,
                        noteSource: null,
                        phraseology: null,
                        phraseologySource: null,
                        sequence: 2,
                        terminus: 'synonym',
                        terminusSource: null,
                    },
                    {
                        id: 32,
                        abbreviation: null,
                        abbreviationSource: null,
                        concurrencyToken: undefined,
                        context: null,
                        contextSource: null,
                        country: null,
                        definition: null,
                        definitionSource: null,
                        languageId: 2,
                        metadata: null,
                        metadataSource: null,
                        name: null,
                        nameSource: null,
                        note: null,
                        noteSource: null,
                        phraseology: null,
                        phraseologySource: null,
                        sequence: 1,
                        terminus: 'Anderer Terminus',
                        terminusSource: null,
                    },
                ],
                officeId: 1,
                relatedCollectionIds: [1],
                reliability: 0,
                status: 0,
                picture: {},
                pictureDeleted: false,
                image: undefined,
            });
        });

        it('should return formEntry removing empty picture attributes', () => {
            const picture = {
                ...formEntry.picture,
                descriptionDe: '',
                descriptionEn: '',
                descriptionFr: '',
                descriptionIt: '',
                descriptionRm: '',
                sourceDe: '',
                sourceEn: '',
                sourceFr: '',
                sourceIt: '',
                sourceRm: '',
            } as PictureDetail;

            component.formEntry = {...formEntry, picture};

            const entry = component.getFormEntry();
            expect(entry.picture).toEqual({
                ...formEntry.picture,
                descriptionDe: undefined,
                descriptionEn: undefined,
                descriptionFr: undefined,
                descriptionIt: undefined,
                descriptionRm: undefined,
                sourceDe: undefined,
                sourceEn: undefined,
                sourceFr: undefined,
                sourceIt: undefined,
                sourceRm: undefined,
            });
        });

        it('should get validation notifications', () => {
            expect(component.getValidationNotifications()).toEqual([]);
        });

        it('should sort synonym up', () => {
            expect(form.dirty).toEqual(false);
            expect(form.touched).toEqual(false);

            component.sortEntryLanguage(0, 0, SynonymMoveDirection.Down);

            expect(form.dirty).toEqual(true);
            expect(form.touched).toEqual(true);

            const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
            const entries = (languagesForm[0].controls.entries as FormArray).controls as FormGroup[];
            expect(entries[0].value.terminus).toEqual('synonym');
            expect(entries[1].value.terminus).toEqual('terminus');
        });

        it('should sort synonym down', () => {
            expect(form.dirty).toEqual(false);
            expect(form.touched).toEqual(false);

            component.sortEntryLanguage(0, 1, SynonymMoveDirection.Up);

            expect(form.dirty).toEqual(true);
            expect(form.touched).toEqual(true);

            const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
            const entries = (languagesForm[0].controls.entries as FormArray).controls as FormGroup[];
            expect(entries[0].value.terminus).toEqual('synonym');
            expect(entries[1].value.terminus).toEqual('terminus');
        });

        it('should sort synonym up and down again', () => {
            expect(form.dirty).toEqual(false);
            expect(form.touched).toEqual(false);

            component.sortEntryLanguage(0, 0, SynonymMoveDirection.Down);

            expect(form.dirty).toEqual(true);
            expect(form.touched).toEqual(true);

            const languagesForm = (component.form.controls.languages as FormArray).controls as FormGroup[];
            let entries = (languagesForm[0].controls.entries as FormArray).controls as FormGroup[];
            expect(entries[0].value.terminus).toEqual('synonym');
            expect(entries[1].value.terminus).toEqual('terminus');

            component.sortEntryLanguage(0, 1, SynonymMoveDirection.Up);

            entries = (languagesForm[0].controls.entries as FormArray).controls as FormGroup[];
            expect(entries[0].value.terminus).toEqual('terminus');
            expect(entries[1].value.terminus).toEqual('synonym');
        });

        it('should get references on getFormEntry', () => {
            // form values for references are set by references-form.component, so behavior is simulated here
            const headForm = form.controls.head as FormGroup;
            headForm.setControl(
                'references',
                formBuilder.array([
                    {id: 1, entryId: 1, type: ReferenceType.None},
                    {id: 2, entryId: 2, type: ReferenceType.Specific},
                    {entryId: 3, type: ReferenceType.General},
                ])
            );

            const {references} = component.getFormEntry();

            expect(references).toHaveLength(3);
            expect(references![0]).toEqual({id: 1, entryId: 1, type: ReferenceType.None});
            expect(references![1]).toEqual({id: 2, entryId: 2, type: ReferenceType.Specific});
            expect(references![2]).toEqual({entryId: 3, type: ReferenceType.General});
        });

        it('should get subject and domains on getFormEntry', () => {
            // form values for subjects and domains are set by subject-form.component, so behavior is simulated here
            const headForm = form.controls.head as FormGroup;
            headForm.setControl(
                'subjectDomains',
                formBuilder.array([
                    {subject: 1, domains: [2, 3]},
                    {subject: 5, domains: [6, 7]},
                ])
            );
            const entryModification = component.getFormEntry();

            expect(entryModification.domainIds).toHaveLength(4);
            expect(entryModification.domainIds).toEqual([2, 3, 6, 7]);
            expect(entryModification.subjectIds).toHaveLength(2);
            expect(entryModification.subjectIds).toEqual([1, 5]);
        });

        it('should add the collection to selectable collections and assign it when assigned collection is not contained in provided collections', () => {
            const updatedCollections = [
                {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary,
                {id: 3, name: 'Collection3', code: 'COL3'} as CollectionSummary,
            ];
            collectionServiceMock.getModificationCollections.mockReturnValue(of(updatedCollections));

            langChangeSubject.next(true);

            expect(component.collections).toHaveLength(3);
            expect(component.collections).toContainEqual(formEntry.collection);
            expect(component.collections).toContainEqual(updatedCollections[0]);
            expect(component.collections).toContainEqual(updatedCollections[1]);
            expect(component._formEntry!.collection.id).toBe(1);
        });

        it('should not add default collection to selectable collections and select first entry when is not contained in provided collections', () => {
            const updatedCollections = [
                {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary,
                {id: 3, name: 'Collection3', code: 'COL3'} as CollectionSummary,
            ];
            component.formEntry = formEntryWithDefaultCollectionId;
            collectionServiceMock.getModificationCollections.mockReturnValue(of(updatedCollections));

            langChangeSubject.next(true);

            expect(component.collections).toHaveLength(2);
            expect(component.collections).toContainEqual(updatedCollections[0]);
            expect(component.collections).toContainEqual(updatedCollections[1]);
            expect(component._formEntry!.collection.id).toBe(2);
        });

        it('should add glossaries to selectable glossaries whenever assigned glossary is not contained in provided glossaries', () => {
            const existingGlossaries = [
                {id: 1, name: 'Glossary1', code: 'GLO1'},
                {id: 3, name: 'Glossary3', code: 'GLO3'},
            ];

            const updatedGlossaries = [
                {id: 2, name: 'Glossary2', code: 'GLO2'},
                {id: 3, name: 'Glossary3', code: 'GLO3'},
            ];
            glossaryServiceMock.getModificationGlossaries.mockReturnValue(of(updatedGlossaries));

            formEntry.glossaries = existingGlossaries;
            component.formEntry = formEntry;

            langChangeSubject.next(true);

            expect(component.glossaryChips).toHaveLength(3);
            expect(component.glossaryChips.some(c => c.tag === existingGlossaries[0].code)).toEqual(true);
            expect(component.glossaryChips.some(c => c.tag === existingGlossaries[1].code)).toEqual(true);
            expect(component.glossaryChips.some(c => c.tag === updatedGlossaries[0].code)).toEqual(true);
        });

        it.each([{canEditCollection: true}, {canEditCollection: false}])(
            'should set collection formfield according to canEditCollection',
            ({canEditCollection}) => {
                formEntry.canEditCollection = canEditCollection;
                component.formEntry = formEntry;

                fixture.detectChanges();

                expect(component.canEditCollection).toEqual(canEditCollection);
            }
        );
    });
});
