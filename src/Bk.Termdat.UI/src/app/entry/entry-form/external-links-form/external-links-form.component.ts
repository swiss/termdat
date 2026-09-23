import {NgClass} from '@angular/common';
import {Component, Input, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {TranslatePipe} from '@ngx-translate/core';
import {ExternalLinkDetail} from 'api/ExternalLinkDetail';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {ExternalLinkEntryComponent} from './external-link-entry/external-link-entry.component';
import {ExternalLinkMode} from './external-link-entry/ExternalLinkMode';

@Component({
    selector: 'td-external-links-form',
    imports: [NgClass, ReactiveFormsModule, TranslatePipe, ExternalLinkEntryComponent],
    templateUrl: './external-links-form.component.html',
    styleUrl: './external-links-form.component.scss',
})
export class ExternalLinksFormComponent implements OnInit {
    EXTERNAL_LINK_MODE = ExternalLinkMode;

    @Input() formArray!: UntypedFormArray;
    createForm: UntypedFormGroup;

    public _externalLinks: ExternalLinkDetail[] = [];

    @Input() set externalLinks(value: ExternalLinkDetail[]) {
        this._externalLinks = value;
        this.setupForm(value);
    }

    constructor(private readonly formBuilder: UntypedFormBuilder) {
        this.createForm = this.formBuilder.group({
            id: this.formBuilder.control(null),
            type: this.formBuilder.control(ExternalLinkType.None, [Validators.required]),
            url: this.formBuilder.control(null, [Validators.required]),
        });
    }

    ngOnInit(): void {
        this.setupForm(this._externalLinks);
    }

    onRemoved(externalLinkIndex: number) {
        this.formArray.removeAt(externalLinkIndex);
        this._externalLinks.splice(externalLinkIndex, 1);
        this.markForm();
    }

    onCreated(externalLinkDetail: ExternalLinkDetail) {
        this._externalLinks.push(externalLinkDetail);
        this.formArray.push(this.buildLinkGroup(externalLinkDetail));
        this.markForm();
    }

    private buildLinkGroup(externalLink: ExternalLinkDetail): UntypedFormGroup {
        return this.formBuilder.group({
            id: this.formBuilder.control(externalLink.id),
            type: this.formBuilder.control(externalLink.type ?? ExternalLinkType.None, [Validators.required]),
            url: this.formBuilder.control(externalLink.url, [Validators.required]),
        });
    }

    private setupForm(externalLinks?: ExternalLinkDetail[]): void {
        if (!externalLinks?.length || !this.formArray) {
            return;
        }

        this.formArray.clear();
        externalLinks.forEach(link => this.formArray.push(this.buildLinkGroup(link)));
    }

    private markForm(): void {
        this.formArray.markAsDirty();
        this.formArray.markAsTouched();
    }
}
