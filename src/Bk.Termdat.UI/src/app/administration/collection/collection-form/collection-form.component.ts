import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {MatSlideToggle} from '@angular/material/slide-toggle';
import {RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {ClassificationSummary} from 'api/ClassificationSummary';
import {CollectionModification} from 'api/CollectionModification';
import {merge, of, Subject} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {formatDateTime} from '../../../shared/date-util';
import {ClassificationService} from '../classification.service';

@Component({
    selector: 'td-collection-form',
    templateUrl: './collection-form.component.html',
    styleUrls: ['./collection-form.component.scss'],
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
        MatSelect,
        MatOption,
        CdkTextareaAutosize,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class CollectionFormComponent implements OnInit, OnDestroy {
    form: UntypedFormGroup;
    classifications: ClassificationSummary[] = [];

    @Input() titleTranslateKey!: string;
    @Input() set collection(value: CollectionModification) {
        this.form.reset({
            ...value,
            conceptSystem: value.conceptSystem ? value.conceptSystem.title : '-',
            created: value.created ? formatDateTime(new Date(value.created)) : '',
            modified: value.modified ? formatDateTime(new Date(value.modified)) : '',
        });
    }
    @Output() readonly saved = new EventEmitter<CollectionModification>();

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly translateService: TranslateService,
        private readonly classificationService: ClassificationService
    ) {
        this.form = this.formBuilder.group({
            id: this.formBuilder.control({value: '', disabled: true}),
            active: this.formBuilder.control(true),
            code: this.formBuilder.control('', [Validators.required, Validators.maxLength(10)]),
            classificationId: this.formBuilder.control(null),
            activeApi: this.formBuilder.control(false),
            exportLindas: this.formBuilder.control(false),
            descriptionDe: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionFr: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionIt: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionRm: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            descriptionEn: this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]),
            conceptSystem: this.formBuilder.control({value: null, disabled: true}),
            documentation: this.formBuilder.control(null, Validators.maxLength(1000)),
            description: this.formBuilder.control(null, Validators.maxLength(1000)),
            created: this.formBuilder.control({value: '', disabled: true}),
            createdBy: this.formBuilder.control({value: '', disabled: true}),
            modified: this.formBuilder.control({value: '', disabled: true}),
            modifiedBy: this.formBuilder.control({value: '', disabled: true}),
        });
    }

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.classificationService.getClassifications()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(classifications => {
                this.classifications = classifications;
            });
        this.form
            .get('activeApi')!
            .valueChanges.pipe(takeUntil(this.unsubscribe))
            .subscribe(val => {
                if (val) {
                    this.form.controls.classificationId.setValidators([Validators.required]);
                } else {
                    this.form.controls.classificationId.clearValidators();
                    this.form.controls.classificationId.reset();
                }
                this.form.controls.classificationId.updateValueAndValidity();
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

        const collection = this.form.value as CollectionModification;

        this.saved.emit({...collection, conceptSystem: undefined});
    }
}
