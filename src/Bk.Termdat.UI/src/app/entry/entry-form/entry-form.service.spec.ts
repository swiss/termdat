import {TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {CollectionSummary} from 'api/CollectionSummary';
import {ConceptSystemPositionSummary} from 'api/ConceptSystemPositionSummary';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryModificationLanguageDetail} from 'api/EntryModificationLanguageDetail';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {GlossarySummary} from 'api/GlossarySummary';
import {LenochClassificationSummary} from 'api/LenochClassificationSummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {EntryFormService} from './entry-form.service';

describe('EntryFormService', () => {
    let service: EntryFormService;
    let formBuilder: FormBuilder;
    let detail: EntryModificationLanguageDetail;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [EntryFormService],
            imports: [ReactiveFormsModule],
        });
        service = TestBed.inject(EntryFormService);
        formBuilder = TestBed.inject(FormBuilder);

        detail = {
            abbreviation: 'abbreviation',
            abbreviationSource: 'abbreviationSource',
            context: 'context',
            contextSource: 'contextSource',
            country: 'country',
            definition: 'definition',
            definitionSource: 'definitionSource',
            id: 1,
            sequence: 1,
            languageId: 1,
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
        };
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should create empty language detail form', () => {
        const form = service.createLanguageDetailFormGroup();

        expect(form).toBeDefined();
        expect(form.getRawValue()).toEqual({
            abbreviation: null,
            abbreviationSource: null,
            context: null,
            contextSource: null,
            country: null,
            definition: null,
            definitionSource: null,
            id: null,
            trackingId: 0,
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
        });

        expect(form.touched).toEqual(false);
        expect(form.pristine).toEqual(true);
        expect(form.valid).toEqual(false);
    });

    it.each([{field: 'name'}, {field: 'terminus'}, {field: 'phraseology'}, {field: 'abbreviation'}])(
        'should add atLeastOneValidator to language detail form for $field',
        ({field}) => {
            const form = service.createLanguageDetailFormGroup();

            expect(form.errors).toEqual({atLeastOne: true});

            form.get(field)?.setValue('test');
            expect(form.valid).toEqual(true);
            expect(form.errors).toEqual(null);
        }
    );

    it.each([
        {field: 'name'},
        {field: 'terminus'},
        {field: 'phraseology'},
        {field: 'abbreviation'},
        {field: 'definition'},
        {field: 'note'},
        {field: 'context'},
        {field: 'metadata'},
    ])('should add dependencyValidator for $field and corrosponding source', ({field}) => {
        const form = service.createLanguageDetailFormGroup();
        const atLeastOneField = field === 'name' ? 'terminus' : 'name';
        form.get(atLeastOneField)?.setValue('test');
        expect(form.valid).toEqual(true);

        form.get(`${field}Source`)?.setValue('test');
        expect(form.valid).toEqual(false);
        expect(form.errors).toEqual({'dependency.required': true});

        form.get(field)?.setValue('test');
        expect(form.valid).toEqual(true);
        expect(form.errors).toEqual(null);
    });

    it('should create language detail form from language detail', () => {
        const form = service.createLanguageDetailFormGroup(detail);

        expect(form.getRawValue()).toEqual({
            id: detail.id,
            trackingId: 0,
            abbreviation: detail.abbreviation,
            abbreviationSource: detail.abbreviationSource,
            context: detail.context,
            contextSource: detail.contextSource,
            country: detail.country,
            definition: detail.definition,
            definitionSource: detail.definitionSource,
            metadata: detail.metadata,
            metadataSource: detail.metadataSource,
            name: detail.name,
            nameSource: detail.nameSource,
            note: detail.note,
            noteSource: detail.noteSource,
            phraseology: detail.phraseology,
            phraseologySource: detail.phraseologySource,
            terminus: detail.terminus,
            terminusSource: detail.terminusSource,
        });

        expect(form.touched).toEqual(false);
        expect(form.pristine).toEqual(true);
        expect(form.valid).toEqual(true);
    });

    it('should increment trackingId for each new language detail form', () => {
        const form1 = service.createLanguageDetailFormGroup();
        const form2 = service.createLanguageDetailFormGroup();
        const form3 = service.createLanguageDetailFormGroup(detail);

        expect(form1.controls.trackingId.value).toEqual(0);
        expect(form2.controls.trackingId.value).toEqual(1);
        expect(form3.controls.trackingId.value).toEqual(2);
    });

    it('should setup empty head form', () => {
        const group = formBuilder.group({});

        service.setupHeadForm(group);

        expect(group.touched).toEqual(false);
        expect(group.pristine).toEqual(true);
        expect(group.valid).toEqual(false);

        expect(group.getRawValue()).toEqual({
            head: {
                adminInfos: [],
                collection: null,
                comment: null,
                conceptSystemPosition: null,
                lenochClassifications: [],
                id: null,
                identification: null,
                office: null,
                relatedCollections: [],
                glossaries: [],
                reliability: null,
                status: null,
                subjectDomains: [],
                references: [],
                externalLinks: [],
            },
            picture: {},
        });

        expect(group.get('head.status')?.errors).toEqual({required: true});
        expect(group.get('head.reliability')?.errors).toEqual({required: true});
        expect(group.get('head.office')?.errors).toEqual({required: true});
        expect(group.get('head.subjectDomains')?.errors).toEqual({required: true});
        expect(group.get('head.collection')?.errors).toEqual({required: true});
    });

    it('should setup head form from entry', () => {
        const group = formBuilder.group({});

        const formEntry: EntryModificationDetail = {
            id: 1,
            status: Status.Validated,
            reliability: Reliability.Translated,
            identification: 10,
            comment: 'comment',
            adminInfos: [],
            office: {id: 5} as OfficeSummary,
            collection: {id: 15, active: true} as CollectionSummary,

            lenochClassifications: [{id: 1} as LenochClassificationSummary, {id: 2} as LenochClassificationSummary],
            relatedCollections: [{id: 3, active: true} as CollectionSummary, {id: 4} as CollectionSummary],
            glossaries: [{id: 5} as GlossarySummary, {id: 6} as GlossarySummary],
            conceptSystemPosition: {id: 20} as ConceptSystemPositionSummary,
            references: [{id: 7} as EntryReferenceDetail, {id: 8} as EntryReferenceDetail],
            externalLinks: [],
            subjects: [{id: 1, name: 'name1', domains: []}],
        };

        service.setupHeadForm(group, formEntry);

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const head = (group.controls as any).head as FormGroup;
        head.setControl(
            'subjectDomains',
            formBuilder.array([
                {subject: 1, domains: [2, 3]},
                {subject: 5, domains: [6, 7]},
            ])
        );

        expect(group.touched).toEqual(false);
        expect(group.pristine).toEqual(true);
        expect(group.valid).toEqual(true);

        expect(group.getRawValue()).toEqual({
            head: {
                id: 1,
                status: Status.Validated,
                reliability: Reliability.Translated,
                identification: 10,
                office: 5,
                collection: 15,
                comment: 'comment',
                adminInfos: [],
                lenochClassifications: [1, 2],
                relatedCollections: [3, 4],
                glossaries: [5, 6],
                conceptSystemPosition: 20,
                references: [],
                externalLinks: [],
                subjectDomains: [
                    {domains: [2, 3], subject: 1},
                    {domains: [6, 7], subject: 5},
                ],
            },
            picture: {},
        });
    });

    it('should setup languages form', () => {
        const baseGroup = formBuilder.group({});

        service.setupLanguagesForm(baseGroup, []);

        expect(baseGroup.getRawValue()).toEqual({languages: []});

        expect(baseGroup.touched).toEqual(false);
        expect(baseGroup.pristine).toEqual(true);
        expect(baseGroup.valid).toEqual(false);
        expect((baseGroup.get('languages') as unknown as FormArray).errors).toEqual({
            language: true,
        });
    });

    it('should create empty languages form group', () => {
        const group = service.createEmptyLanguageFormGroup(1);

        expect(group.touched).toEqual(false);
        expect(group.pristine).toEqual(true);
        expect(group.valid).toEqual(true);

        expect(group.getRawValue()).toEqual({
            entries: [],
            languageId: 1,
        });
    });

    it('should create empty languages form group with empty language detail', () => {
        const group = service.createEmptyLanguageFormGroup(1, true);

        expect(group.touched).toEqual(false);
        expect(group.pristine).toEqual(true);
        expect(group.valid).toEqual(false);

        expect(group.getRawValue()).toEqual({
            entries: [
                {
                    abbreviation: null,
                    abbreviationSource: null,
                    context: null,
                    contextSource: null,
                    country: null,
                    definition: null,
                    definitionSource: null,
                    id: null,
                    trackingId: 0,
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
                },
            ],
            languageId: 1,
        });
    });

    it('should create language form group from languages details', () => {
        const details = [{...detail}, {...detail}];

        const group = service.createLanguageFormGroup(1, details);

        expect(group.touched).toEqual(false);
        expect(group.pristine).toEqual(true);
        expect(group.valid).toEqual(true);

        expect(group.getRawValue()).toEqual({
            entries: [
                {
                    id: detail.id,
                    trackingId: 0,
                    abbreviation: detail.abbreviation,
                    abbreviationSource: detail.abbreviationSource,
                    context: detail.context,
                    contextSource: detail.contextSource,
                    country: detail.country,
                    definition: detail.definition,
                    definitionSource: detail.definitionSource,
                    metadata: detail.metadata,
                    metadataSource: detail.metadataSource,
                    name: detail.name,
                    nameSource: detail.nameSource,
                    note: detail.note,
                    noteSource: detail.noteSource,
                    phraseology: detail.phraseology,
                    phraseologySource: detail.phraseologySource,
                    terminus: detail.terminus,
                    terminusSource: detail.terminusSource,
                },
                {
                    id: detail.id,
                    trackingId: 1,
                    abbreviation: detail.abbreviation,
                    abbreviationSource: detail.abbreviationSource,
                    context: detail.context,
                    contextSource: detail.contextSource,
                    country: detail.country,
                    definition: detail.definition,
                    definitionSource: detail.definitionSource,
                    metadata: detail.metadata,
                    metadataSource: detail.metadataSource,
                    name: detail.name,
                    nameSource: detail.nameSource,
                    note: detail.note,
                    noteSource: detail.noteSource,
                    phraseology: detail.phraseology,
                    phraseologySource: detail.phraseologySource,
                    terminus: detail.terminus,
                    terminusSource: detail.terminusSource,
                },
            ],
            languageId: 1,
        });
    });
});
