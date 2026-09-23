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
import {AdminInfoModification} from 'api/AdminInfoModification';

@Component({
    selector: 'td-admin-info-form',
    templateUrl: './admin-info-form.component.html',
    styleUrls: ['./admin-info-form.component.scss'],
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
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class AdminInfoFormComponent {
    form: UntypedFormGroup;

    @Input() titleTranslateKey!: string;
    @Input() set adminInfo(value: AdminInfoModification) {
        this.form.reset(value);
    }
    @Output() readonly saved = new EventEmitter<AdminInfoModification>();

    constructor(private readonly formBuilder: UntypedFormBuilder) {
        this.form = this.formBuilder.group({
            id: {value: '', disabled: true},
            code: this.formBuilder.control('', Validators.maxLength(100)),
            description: this.formBuilder.control('', Validators.maxLength(1000)),
        });
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const adminInfo = this.form.value as AdminInfoModification;

        this.saved.emit(adminInfo);
    }
}
