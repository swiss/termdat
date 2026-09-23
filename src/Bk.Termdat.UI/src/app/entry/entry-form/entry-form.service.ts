import {Injectable} from '@angular/core';
import {AbstractControlOptions, UntypedFormBuilder, UntypedFormGroup, ValidatorFn, Validators} from '@angular/forms';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryModificationLanguageDetail} from 'api/EntryModificationLanguageDetail';
import {atLeastOneValidator} from '../../shared/at-least-one.validator';
import {dependencyValidator} from '../../shared/dependency.validator';
import {languageEntriesValidator} from './language-entries.validator';

@Injectable({
    providedIn: 'root',
})
export class EntryFormService {
    private trackingIdCounter = 0;

    constructor(private readonly formBuilder: UntypedFormBuilder) {}

    createLanguageDetailFormGroup(languageDetail?: EntryModificationLanguageDetail): UntypedFormGroup {
        const {id, name, terminus, phraseology, abbreviation, definition, note, context, metadata, country} = this.createControls(languageDetail);
        const {nameSource, terminusSource, phraseologySource, abbreviationSource, definitionSource, noteSource, contextSource, metadataSource} =
            this.createSourceControls(languageDetail);
        const trackingId = this.formBuilder.control(this.trackingIdCounter++);

        return this.formBuilder.group(
            {
                id,
                trackingId,
                name,
                nameSource,
                terminus,
                terminusSource,
                phraseology,
                phraseologySource,
                abbreviation,
                abbreviationSource,
                definition,
                definitionSource,
                note,
                noteSource,
                context,
                contextSource,
                metadata,
                metadataSource,
                country,
            },
            {
                validators: [
                    atLeastOneValidator(Validators.required, ['name', 'terminus', 'phraseology', 'abbreviation']),
                    dependencyValidator(Validators.required, nameSource, name),
                    dependencyValidator(Validators.required, terminusSource, terminus),
                    dependencyValidator(Validators.required, phraseologySource, phraseology),
                    dependencyValidator(Validators.required, abbreviationSource, abbreviation),
                    dependencyValidator(Validators.required, definitionSource, definition),
                    dependencyValidator(Validators.required, noteSource, note),
                    dependencyValidator(Validators.required, contextSource, context),
                    dependencyValidator(Validators.required, metadataSource, metadata),
                ],
            } as AbstractControlOptions
        );
    }

    setupHeadForm(form: UntypedFormGroup, formEntry?: EntryModificationDetail): void {
        const lenochClassifications = formEntry?.lenochClassifications?.map(lc => lc.id) ?? [];
        const relatedCollections = formEntry?.relatedCollections?.map(col => col.id) ?? [];
        const glossaries = formEntry?.glossaries?.map(glossary => glossary.id) ?? [];
        const adminInfos = formEntry?.adminInfos ?? [];

        form.setControl(
            'head',
            this.formBuilder.group({
                id: this.formBuilder.control({value: formEntry?.id ?? null, disabled: true}),
                status: this.formBuilder.control(formEntry?.status ?? null, {
                    validators: [Validators.required],
                } as AbstractControlOptions),
                reliability: this.formBuilder.control(formEntry?.reliability ?? null, {
                    validators: [Validators.required],
                } as AbstractControlOptions),
                identification: this.formBuilder.control(formEntry?.identification ?? null),
                office: this.formBuilder.control(formEntry?.office?.id ?? null, {
                    validators: [Validators.required],
                } as AbstractControlOptions),
                collection: this.formBuilder.control(formEntry?.collection?.id ?? null, {
                    validators: [Validators.required],
                } as AbstractControlOptions),
                comment: this.formBuilder.control(formEntry?.comment ?? null),
                lenochClassifications: this.formBuilder.control(lenochClassifications),
                relatedCollections: this.formBuilder.control(relatedCollections),
                glossaries: this.formBuilder.control(glossaries),
                adminInfos: this.formBuilder.control(adminInfos),
                conceptSystemPosition: this.formBuilder.control({value: formEntry?.conceptSystemPosition?.id ?? null, disabled: true}),
                references: this.formBuilder.array([]),
                externalLinks: this.formBuilder.array([]),
                subjectDomains: this.formBuilder.array([], {validators: [Validators.required]}),
            })
        );

        form.setControl('picture', this.formBuilder.group({}));
    }

    setupLanguagesForm(form: UntypedFormGroup, formGroups: UntypedFormGroup[]) {
        const formArray = this.formBuilder.array(formGroups, {
            validators: [Validators.minLength(1), languageEntriesValidator(1) as ValidatorFn],
        } as AbstractControlOptions);

        form.setControl('languages', formArray);
    }

    createEmptyLanguageFormGroup(languageId: number, addEntryForm = false): UntypedFormGroup {
        const controls = addEntryForm ? [this.createLanguageDetailFormGroup()] : [];

        return this.formBuilder.group({
            languageId: this.formBuilder.control(languageId),
            entries: this.formBuilder.array(controls),
        });
    }

    createLanguageFormGroup(languageId: number, languageDetails: EntryModificationLanguageDetail[]) {
        const controls = languageDetails.map(detail => this.createLanguageDetailFormGroup(detail));

        return this.formBuilder.group({
            languageId: this.formBuilder.control(languageId),
            entries: this.formBuilder.array(controls),
        });
    }

    private createSourceControls(languageDetail: EntryModificationLanguageDetail | undefined) {
        const nameSource = this.formBuilder.control(languageDetail?.nameSource ?? null);
        const terminusSource = this.formBuilder.control(languageDetail?.terminusSource ?? null);
        const phraseologySource = this.formBuilder.control(languageDetail?.phraseologySource ?? null);
        const abbreviationSource = this.formBuilder.control(languageDetail?.abbreviationSource ?? null);
        const definitionSource = this.formBuilder.control(languageDetail?.definitionSource ?? null);
        const noteSource = this.formBuilder.control(languageDetail?.noteSource ?? null);
        const contextSource = this.formBuilder.control(languageDetail?.contextSource ?? null);
        const metadataSource = this.formBuilder.control(languageDetail?.metadataSource ?? null);
        return {
            nameSource,
            terminusSource,
            phraseologySource,
            abbreviationSource,
            definitionSource,
            noteSource,
            contextSource,
            metadataSource,
        };
    }

    private createControls(languageDetail: EntryModificationLanguageDetail | undefined) {
        const id = this.formBuilder.control({value: languageDetail?.id ?? null, disabled: true});
        const name = this.formBuilder.control(languageDetail?.name ?? null);
        const terminus = this.formBuilder.control(languageDetail?.terminus ?? null);
        const phraseology = this.formBuilder.control(languageDetail?.phraseology ?? null);
        const abbreviation = this.formBuilder.control(languageDetail?.abbreviation ?? null);
        const definition = this.formBuilder.control(languageDetail?.definition ?? null);
        const note = this.formBuilder.control(languageDetail?.note ?? null);
        const context = this.formBuilder.control(languageDetail?.context ?? null);
        const metadata = this.formBuilder.control(languageDetail?.metadata ?? null);
        const country = this.formBuilder.control(languageDetail?.country ?? null);
        return {id, name, terminus, phraseology, abbreviation, definition, note, context, metadata, country};
    }
}
