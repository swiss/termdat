import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, EventEmitter, Input, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {OfficeModification} from 'api/OfficeModification';

@Component({
    selector: 'td-office-form',
    templateUrl: './office-form.component.html',
    styleUrls: ['./office-form.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        RouterLink,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatError,
        CdkTextareaAutosize,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class OfficeFormComponent {
    form: UntypedFormGroup;

    @Input() titleTranslateKey!: string;
    @Input() set office(value: OfficeModification) {
        this.form.reset(value);
    }
    @Output() readonly saved = new EventEmitter<OfficeModification>();

    constructor(private readonly formBuilder: UntypedFormBuilder) {
        this.form = this.formBuilder.group({
            id: {value: '', disabled: true},
            code: this.formBuilder.control('', Validators.maxLength(5)),
            descriptionDe: '',
            descriptionFr: '',
            descriptionIt: '',
            descriptionRm: '',
            descriptionEn: '',
        });
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const office = this.form.value as OfficeModification;

        this.saved.emit(office);
    }
}
