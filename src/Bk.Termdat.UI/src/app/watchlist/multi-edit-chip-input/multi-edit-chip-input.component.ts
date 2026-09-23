import {Component, EventEmitter, Input, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatLabel} from '@angular/material/form-field';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {Chip} from '../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';

@Component({
    selector: 'td-multi-edit-chip-input',
    templateUrl: './multi-edit-chip-input.component.html',
    styleUrls: ['./multi-edit-chip-input.component.scss'],
    imports: [ReactiveFormsModule, ChiplistAutocompleteInputComponent, MatLabel, MatError, MatButton, ObButtonDirective, TranslatePipe],
})
export class MultiEditChipInputComponent {
    @Input() titleTranslateKey!: string;
    @Input() typeTranslateKey!: string;
    @Input() chips!: Chip[];

    @Output() readonly addEvent = new EventEmitter<AdminInfoSummary[]>();
    @Output() readonly setEvent = new EventEmitter<AdminInfoSummary[]>();
    @Output() readonly removeEvent = new EventEmitter<AdminInfoSummary[]>();

    form = new UntypedFormGroup({
        multiSelect: new UntypedFormControl(),
    });

    constructor(public readonly errorService: ErrorService) {}

    add() {
        const adminInfos: AdminInfoSummary[] =
            this.form.controls.multiSelect.value.map((info: Chip) => {
                return {
                    id: info.value ?? undefined,
                    code: info.displayText,
                } as AdminInfoSummary;
            }) ?? [];

        this.addEvent.emit(adminInfos);
    }

    set() {
        const adminInfos: AdminInfoSummary[] =
            this.form.controls.multiSelect.value.map((info: Chip) => {
                return {
                    id: info.value ?? undefined,
                    code: info.displayText,
                } as AdminInfoSummary;
            }) ?? [];

        this.setEvent.emit(adminInfos);
    }

    remove() {
        this.removeEvent.emit();
    }
}
