import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {Component, OnDestroy, OnInit} from '@angular/core';
import {FormArray, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatInput} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {Router} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {FileImport} from 'api/FileImport';
import {FileImportResult} from 'api/FileImportResult';
import {GlossarySummary} from 'api/GlossarySummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {catchError, EMPTY, forkJoin, merge, of, Subject, switchMap, takeUntil, tap} from 'rxjs';
import {CollectionService} from '../../administration/collection/collection.service';
import {GlossaryService} from '../../administration/glossary/glossary.service';
import {EntryService} from '../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {Chip} from '../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';
import {FileSizePipe} from '../../shared/file-size.pipe';
import {toCodeNameLabel} from '../../shared/LabelFormat';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {FileImportErrorComponent} from './file-import-error/file-import-error.component';
import {FileImportUploadComponent} from './file-import-upload/file-import-upload.component';
import {FileImportService} from './file-import.service';

@Component({
    selector: 'td-file-import',
    templateUrl: './file-import.component.html',
    styleUrls: ['./file-import.component.scss'],
    imports: [
        ReactiveFormsModule,
        FileImportUploadComponent,
        AutocompleteComponent,
        MatLabel,
        MatError,
        SubjectFormComponent,
        ChiplistAutocompleteComponent,
        MatFormField,
        MatSelect,
        MatOption,
        MatInput,
        CdkTextareaAutosize,
        ChiplistAutocompleteInputComponent,
        MatButton,
        ObButtonDirective,
        FileImportErrorComponent,
        FileSizePipe,
        TranslatePipe,
    ],
})
export class FileImportComponent implements OnInit, OnDestroy {
    offices: OfficeSummary[] = [];
    collections: CollectionSummary[] = [];
    adminInfos: AdminInfoSummary[] = [];
    adminInfoChips: Chip[] = [];
    glossaryChips: Chip[] = [];
    states: Option[] = [];
    reliabilities: Option[] = [];
    subjectDomainsForm!: FormArray;
    noFileSelected = true;
    importedFile!: File | undefined;
    importResult!: FileImportResult;
    displayErrors = false;
    errorList!: string[];

    form: UntypedFormGroup;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly errorService: ErrorService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly translateService: TranslateService,
        private readonly fileImportService: FileImportService,
        private readonly entryService: EntryService,
        private readonly collectionService: CollectionService,
        private readonly glossaryService: GlossaryService,
        private readonly notificationService: ObNotificationService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly router: Router,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {
        this.form = this.formBuilder.group({
            office: this.formBuilder.control('', Validators.required),
            collection: this.formBuilder.control('', Validators.required),
            reliability: this.formBuilder.control('', Validators.required),
            status: this.formBuilder.control('', Validators.required),
            comment: this.formBuilder.control('', Validators.maxLength(1000)),
            glossaries: this.formBuilder.control([]),
            adminInfos: this.formBuilder.control([]),
            subjectDomains: this.formBuilder.array([], {validators: [Validators.required]}),
        });
    }

    ngOnInit(): void {
        this.subjectDomainsForm = this.form.controls.subjectDomains as FormArray;

        merge(of({lang: null, translations: null}), this.translateService.onLangChange)
            .pipe(
                switchMap(langChangeEvent =>
                    forkJoin({
                        langChangeEvent: of(langChangeEvent),
                        collections: this.collectionService.getModificationCollections().pipe(catchError(() => of([] as CollectionSummary[]))),
                        offices: this.extendedSearchFilterService.getOffices().pipe(catchError(() => of([] as OfficeSummary[]))),
                        states: this.extendedSearchFilterService.getStates().pipe(catchError(() => of([] as Option[]))),
                        reliabilities: this.entryService.getReliabilities().pipe(catchError(() => of([] as Option[]))),
                        adminInfos: this.extendedSearchFilterService.getAdminInfos().pipe(catchError(() => of([] as AdminInfoSummary[]))),
                        glossaries: this.glossaryService.getModificationGlossaries().pipe(catchError(() => of([] as GlossarySummary[]))),
                    })
                ),
                takeUntil(this.unsubscribe),
                tap(({collections, offices, states, reliabilities, adminInfos, glossaries}) => {
                    this.offices = offices;
                    this.states = states;
                    this.adminInfos = adminInfos;
                    this.collections = collections;
                    this.adminInfoChips = adminInfos.map(adminInfo => new Chip(adminInfo.code, adminInfo.code, adminInfo.id));
                    this.glossaryChips = glossaries.map(glossary => new Chip(glossary.code, toCodeNameLabel(glossary), glossary.id));
                    this.reliabilities = reliabilities;
                })
            )
            .subscribe();
    }

    ngOnDestroy(): void {}

    importFile(file: File): void {
        this.displayErrors = false;
        this.importedFile = file;
        this.noFileSelected = false;
    }

    import(): void {
        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        if (this.form.valid && !this.noFileSelected) {
            this.fileImportService
                .importFile(this.getFormValues())
                .pipe(
                    catchError(error => {
                        this.errorList = Object.values<string>(error.error?.errors).flat();
                        this.displayErrors = true;
                        return EMPTY;
                    })
                )
                .subscribe(data => {
                    this.importResult = data;

                    this.noFileSelected = true;
                    this.importedFile = undefined;
                    this.notificationService.success({
                        message: this.translateService.instant('file.import.success', {adminInfo: data.adminInfoCode}),
                        timeout: 15000,
                    });
                });
        } else {
            this.form.markAllAsTouched();
        }
    }

    cancel(): void {
        void this.router.navigate(['search']);
    }

    private getFormValues(): FileImport {
        const importData = this.form.getRawValue();

        return {
            entryOffice: importData.office,
            entryCollection: importData.collection,
            reliability: importData.reliability,
            status: importData.status,
            comment: importData.comment,

            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            subjectIds: importData.subjectDomains.map((sub: any) => {
                return sub.subject;
            }),
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            domainIds: importData.subjectDomains.flatMap((sub: any) => {
                return sub.domains;
            }),
            glossaryIds: importData.glossaries,
            adminInfos:
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                importData.adminInfos?.map((info: any) => {
                    return {
                        id: info.value ?? undefined,
                        code: info.displayText,
                    } as AdminInfoSummary;
                }) ?? [],

            file: this.importedFile,
        } as unknown as FileImport;
    }
}
