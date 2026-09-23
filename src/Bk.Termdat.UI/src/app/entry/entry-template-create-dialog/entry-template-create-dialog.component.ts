import {CdkScrollable} from '@angular/cdk/scrolling';
import {KeyValuePipe} from '@angular/common';
import {Component, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';

@Component({
    selector: 'td-entry-template-create-dialog',
    templateUrl: './entry-template-create-dialog.component.html',
    styleUrls: ['./entry-template-create-dialog.component.scss'],
    imports: [
        MatDialogTitle,
        MatIconButton,
        MatDialogClose,
        MatIcon,
        CdkScrollable,
        MatDialogContent,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatError,
        MatDialogActions,
        MatButton,
        TranslatePipe,
        KeyValuePipe,
    ],
})
export class EntryTemplateCreateDialogComponent implements OnInit {
    templateForm!: UntypedFormGroup;

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly dialogRef: MatDialogRef<EntryTemplateCreateDialogComponent>
    ) {}

    ngOnInit(): void {
        this.templateForm = this.formBuilder.group({
            name: null,
        });
    }

    createTemplate(): void {
        this.templateForm.markAsTouched();

        if (this.templateForm.invalid) {
            return;
        }

        const templateName = this.templateForm.controls.name?.value;
        this.dialogRef.close(templateName);
    }
}
