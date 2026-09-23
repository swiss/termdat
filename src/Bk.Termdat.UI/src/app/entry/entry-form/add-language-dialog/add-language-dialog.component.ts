import {CdkScrollable} from '@angular/cdk/scrolling';
import {Component, Inject, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormControl, ValidatorFn} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {LanguageSummary} from 'api/LanguageSummary';
import {selectedCheckboxesValidator} from '../../../shared/selected-checkboxes.validator';

@Component({
    selector: 'td-add-language-dialog',
    templateUrl: './add-language-dialog.component.html',
    styleUrls: ['./add-language-dialog.component.scss'],
    imports: [
        MatDialogTitle,
        CdkScrollable,
        MatDialogContent,
        MatCheckbox,
        ReactiveFormsModule,
        MatDialogActions,
        MatButton,
        ObButtonDirective,
        MatDialogClose,
        TranslatePipe,
    ],
})
export class AddLanguageDialogComponent implements OnInit {
    form: UntypedFormArray;
    checkboxes: UntypedFormControl[];

    constructor(
        private readonly dialogRef: MatDialogRef<AddLanguageDialogComponent>,
        private readonly formBuilder: UntypedFormBuilder,
        @Inject(MAT_DIALOG_DATA) public languages: LanguageSummary[]
    ) {
        this.form = this.formBuilder.array([], selectedCheckboxesValidator(1) as ValidatorFn);
        this.checkboxes = this.form.controls as UntypedFormControl[];
    }

    ngOnInit(): void {
        this.languages.forEach(() => {
            this.form.push(this.formBuilder.control(false));
        });
    }

    confirmDialog(): void {
        this.form.markAsTouched();

        if (this.form.invalid) {
            return;
        }

        const selected: LanguageSummary[] = [];
        const values = this.form.value as boolean[];

        values.forEach((value: boolean, i: number) => {
            if (!value) {
                return;
            }

            selected.push(this.languages[i]);
        });

        this.dialogRef.close(selected);
    }
}
