import {KeyValuePipe} from '@angular/common';
import {ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatTooltip} from '@angular/material/tooltip';
import {RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObUnsavedChangesDirective} from '@oblique/oblique';
import {ConceptSystemModification} from 'api/ConceptSystemModification';
import {ConceptSystemPositionModification} from 'api/ConceptSystemPositionModification';
import {merge, of, Subject} from 'rxjs';
import {map, switchMap, takeUntil} from 'rxjs/operators';
import {AutocompleteComponent} from '../../../shared/autocomplete/autocomplete.component';
import {Option} from '../../../shared/autocomplete/Option';
import {CollectionService} from '../../collection/collection.service';

@Component({
    selector: 'td-concept-system-form',
    templateUrl: './concept-system-form.component.html',
    styleUrls: ['./concept-system-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
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
        AutocompleteComponent,
        MatIconButton,
        MatTooltip,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
    ],
})
export class ConceptSystemFormComponent implements OnInit, OnDestroy {
    @Input() titleTranslateKey!: string;
    @Input() set conceptSystem(value: ConceptSystemModification) {
        let positions = value.positions ?? [];
        if (positions.length < 1) {
            positions = [{value: ''}];
        }

        this.form.reset({
            id: value.id,
            title: value.title,
            collectionId: value.collectionId,
            positions: [],
        });

        this.positions.clear();
        positions.forEach(item => this.positions.push(this.createPositionForm(item)));
    }
    @Output() readonly saved = new EventEmitter<ConceptSystemModification>();

    collectionOptions: Option[] = [];

    form: UntypedFormGroup;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly translateService: TranslateService,
        private readonly collectionService: CollectionService,
        private readonly changeDtc: ChangeDetectorRef
    ) {
        this.form = this.formBuilder.group({
            id: {value: undefined, disabled: true},
            title: this.formBuilder.control('', Validators.maxLength(1000)),
            collectionId: null,
            positions: this.formBuilder.array([]),
        });
    }

    ngOnInit(): void {
        if (!this.positions.length) {
            this.positions.push(this.createPositionForm({value: ''}));
        }

        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.collectionService.getCollectionDetails()),
                map(collections => collections.map(collection => ({...collection, disabled: !!collection.conceptSystem}))),
                takeUntil(this.unsubscribe)
            )
            .subscribe(options => {
                const collectionId = this.form.controls.collectionId.value;
                this.collectionOptions = options;
                this.form.controls.collectionId.setValue(collectionId);
                this.changeDtc.detectChanges();
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    get positions(): UntypedFormArray {
        return this.form.get('positions') as UntypedFormArray;
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        const values = this.form.getRawValue();
        const conceptSystem: ConceptSystemModification = {
            id: values.id,
            title: values.title,
            collectionId: values.collectionId,
            positions: (values.positions as ConceptSystemPositionModification[]).filter(item => item.value.length),
        };

        this.saved.emit(conceptSystem);
    }

    addPosition(index: number): void {
        this.positions.insert(index + 1, this.createPositionForm({id: undefined, value: ''}));
    }

    removePosition(index: number): void {
        this.positions.removeAt(index);
        if (!this.positions.length) {
            // We always need an initial position the user can start editing
            this.positions.push(this.createPositionForm({value: ''}));
        }
    }

    private createPositionForm(position: ConceptSystemPositionModification) {
        return this.formBuilder.group({
            id: this.formBuilder.control(position.id),
            value: this.formBuilder.control(position.value, Validators.maxLength(1000)),
        });
    }
}
