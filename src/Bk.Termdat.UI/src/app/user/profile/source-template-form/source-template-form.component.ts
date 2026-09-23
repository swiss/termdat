import {ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatMenu, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {SourceTemplateModification} from 'api/SourceTemplateModification';
import {SourceTemplateSummary} from 'api/SourceTemplateSummary';

@Component({
    selector: 'td-source-template-form',
    templateUrl: './source-template-form.component.html',
    styleUrls: ['./source-template-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ReactiveFormsModule,
        MatIconButton,
        ObButtonDirective,
        MatMenuTrigger,
        MatIcon,
        MatMenu,
        MatFormField,
        MatLabel,
        MatInput,
        MatError,
        MatButton,
        MatTooltip,
        TranslatePipe,
    ],
})
export class SourceTemplateFormComponent implements OnInit {
    @Input() sourceTemplate: SourceTemplateSummary | undefined;
    @Input() updateMode = false;
    @Output() readonly create = new EventEmitter<SourceTemplateModification>();
    @Output() readonly update = new EventEmitter<SourceTemplateModification>();
    @Output() readonly delete = new EventEmitter<SourceTemplateSummary>();
    form: FormGroup;

    private submitted = false;

    public get isEmpty() {
        return (
            this.submitted &&
            this.isNullOrEmpty(this.form.controls.textDe.value) &&
            this.isNullOrEmpty(this.form.controls.textFr.value) &&
            this.isNullOrEmpty(this.form.controls.textIt.value) &&
            this.isNullOrEmpty(this.form.controls.textEn.value) &&
            this.isNullOrEmpty(this.form.controls.textRm.value)
        );
    }

    constructor(private readonly fb: FormBuilder) {
        this.form = this.createEmptyForm();
    }

    ngOnInit(): void {
        this.fillForm();
    }

    resetFields() {
        this.submitted = false;

        if (this.updateMode) {
            this.fillForm();
        } else {
            this.form = this.createEmptyForm();
        }
    }

    deleteEntry() {
        this.delete.emit(this.sourceTemplate);
    }

    createOrUpdateSourceTemplate(): boolean {
        this.submitted = true;

        const dto = {
            id: this.sourceTemplate?.id,
            textDe: this.form.controls.textDe.value,
            textFr: this.form.controls.textFr.value,
            textIt: this.form.controls.textIt.value,
            textEn: this.form.controls.textEn.value,
            textRm: this.form.controls.textRm.value,
        };
        if (this.isEmpty) {
            return false;
        }

        if (this.updateMode) {
            this.update.emit(dto);
        } else {
            this.create.emit(dto);
        }

        this.resetFields();
        return true;
    }

    private createEmptyForm(): FormGroup {
        return this.fb.group({
            textDe: {value: '', disabled: false},
            textFr: {value: '', disabled: false},
            textIt: {value: '', disabled: false},
            textEn: {value: '', disabled: false},
            textRm: {value: '', disabled: false},
        });
    }

    private fillForm() {
        this.form = this.fb.group({
            textDe: {value: this.sourceTemplate?.textDe, disabled: false},
            textFr: {value: this.sourceTemplate?.textFr, disabled: false},
            textIt: {value: this.sourceTemplate?.textIt, disabled: false},
            textEn: {value: this.sourceTemplate?.textEn, disabled: false},
            textRm: {value: this.sourceTemplate?.textRm, disabled: false},
        });
    }

    private isNullOrEmpty(value: string | number | undefined | null): boolean {
        return value === undefined || value === null || value.toString().trim() === '';
    }
}
