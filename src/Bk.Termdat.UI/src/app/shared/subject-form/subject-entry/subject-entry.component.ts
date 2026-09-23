import {Component, EventEmitter, Inject, Input, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormGroup} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatSelect, MatSelectChange} from '@angular/material/select';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {DomainSummary} from 'api/DomainSummary';
import {SubjectSummary} from 'api/SubjectSummary';
import {Chip} from '../../chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../error.service';
import {WINDOW} from '../../injection.tokens';

@Component({
    selector: 'td-subject-entry',
    templateUrl: './subject-entry.component.html',
    styleUrls: ['./subject-entry.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatSelect,
        MatOption,
        MatError,
        ChiplistAutocompleteComponent,
        MatIconButton,
        ObButtonDirective,
        MatTooltip,
        MatIcon,
        TranslatePipe,
    ],
})
export class SubjectEntryComponent implements OnInit {
    domains: DomainSummary[] = [];
    domainChips: Chip[] = [];
    @Input() subjects: SubjectSummary[] = [];
    @Input() entrySubject: SubjectSummary | undefined;
    @Input() formGroup!: UntypedFormGroup;

    @Output() readonly subjectRemoved = new EventEmitter<void>();

    constructor(
        public readonly errorService: ErrorService,
        @Inject(WINDOW) private readonly window: Window
    ) {}

    ngOnInit(): void {
        if (this.entrySubject) {
            this.setupForm(this.entrySubject.id);
        }
        this.window.setTimeout(() => {
            this.formGroup.controls.domains.setValue(this.formGroup.controls.domains.value);
        }, 25);
    }

    selectionChanged(event: MatSelectChange) {
        this.formGroup.controls.domains.reset();

        this.setupForm(event.value);
    }

    remove(): void {
        this.subjectRemoved.emit();
    }

    private setupForm(subjectId: number): void {
        this.domains = this.subjects.find(s => s.id === subjectId)?.domains.sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')) ?? [];
        this.domainChips = this.domains.map(d => new Chip(d.name, d.name, d.id));
    }
}
