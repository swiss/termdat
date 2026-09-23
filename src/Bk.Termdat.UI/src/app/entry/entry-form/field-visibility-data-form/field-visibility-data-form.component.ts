import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatIcon} from '@angular/material/icon';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {FieldVisibility} from './FieldVisibility';

const FIELD_VISIBILITY: FieldVisibility = {
    name: false,
    terminus: false,
    abbreviation: false,
    phraseology: false,
    definition: false,
    note: false,
    context: false,
    metadata: false,
    country: false,
};
const FIELD_VISIBILITY_PROPERTIES = Object.keys(FIELD_VISIBILITY) as (keyof FieldVisibility)[];

@Component({
    selector: 'td-field-visibility-data-form',
    templateUrl: './field-visibility-data-form.component.html',
    styleUrls: ['./field-visibility-data-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatButton, ObButtonDirective, MatMenuTrigger, MatTooltip, MatIcon, MatMenu, MatMenuItem, MatCheckbox, TranslatePipe],
})
export class FieldVisibilityDataFormComponent {
    @Output() readonly visibilityChanged = new EventEmitter<FieldVisibility>();

    fieldNames: (keyof FieldVisibility)[] = FIELD_VISIBILITY_PROPERTIES;
    showAll = false;

    @Input() set enableDefinition({enabled: isEnabled}: {enabled: boolean}) {
        if (isEnabled) {
            this.fieldNames = FIELD_VISIBILITY_PROPERTIES;
        } else {
            this.fieldNames = FIELD_VISIBILITY_PROPERTIES.filter(val => val !== 'definition');
            this._fieldVisibility.definition = true;
        }
        this.visibilityChanged.emit(this._fieldVisibility);
    }

    private _fieldVisibility = {} as FieldVisibility;
    @Input() set fieldVisibility(value: FieldVisibility) {
        this._fieldVisibility = value;
        this.showAll = this.calculateShowAll();
    }
    get fieldVisibility() {
        return this._fieldVisibility;
    }

    selectAll(checked: boolean) {
        this.showAll = checked;
        this._fieldVisibility.terminus = true;
        this._fieldVisibility.name = true;
        this._fieldVisibility.abbreviation = checked;
        this._fieldVisibility.phraseology = checked;
        this._fieldVisibility.definition = checked;
        this._fieldVisibility.note = checked;
        this._fieldVisibility.context = checked;
        this._fieldVisibility.metadata = checked;
        this._fieldVisibility.country = checked;

        this.visibilityChanged.emit(this._fieldVisibility);
    }

    show(name: string, show: boolean) {
        this._fieldVisibility[name as keyof FieldVisibility] = show;
        this.showAll = this.calculateShowAll();

        this.visibilityChanged.emit(this._fieldVisibility);
    }

    menuSelectAll($event: Event, show: boolean) {
        $event.stopPropagation();
        this.selectAll(show);
    }

    menuShow($event: Event, fieldName: keyof FieldVisibility, show: boolean) {
        $event.stopPropagation();
        this.show(fieldName, show);
    }

    private calculateShowAll(): boolean {
        return this.fieldNames.every(key => this._fieldVisibility?.[key]);
    }
}
