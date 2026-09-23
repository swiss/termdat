import {moveItemInArray} from '@angular/cdk/drag-drop';
import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {NgClass} from '@angular/common';
import {Component, Inject, Input, OnDestroy, OnInit} from '@angular/core';
import {FormArray, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatBadge} from '@angular/material/badge';
import {MatButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelHeader, MatExpansionPanelTitle} from '@angular/material/expansion';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {EntryModification} from 'api/EntryModification';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryModificationExternalLink} from 'api/EntryModificationExternalLink';
import {EntryModificationLanguageDetail} from 'api/EntryModificationLanguageDetail';
import {EntryModificationReference} from 'api/EntryModificationReference';
import {GlossarySummary} from 'api/GlossarySummary';
import {LanguageSummary} from 'api/LanguageSummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {PictureDetail} from 'api/PictureDetail';
import {defer, forkJoin, iif, merge, of, Subject} from 'rxjs';
import {catchError, distinctUntilChanged, first, map, switchMap, takeUntil, tap} from 'rxjs/operators';
import {CollectionService} from '../../administration/collection/collection.service';
import {GlossaryService} from '../../administration/glossary/glossary.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {Option as AutocompleteOption} from '../../shared/autocomplete/Option';
import {Chip} from '../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';
import {WINDOW} from '../../shared/injection.tokens';
import {toCodeNameLabel} from '../../shared/LabelFormat';
import {LanguageService} from '../../shared/language.service';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {EntryService} from '../entry.service';
import {AddLanguageDialogComponent} from './add-language-dialog/add-language-dialog.component';
import {EntryFormPictureComponent} from './entry-form-picture/entry-form-picture.component';
import {PictureChange} from './entry-form-picture/PictureChange';
import {EntryFormSynonymComponent} from './entry-form-synonym/entry-form-synonym.component';
import {SynonymMoveDirection} from './entry-form-synonym/SynonymMoveDirection';
import {EntryFormService} from './entry-form.service';
import {ExternalLinksFormComponent} from './external-links-form/external-links-form.component';
import {ReferencesFormComponent} from './references-form/references-form.component';
import {languageDetailCompare} from './sort';
import {SourceTemplateDefaults} from './source-templates/SourceTemplateDefaults';

@Component({
    selector: 'td-entry-form',
    templateUrl: './entry-form.component.html',
    styleUrls: ['./entry-form.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatAccordion,
        MatExpansionPanel,
        NgClass,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatBadge,
        MatFormField,
        MatLabel,
        MatInput,
        MatError,
        AutocompleteComponent,
        MatSelect,
        MatOption,
        SubjectFormComponent,
        ChiplistAutocompleteComponent,
        CdkTextareaAutosize,
        ChiplistAutocompleteInputComponent,
        EntryFormPictureComponent,
        ReferencesFormComponent,
        ExternalLinksFormComponent,
        EntryFormSynonymComponent,
        MatExpansionPanelActionRow,
        MatButton,
        ObButtonDirective,
        MatIcon,
        TranslatePipe,
    ],
})
export class EntryFormComponent implements OnInit, OnDestroy {
    offices: OfficeSummary[] = [];
    collections: CollectionSummary[] = [];
    canEditCollection = false;
    adminInfos: AdminInfoSummary[] = [];
    relatedCollectionChips: Chip[] = [];
    lenochClassificationChips: Chip[] = [];
    glossaryChips: Chip[] = [];
    adminInfoChips: Chip[] = [];
    states: Option[] = [];
    languages: LanguageSummary[] = [];
    reliabilities: Option[] = [];
    languageMap: {[languageId: number]: LanguageSummary} = {};
    sourceTemplateLanguageMap: {[key: number]: keyof SourceTemplateDefaults} = {};
    pictureChange: PictureChange | null = null;
    pictureDeleted = false;
    headForm!: FormGroup;
    pictureForm!: FormGroup;
    referencesForm!: FormArray;
    externalLinksForm!: FormArray;
    subjectDomainsForm!: FormArray;

    @Input() form!: FormGroup;
    _formEntry?: EntryModificationDetail;
    @Input() set formEntry(value: EntryModificationDetail) {
        this._formEntry = value;
        if (value && this.form) {
            this.modelToForm(value);
            this.pictureDeleted = false;
            this.pictureChange = null;
        }
    }

    conceptSystemPositions: AutocompleteOption[] = [];

    private get languagesForm(): FormGroup[] {
        return (this.form.controls.languages as FormArray).controls as FormGroup[];
    }

