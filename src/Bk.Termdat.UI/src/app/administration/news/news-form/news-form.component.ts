import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, effect, EventEmitter, input, Input, Output} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatDatepickerModule, MatDatepickerToggle, MatDateRangeInput, MatDateRangePicker, MatEndDate, MatStartDate} from '@angular/material/datepicker';
import {MatError, MatFormField, MatLabel, MatSuffix} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSlideToggle} from '@angular/material/slide-toggle';
import {RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {formatDateTime} from '../../../shared/date-util';
import {RichTextEditorComponent} from '../../../shared/rich-text-editor/rich-text-editor.component';

@Component({
    selector: 'td-news-form',
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
        MatDateRangeInput,
        MatDateRangePicker,
        MatDatepickerToggle,
        MatStartDate,
        MatEndDate,
        MatDatepickerModule,
        MatSuffix,
        RichTextEditorComponent,
    ],
    templateUrl: './news-form.component.html',
    styleUrl: './news-form.component.scss',
})
export class NewsFormComponent {
    form: FormGroup;
    initialData = input<NewsItemModification | null>(null);
    @Input() titleTranslateKey!: string;
    @Input() set newsItem(value: NewsItemModification) {
        this.form.reset({
            ...value,
            created: value.created ? formatDateTime(new Date(value.created)) : '',
            modified: value.modified ? formatDateTime(new Date(value.modified)) : '',
        });
    }
    @Output() readonly saved = new EventEmitter<NewsItemModification>();

    readonly initialDataEffect = effect(() => {
        const data = this.initialData();

        if (!data) {
            return;
        }

        this.form.patchValue({
            titleDe: data.titleDe,
            titleFr: data.titleFr,
            titleIt: data.titleIt,
            titleRm: data.titleRm,
            titleEn: data.titleEn,
            textDe: data.textDe,
            textFr: data.textFr,
            textIt: data.textIt,
            textRm: data.textRm,
            textEn: data.textEn,
            validFrom: data.validFrom,
            validTo: data.validTo,
            isEnabled: data.isEnabled,
        });
    });

    constructor(private readonly formBuilder: FormBuilder) {
        this.form = this.formBuilder.group({
            id: this.formBuilder.control<number | null>({value: null, disabled: true}),
            isEnabled: this.formBuilder.control<boolean | null>({value: true, disabled: false}),
            validFrom: this.formBuilder.control<Date | null>({value: null, disabled: false}, [Validators.required]),
            validTo: this.formBuilder.control<Date | null>({value: null, disabled: false}, [Validators.required]),
            titleDe: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(1000)]),
            titleFr: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(1000)]),
            titleIt: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(1000)]),
            titleRm: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(1000)]),
            titleEn: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(1000)]),
            textDe: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(4000)]),
            textFr: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(4000)]),
            textIt: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(4000)]),
            textRm: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(4000)]),
            textEn: this.formBuilder.control<string | null>(null, [Validators.required, Validators.maxLength(4000)]),
            created: this.formBuilder.control<Date | null>({value: null, disabled: true}),
            createdBy: this.formBuilder.control<string | null>({value: null, disabled: true}),
            modified: this.formBuilder.control<Date | null>({value: null, disabled: true}),
            modifiedBy: this.formBuilder.control<string | null>({value: null, disabled: true}),
        });
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const newsItem = this.form.value as NewsItemModification;

        this.saved.emit(newsItem);
    }
}
