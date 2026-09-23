import {Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatError, MatLabel, MatOption, MatSelect} from '@angular/material/select';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObNotificationService} from '@oblique/oblique';
import {DuplicateCheckRequest} from 'api/DuplicateCheckRequest';
import {LanguageSummary} from 'api/LanguageSummary';
import {catchError, forkJoin, of, startWith, switchMap, take, tap} from 'rxjs';
import {ExtendedSearchFiltersService} from '../search/extended-search-filters.service';
import {AutocompleteComponent} from '../shared/autocomplete/autocomplete.component';
import {Option} from '../shared/autocomplete/Option';
import {ErrorService} from '../shared/error.service';
import {LanguageService} from '../shared/language.service';
import {DuplicateCheckService} from './duplicate-check.service';

@Component({
    selector: 'td-duplicate-check',
    imports: [
        TranslatePipe,
        MatLabel,
        ReactiveFormsModule,
        AutocompleteComponent,
        MatError,
        MatOption,
        MatFormFieldModule,
        MatIcon,
        ObButtonDirective,
        MatButton,
        MatSelect,
    ],
    templateUrl: './duplicate-check.component.html',
    styleUrl: './duplicate-check.component.scss',
})
export class DuplicateCheckComponent implements OnInit {
    collections = signal<Option[]>([]);
    glossaries = signal<Option[]>([]);
    languageOptions = signal<LanguageSummary[]>([]);
    form: FormGroup;
    private readonly destroyRef = inject(DestroyRef);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        protected readonly errorService: ErrorService,
        private readonly translateService: TranslateService,
        private readonly languageService: LanguageService,
        private readonly duplicateCheckService: DuplicateCheckService,
        private readonly notificationService: ObNotificationService
    ) {
        this.form = this.formBuilder.group(
            {
                language: this.formBuilder.control<number | null>(null, {validators: [Validators.required]}),
                collection: this.formBuilder.control<string | null>(null),
                glossary: this.formBuilder.control<string | null>(null),
            },
            {
                validators: exactlyOneValidator,
            }
        );
    }

    ngOnInit(): void {
        const collectionCtrl = this.form.get('collection');
        const glossaryCtrl = this.form.get('glossary');

        collectionCtrl?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(value => {
            if (value) {
                glossaryCtrl?.disable({emitEvent: false});
            } else {
                glossaryCtrl?.enable({emitEvent: false});
            }
        });

        glossaryCtrl?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(value => {
            if (value) {
                collectionCtrl?.disable({emitEvent: false});
            } else {
                collectionCtrl?.enable({emitEvent: false});
            }
        });

        this.translateService.onLangChange
            .pipe(
                startWith(null),
                switchMap(() =>
                    forkJoin({
                        collections: this.extendedSearchFilterService.getCollections().pipe(catchError(() => of([]))),
                        glossaries: this.extendedSearchFilterService.getGlossaries().pipe(catchError(() => of([]))),
                        languageOptions: this.languageService.languageSummaries$.pipe(take(1)),
                    })
                ),
                tap(({collections, glossaries, languageOptions}) => {
                    this.collections.set(collections);
                    this.glossaries.set(glossaries);
                    this.languageOptions.set(languageOptions);
                }),
                takeUntilDestroyed(this.destroyRef)
            )
            .subscribe();
    }

    reset() {
        this.form.reset();
    }

    exportAsText() {
        const request: DuplicateCheckRequest = {
            languageId: this.form.value.language,
            collectionId: this.form.value.collection,
            glossaryId: this.form.value.glossary,
        };

        this.duplicateCheckService.performDuplicateCheck(request).subscribe(
            response => {
                this.duplicateCheckService.exportDuplicateCheckTxt(response.ids);
            },
            () => {
                this.notificationService.error(this.translateService.instant('duplicateCheck.exportError'));
            }
        );
    }

    exportAsXlsx() {
        const request: DuplicateCheckRequest = {
            languageId: this.form.value.language,
            collectionId: this.form.value.collection,
            glossaryId: this.form.value.glossary,
        };

        this.duplicateCheckService.exportDuplicateCheckXlsx(request).subscribe({
            error: () => {
                this.notificationService.error(this.translateService.instant('duplicateCheck.exportError'));
            },
        });
    }
}

export const exactlyOneValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    const hasCollection = !!group.get('collection')?.value;
    const hasGlossary = !!group.get('glossary')?.value;

    return hasCollection !== hasGlossary ? null : {exactlyOneRequired: true};
};
