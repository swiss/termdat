import {NgClass} from '@angular/common';
import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {TranslatePipe} from '@ngx-translate/core';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {ReferenceEntryComponent} from './reference-entry/reference-entry.component';
import {ReferenceEntryMode} from './reference-entry/ReferenceEntryMode';

@Component({
    selector: 'td-references-form',
    templateUrl: './references-form.component.html',
    styleUrls: ['./references-form.component.scss'],
    imports: [NgClass, ReferenceEntryComponent, TranslatePipe],
})
export class ReferencesFormComponent implements OnInit {
    REFERENCE_ENTRY_MODE = ReferenceEntryMode;

    @Input() formArray!: UntypedFormArray;
    createForm: UntypedFormGroup;

    public _references: EntryReferenceDetail[] = [];

    @Input() set references(value: EntryReferenceDetail[]) {
        this._references = value;
        this.setupForm(value);
    }

    constructor(private readonly formBuilder: UntypedFormBuilder) {
        this.createForm = this.formBuilder.group({});
    }

    ngOnInit(): void {
        this.setupForm(this._references);
    }

    onRemoved(referenceIndex: number) {
        this.formArray.removeAt(referenceIndex);
        this._references.splice(referenceIndex, 1);
        this.markForm();
    }

    onCreated(reference: EntryReferenceDetail) {
        this._references.push(reference);
        this.formArray.push(this.formBuilder.group({}));
        this.markForm();
    }

    private setupForm(references?: EntryReferenceDetail[]): void {
        if (!references?.length || !this.formArray) {
            return;
        }

        this.formArray.clear();
        references.forEach(() => this.formArray.push(this.formBuilder.group({})));
    }

    private markForm(): void {
        this.formArray.markAsDirty();
        this.formArray.markAsTouched();
    }
}