    private static readonly DEFAULT_SOURCE_TEMPLATE_LANGUAGE = 'de';
    private readonly unsubscribe = new Subject<void>();
    private readonly patchControlsSubject = new Subject();

    constructor(
        public readonly errorService: ErrorService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly languageService: LanguageService,
        private readonly translateService: TranslateService,
        private readonly entryService: EntryService,
        private readonly dialog: MatDialog,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly entryFormService: EntryFormService,
        private readonly collectionService: CollectionService,
        private readonly glossaryService: GlossaryService,
        @Inject(WINDOW) private readonly window: Window
    ) {}

    ngOnInit(): void {
        this.entryFormService.setupHeadForm(this.form, this._formEntry);
        this.headForm = this.form.controls.head as FormGroup;
        this.pictureForm = this.form.controls.picture as FormGroup;
        this.referencesForm = this.headForm.controls.references as FormArray;
        this.externalLinksForm = this.headForm.controls.externalLinks as FormArray;
        this.subjectDomainsForm = this.headForm.controls.subjectDomains as FormArray;

        merge(of({lang: null, translations: null}), this.translateService.onLangChange)
            .pipe(
                switchMap(langChangeEvent =>
                    forkJoin({
                        langChangeEvent: of(langChangeEvent),
                        collections: this.collectionService.getModificationCollections().pipe(catchError(() => of([] as CollectionSummary[]))),
                        relatedCollections: this.collectionService.getActiveCollections().pipe(catchError(() => of([] as CollectionSummary[]))),
                        offices: this.extendedSearchFilterService.getOffices().pipe(catchError(() => of([] as OfficeSummary[]))),
                        glossaries: this.glossaryService.getModificationGlossaries().pipe(catchError(() => of([] as GlossarySummary[]))),
                        states: this.extendedSearchFilterService.getStates().pipe(catchError(() => of([] as Option[]))),
                        reliabilities: this.entryService.getReliabilities().pipe(catchError(() => of([] as Option[]))),
                        languages: this.languageService.getCurrentLanguageSummaries().pipe(catchError(() => of([] as LanguageSummary[]))),
                        adminInfos: this.extendedSearchFilterService.getAdminInfos().pipe(catchError(() => of([] as AdminInfoSummary[]))),
                    })
                ),
                takeUntil(this.unsubscribe),
                tap(({collections, relatedCollections, offices, glossaries, states, reliabilities, languages, adminInfos}) => {
                    this.offices = offices;
                    this.states = states;
                    this.adminInfos = adminInfos;
                    this.lenochClassificationChips = this._formEntry?.lenochClassifications.map(lc => new Chip(lc.code, toCodeNameLabel(lc), lc.id)) ?? [];

                    this.collections = collections;

                    if (this._formEntry?.collection.deleted) {
                        this._formEntry.collection.name += ` (${this.translateService.instant('field.deleted')})`;
                        this.collections.push(this._formEntry.collection);
                    }

                    if (!this._formEntry?.collection.active && this._formEntry?.collection) {
                        this._formEntry.collection.name += ` (${this.translateService.instant('field.inactive')})`;
                        this.collections.push(this._formEntry.collection);
                    }

                    // When updating, writers can have access to entries but not the assigned collection.
                    // In order to save, it still needs to be selectable
                    if (this._formEntry?.collection && !this.collections.some(c => c.id === this._formEntry?.collection.id)) {
                        if (!this.isNullOrEmpty(this._formEntry?.collection.code)) {
                            this.collections.push(this._formEntry.collection);
                        } else {
                            this._formEntry.collection = this.collections[0];
                        }
                    }
                    this.collections.forEach(collection => this.appendCollectionMarkers(collection));
                    relatedCollections.forEach(collection => this.appendCollectionMarkers(collection));

                    this.relatedCollectionChips = relatedCollections.map(collection => new Chip(collection.code, toCodeNameLabel(collection), collection.id));

                    this._formEntry?.relatedCollections
                        ?.filter(rc => rc.deleted || !rc.active)
                        .forEach(rc => {
                            rc.name += ` (${this.translateService.instant(rc.deleted ? 'field.deleted' : 'field.inactive')})`;
                            this.appendCollectionMarkers(rc);
                            this.relatedCollectionChips.push(new Chip(rc.code, toCodeNameLabel(rc), rc.id, true));
                        });

                    // When updating, writers can have access to entries but not the assigned glossary.
                    // In order to save, it still needs to be selectable
                    if (this._formEntry?.glossaries) {
                        this._formEntry.glossaries.forEach(glossary => {
                            if (!glossaries.some(g => g.id === glossary.id)) {
                                glossaries.push(glossary);
                            }
                        });
                    }
                    this.glossaryChips = glossaries.map(glossary => new Chip(glossary.code, toCodeNameLabel(glossary), glossary.id));
                    this.adminInfoChips = adminInfos.map(adminInfo => new Chip(adminInfo.code, adminInfo.code, adminInfo.id));
                    this.reliabilities = reliabilities;
                    this.languages = languages;
                })
            )
            .subscribe(({langChangeEvent}) => {
                if (!langChangeEvent?.lang) {
                    // Angular must render the corresponding child components before patching new values
                    this.window.setTimeout(() => {
                        this.headForm.controls.office.setValue(this._formEntry?.office?.id ?? null);
                        this.headForm.controls.collection.setValue(this._formEntry?.collection?.id ?? null);
                        const lenochClassificationIds = this._formEntry?.lenochClassifications?.map(lc => lc.id) ?? [];
                        this.headForm.controls.lenochClassifications.setValue(lenochClassificationIds);
                        const relatedCollectionIds = this._formEntry?.relatedCollections?.map(col => col.id) ?? [];
                        this.headForm.controls.relatedCollections.setValue(relatedCollectionIds);
                        const glossaryIds = this._formEntry?.glossaries?.map(glossary => glossary.id) ?? [];
                        this.headForm.controls.glossaries.setValue(glossaryIds);
                        const adminInfoIds = this._formEntry?.adminInfos?.map(adminInfo => adminInfo.id) ?? [];
                        this.headForm.controls.adminInfos.setValue(adminInfoIds);
                    }, 25);
                }

                this.createLanguagesForm(this._formEntry?.languageDetails);
            });

        this.headForm.controls.collection.valueChanges
            .pipe(
                takeUntil(this.unsubscribe),
                map((value: string) => {
                    const id = parseInt(value, 10);
                    return isNaN(id) ? null : id;
                }),
                distinctUntilChanged(),
                switchMap(collectionId =>
                    iif(
                        () => collectionId === null,
                        of([]),
                        defer(() => {
                            this.httpInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
                            return this.entryService.getConceptSystemPositions(collectionId!).pipe(
                                map(response =>
                                    response.map(item => ({
                                        id: item.id,
                                        code: '',
                                        name: item.value,
                                    }))
                                ),
                                catchError(() => of([]))
                            );
                        })
                    )
                )
            )
            .subscribe(options => {
                this.conceptSystemPositions = options;

                if (!this.conceptSystemPositions.length) {
                    this.headForm.controls.conceptSystemPosition.patchValue(null);
                    this.headForm.controls.conceptSystemPosition.disable();
                    return;
                }

                this.headForm.controls.conceptSystemPosition.enable();
                const id = this._formEntry?.conceptSystemPosition?.id;
                this.window.setTimeout(() => {
                    // Angular must render the corresponding components before patching new values
                    this.headForm.controls.conceptSystemPosition.setValue(id);
                }, 25);
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.patchControlsSubject.complete();
    }

    openAddLanguageDialog(): void {
        const existingLanguages = this.languagesForm.map(group => {
            return group.controls.languageId.value as number;
        });

        const possibleLanguages = this.languages.filter(language => !existingLanguages.some(el => el === language.id));

        const dialogRef = this.dialog.open(AddLanguageDialogComponent, {
            data: possibleLanguages,
        });

        dialogRef
            .afterClosed()
            .pipe(first())
            .subscribe((result?: LanguageSummary[]) => {
                if (!result) {
                    return;
                }

                const forms = this.form.controls.languages as FormArray;

                result.forEach((language: LanguageSummary) => {
                    forms.push(this.entryFormService.createEmptyLanguageFormGroup(language.id, true));
                });

                forms.markAsTouched();
                forms.markAsDirty();
            });
    }

    createEntryLanguage(translationFormIndex: number) {
        const formArray = this.languagesForm[translationFormIndex].controls.entries as FormArray;

        const group = this.entryFormService.createLanguageDetailFormGroup();
        formArray.push(group);

        formArray.markAsDirty();
        formArray.markAsTouched();
    }

    deleteEntryLanguage(translationFormIndex: number, entryLanguageIndex: number) {
        const formArray = this.languagesForm[translationFormIndex].controls.entries as FormArray;
        formArray.removeAt(entryLanguageIndex);

        formArray.markAsDirty();
        formArray.markAsTouched();

        if (formArray.length === 0) {
            const languageId = this.languagesForm[translationFormIndex].value.languageId as number;

            if (this.languageMap[languageId]?.isUiLanguage) {
                return;
            }

            const languages = this.form.controls.languages as FormArray;
            languages.removeAt(translationFormIndex);
        }
    }

    sortEntryLanguage(translationFormIndex: number, entryLanguageIndex: number, direction: SynonymMoveDirection) {
        const formArray = this.languagesForm[translationFormIndex].controls.entries as FormArray;
        const newEntryLanguageIndex = direction === SynonymMoveDirection.Up ? entryLanguageIndex - 1 : entryLanguageIndex + 1;

        if ((entryLanguageIndex === 0 && newEntryLanguageIndex === 1) || (entryLanguageIndex === 1 && newEntryLanguageIndex === 0)) {
            const firstDefinition = formArray.controls[0].get('definition')?.value;
            formArray.controls[1].get('definition')?.setValue(firstDefinition);
            formArray.controls[0].get('definition')?.reset(null);

            const firstDefinitionSource = formArray.controls[0].get('definitionSource')?.value;
            formArray.controls[1].get('definitionSource')?.setValue(firstDefinitionSource);
            formArray.controls[0].get('definitionSource')?.reset(null);
        }

        moveItemInArray(formArray.controls, entryLanguageIndex, newEntryLanguageIndex);

        formArray.markAsDirty();
        formArray.markAsTouched();
    }

    getFormEntry(): EntryModification {
        const {head, languages} = this.form.getRawValue();

        const languageDetails = this.getLanguageDetails(languages);
        const conceptSystemPositionId = parseInt(head.conceptSystemPosition, 10);
        const officeId = parseInt(head.office, 10);
        const collectionId = parseInt(head.collection, 10);

        let entry = {
            id: this.form.get('head.id')?.value,
            status: head.status,
            reliability: head.reliability,
            identification: head.identification,
            comment: head.comment,
            officeId: isNaN(officeId) ? null : officeId,
            collectionId: isNaN(collectionId) ? null : collectionId,
            lenochClassificationIds: head.lenochClassifications,
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            subjectIds: head.subjectDomains.map((sub: any) => {
                return sub.subject;
            }),
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            domainIds: head.subjectDomains.flatMap((sub: any) => {
                return sub.domains;
            }),
            relatedCollectionIds: head.relatedCollections,
            glossaryIds: head.glossaries,
            adminInfos:
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                head.adminInfos?.map((info: any) => {
                    return {
                        id: info.value ?? undefined,
                        code: info.displayText,
                    } as AdminInfoSummary;
                }) ?? [],
            conceptSystemPositionId: isNaN(conceptSystemPositionId) ? null : conceptSystemPositionId,
            languageDetails,
            concurrencyToken: this._formEntry?.concurrencyToken,
            references:
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                head.references?.map((ref: any) => {
                    return {
                        id: ref.id ?? undefined,
                        entryId: ref.entryId,
                        type: ref.type,
                    } as EntryModificationReference;
                }) ?? [],
            externalLinks:
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                head.externalLinks?.map((ref: any) => {
                    return {
                        id: ref.id ?? undefined,
                        url: ref.url,
                        type: ref.type,
                    } as EntryModificationExternalLink;
                }) ?? [],
        } as EntryModification;

        if (head.id) {
            const {picture, image} = this.getHead();
            entry = {
                ...entry,
                picture,
                image,
                pictureDeleted: this.pictureDeleted,
            };
        }

        return entry;
    }

    getValidationNotifications(): string[] {
        if (this.form.valid) {
            return [];
        }

        const validationErrors: string[] = [];
        if (this.headForm.invalid || this.languagesForm.some(y => y.invalid) || this.form.controls.languages.errors) {
            validationErrors.push(this.translateService.instant('entry.notification.invalidHeadOrLanguage'));
        }

        return validationErrors;
    }

    pictureChanged($event: PictureChange | null) {
        this.pictureChange = $event;
        this.pictureDeleted = $event === null;
    }

    private static getSourceTemplateCode(language: LanguageSummary): keyof SourceTemplateDefaults {
        if (!language?.isUiLanguage) {
            return EntryFormComponent.DEFAULT_SOURCE_TEMPLATE_LANGUAGE;
        }

        return (language.isoCode || EntryFormComponent.DEFAULT_SOURCE_TEMPLATE_LANGUAGE).toLowerCase() as keyof SourceTemplateDefaults;
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private getLanguageDetails(languages: any) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        return languages.flatMap((language: any) => {
            const {entries, languageId} = language;

            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            return entries.map((entry: any, i: number) => {
                return {
                    id: entry.id,
                    languageId,
                    sequence: i + 1,
                    name: entry.name,
                    nameSource: entry.nameSource,
                    terminus: entry.terminus,
                    terminusSource: entry.terminusSource,
                    phraseology: entry.phraseology,
                    phraseologySource: entry.phraseologySource,
                    abbreviation: entry.abbreviation,
                    abbreviationSource: entry.abbreviationSource,
                    definition: entry.definition,
                    definitionSource: entry.definitionSource,
                    note: entry.note,
                    noteSource: entry.noteSource,
                    context: entry.context,
                    contextSource: entry.contextSource,
                    metadata: entry.metadata,
                    metadataSource: entry.metadataSource,
                    country: entry.country,
                    concurrencyToken: entry.concurrencyToken,
                } as EntryLanguageDetail;
            });
        });
    }

    private getHead() {
        const picture = this.pictureForm.value as PictureDetail;
        picture.descriptionDe = picture.descriptionDe || undefined;
        picture.descriptionEn = picture.descriptionEn || undefined;
        picture.descriptionFr = picture.descriptionFr || undefined;
        picture.descriptionIt = picture.descriptionIt || undefined;
        picture.descriptionRm = picture.descriptionRm || undefined;
        picture.sourceDe = picture.sourceDe || undefined;
        picture.sourceEn = picture.sourceEn || undefined;
        picture.sourceFr = picture.sourceFr || undefined;
        picture.sourceIt = picture.sourceIt || undefined;
        picture.sourceRm = picture.sourceRm || undefined;

        const image = this.pictureChange ? this.pictureChange.file : undefined;
        return {picture, image};
    }

    private modelToForm(entry: EntryModificationDetail) {
        if (!this.form) {
            return;
        }

        this.headForm?.patchValue({
            id: entry.id,
            status: entry.status,
            reliability: entry.reliability,
            identification: entry.identification,
            office: entry.office.id,
            collection: entry.collection.id,
            comment: entry.comment ?? null,
            lenochClassifications: entry.lenochClassifications?.map(lc => lc.id) ?? [],
            relatedCollections: entry.relatedCollections?.map(col => col.id) ?? [],
            glossaries: entry.glossaries?.map(glossary => glossary.id) ?? [],
            adminInfos: entry.adminInfos?.map(adminInfo => adminInfo.id) ?? [],
            conceptSystemPosition: entry.conceptSystemPosition?.id,
        });

        this.canEditCollection = (entry.id === undefined || entry.canEditCollection) ?? false;
        this.createLanguagesForm(entry.languageDetails);
    }

    private createLanguagesForm(details?: EntryModificationLanguageDetail[]): void {
        const formGroups = [] as FormGroup[];
        const groupedDetails: {[id: number]: EntryModificationLanguageDetail[]} = {};

        details?.sort(languageDetailCompare).forEach(detail => {
            if (groupedDetails[detail.languageId]) {
                groupedDetails[detail.languageId].push(detail);
            } else {
                groupedDetails[detail.languageId] = [detail];
            }
        });

        const existingLanguages = this.form.controls.languages as FormArray;
        const existingLanguageControls = (existingLanguages?.controls ?? []) as FormGroup[];

        this.languages.forEach(language => {
            this.languageMap[language.id] = language;
            this.sourceTemplateLanguageMap[language.id] = EntryFormComponent.getSourceTemplateCode(language);

            const hasGroupedEntry = !!groupedDetails[language.id];
            const existingFormGroup = existingLanguageControls.filter(formGroup => formGroup.controls.languageId.value === language.id) ?? [];
            if (existingFormGroup.length > 0) {
                if (hasGroupedEntry) {
                    formGroups.push(this.entryFormService.createLanguageFormGroup(language.id, groupedDetails[language.id]));
                } else {
                    formGroups.push(existingFormGroup[0]);
                }
            } else if (hasGroupedEntry) {
                formGroups.push(this.entryFormService.createLanguageFormGroup(language.id, groupedDetails[language.id]));
            } else if (language.isUiLanguage) {
                formGroups.push(this.entryFormService.createEmptyLanguageFormGroup(language.id));
            }
        });

        this.entryFormService.setupLanguagesForm(this.form, formGroups);
    }

    private isNullOrEmpty(value: string | number | undefined | null): boolean {
        return value === undefined || value === null || value.toString().trim() === '';
    }

    private appendCollectionMarkers(collection: CollectionSummary): void {
        collection.name += collection.exportLindas ? ' (LINDAS)' : '';
        collection.name += collection.activeApi ? ` (${this.translateService.instant('administration.collection.field.activeApi')})` : '';
    }
}
