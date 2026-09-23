import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {NgClass} from '@angular/common';
import {ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormGroup} from '@angular/forms';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {ErrorService} from '../../../shared/error.service';
import {FieldVisibilityDataFormComponent} from '../field-visibility-data-form/field-visibility-data-form.component';
import {FieldVisibility} from '../field-visibility-data-form/FieldVisibility';
import {NoteTemplatesComponent} from '../note-templates/note-templates.component';
import {SourceTemplatesComponent} from '../source-templates/source-templates.component';
import {SourceTemplateDefaults} from '../source-templates/SourceTemplateDefaults';
import {SynonymMoveDirection} from './SynonymMoveDirection';

@Component({
    selector: 'td-entry-form-synonym',
    templateUrl: './entry-form-synonym.component.html',
    styleUrls: ['./entry-form-synonym.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ReactiveFormsModule,
        NgClass,
        FieldVisibilityDataFormComponent,
        MatIconButton,
        ObButtonDirective,
        MatTooltip,
        MatIcon,
        MatButton,
        MatError,
        MatFormField,
        MatLabel,
        MatInput,
        CdkTextareaAutosize,
        SourceTemplatesComponent,
        TranslatePipe,
        NoteTemplatesComponent,
    ],
})
export class EntryFormSynonymComponent {
    readonly sortDirection = SynonymMoveDirection;
    fieldVisibility: FieldVisibility = {} as FieldVisibility;

    @Input() language: keyof SourceTemplateDefaults = 'de';
    @Input() title!: string;
    @Input() isFirst = false;
    @Input() isLast = false;

    @Output() readonly synonymSorted = new EventEmitter<SynonymMoveDirection>();
    @Output() readonly synonymDeleted = new EventEmitter<void>();

    public _form!: UntypedFormGroup;
    @Input() set form(value: UntypedFormGroup) {
        this._form = value;
        this.fieldVisibility = EntryFormSynonymComponent.createFieldVisibility(this._form.value as EntryLanguageDetail);
    }

    constructor(
        public readonly errorService: ErrorService,
        private readonly translateService: TranslateService,
        private readonly changeDetectorRef: ChangeDetectorRef
    ) {}

    onVisibilityChanged($event: FieldVisibility): void {
        this.fieldVisibility = $event;
    }

    changeSort(direction: SynonymMoveDirection): void {
        this.changeDetectorRef.detectChanges();
        this.synonymSorted.next(direction);
    }

    delete(): void {
        this.synonymDeleted.next();
    }

    sourceTemplateSelected(template: string, controlName: string) {
        const control = this._form.get(controlName);
        if (!control) {
            return;
        }

        let value = control.value ?? '';

        if (value) {
            value += '; ';
        }
        value += template;

        control.setValue(value);
        control.markAsDirty();
        control.markAsTouched();
    }

    getInvisibleControlsErrors(): string[] {
        const fields = Object.keys(this.fieldVisibility) as (keyof FieldVisibility)[];
        const invisibleFields = fields.filter(field => !this.fieldVisibility[field]);

        const errors = invisibleFields.flatMap(field => this.getEntryErrors(this._form, field));

        return errors.filter((v, i, a) => a.indexOf(v) === i);
    }

    private static createFieldVisibility(entry: EntryLanguageDetail): FieldVisibility {
        const visibility: FieldVisibility = {
            terminus: !!(entry.terminus || entry.terminusSource),
            name: !!(entry.name || entry.nameSource),
            abbreviation: !!(entry.abbreviation || entry.abbreviationSource),
            phraseology: !!(entry.phraseology || entry.phraseologySource),
            definition: !!(entry.definition || entry.definitionSource),
            note: !!(entry.note || entry.noteSource),
            context: !!(entry.context || entry.contextSource),
            metadata: !!(entry.metadata || entry.metadataSource),
            country: !!entry.country,
        };

        const hasVisibleMandatoryField = visibility.terminus || visibility.name || visibility.abbreviation || visibility.phraseology;
        if (!hasVisibleMandatoryField) {
            visibility.terminus = true;
            visibility.name = true;
        }

        return visibility;
    }

    private getEntryErrors(form: UntypedFormGroup, fieldName: string): string[] {
        const errors = [] as string[];
        const fieldControl = form.get(fieldName);
        const sourceFieldControl = form.get(`${fieldName}Source`);

        if (!fieldControl) {
            return errors;
        }

        if (fieldControl.errors && fieldControl.touched) {
            errors.push(this.errorService.getControlError(fieldControl.errors, this.translateService.instant(`field.${fieldName}`), 'entry.error'));
        }

        if (sourceFieldControl?.errors && sourceFieldControl?.touched) {
            errors.push(this.errorService.getControlError(sourceFieldControl.errors, this.translateService.instant(`field.${fieldName}`), 'entry.error'));
        }

        return errors;
    }
}
