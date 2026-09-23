import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, EventEmitter, Input, OnDestroy, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {GlossaryModification} from 'api/GlossaryModification';
import {Subject} from 'rxjs';
import {formatDateTime} from '../../../shared/date-util';

@Component({
    selector: 'td-glossary-form',
    templateUrl: './glossary-form.component.html',
    styleUrls: ['./glossary-form.component.scss'],
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
export class GlossaryFormComponent implements OnDestroy {
    form: UntypedFormGroup;

    @Input() titleTranslateKey!: string;
    @Input() set glossary(value: GlossaryModification) {
        this.form.reset({
            ...value,
            created: value.created ? formatDateTime(new Date(value.created)) : '',
            modified: value.modified ? formatDateTime(new Date(value.modified)) : '',
        });
    }
    @Output() readonly saved = new EventEmitter<GlossaryModification>();

    private readonly unsubscribe = new Subject<void>();

    constructor(private readonly formBuilder: UntypedFormBuilder) {
        this.form = this.formBuilder.group({
            id: this.formBuilder.control({value: '', disabled: true}),
            code: this.formBuilder.control('', [Validators.required, Validators.maxLength(10)]),
            descriptionDe: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionFr: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionIt: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionRm: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionEn: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            description: this.formBuilder.control(null, Validators.maxLength(1000)),
            created: this.formBuilder.control({value: '', disabled: true}),
            createdBy: this.formBuilder.control({value: '', disabled: true}),
            modified: this.formBuilder.control({value: '', disabled: true}),
            modifiedBy: this.formBuilder.control({value: '', disabled: true}),
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const collection = this.form.value as GlossaryModification;

        this.saved.emit(collection);
    }
}
