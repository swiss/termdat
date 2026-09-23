import {Component, EventEmitter, Input, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatLabel} from '@angular/material/form-field';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {Chip} from '../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../shared/error.service';

@Component({
    selector: 'td-multi-edit-chip',
    templateUrl: './multi-edit-chip.component.html',
    styleUrls: ['./multi-edit-chip.component.scss'],
    imports: [ReactiveFormsModule, ChiplistAutocompleteComponent, MatLabel, MatError, MatButton, ObButtonDirective, TranslatePipe],
})
export class MultiEditChipComponent {
    @Input() titleTranslateKey!: string;
    @Input() typeTranslateKey!: string;
    @Input() chips!: Chip[];

    @Output() readonly addEvent = new EventEmitter<string[]>();
    @Output() readonly setEvent = new EventEmitter<string[]>();
    @Output() readonly removeEvent = new EventEmitter<string[]>();

    form = new UntypedFormGroup({
        multiSelect: new UntypedFormControl(),
    });

    constructor(public readonly errorService: ErrorService) {}

    add() {
        const value: string[] = this.form.controls.multiSelect.value;
        this.addEvent.emit(value);
    }

    set() {
        const value: string[] = this.form.controls.multiSelect.value;
        this.setEvent.emit(value);
    }

    remove() {
        const value: string[] = this.form.controls.multiSelect.value;
        this.removeEvent.emit(value);
    }
}
