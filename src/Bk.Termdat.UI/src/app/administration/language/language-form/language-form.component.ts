import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSlideToggle} from '@angular/material/slide-toggle';
import {RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {LanguageModification} from 'api/LanguageModification';
import {Subject} from 'rxjs';
import {LanguageService} from '../../../shared/language.service';

@Component({
    selector: 'td-language-form',
    templateUrl: './language-form.component.html',
    styleUrls: ['./language-form.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        RouterLink,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatSlideToggle,
        MatError,
        CdkTextareaAutosize,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class LanguageFormComponent implements OnInit, OnDestroy {
    form: UntypedFormGroup;

    @Input() titleTranslateKey!: string;
    @Input() set language(value: LanguageModification) {
        this.form.reset(value);
    }
    @Output() readonly saved = new EventEmitter<LanguageModification>();

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly languageService: LanguageService
    ) {
        this.form = this.formBuilder.group({
            id: {value: '', disabled: true},
            isoCode: this.formBuilder.control('', [Validators.maxLength(2), Validators.pattern(/[A-Z]{2}/)]),
            descriptionDe: '',
            descriptionFr: '',
            descriptionIt: '',
            descriptionRm: '',
            descriptionEn: '',
            isUiLanguage: false,
            sortIndex: this.formBuilder.control('', [Validators.required, Validators.min(1), Validators.pattern(/^-?(0|[1-9]\d*)?$/)]),
        });
    }

    ngOnInit(): void {
        if (this.form.controls.sortIndex.value === '') {
            this.languageService.nextSort().subscribe(data => {
                this.form.controls.sortIndex.patchValue(data);
            });
        }
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    isoToUppercase(event: Event) {
        const inputValue = (event.target as HTMLInputElement)?.value;
        this.form.controls.isoCode.patchValue(inputValue.toUpperCase());
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const language = this.form.value as LanguageModification;

        this.saved.emit(language);
    }
}
