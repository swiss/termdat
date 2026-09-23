import {Component, Input} from '@angular/core';
import {MatTooltip} from '@angular/material/tooltip';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {EntryReferenceSummary} from 'api/EntryReferenceSummary';
import {ReferenceType} from 'api/ReferenceType';
import {RenderReference} from './RenderReference';

@Component({
    selector: 'td-reference-list',
    templateUrl: './reference-list.component.html',
    styleUrls: ['./reference-list.component.scss'],
    imports: [MatTooltip, RouterLink, TranslatePipe],
})
export class ReferenceListComponent {
    public displayReferences: RenderReference[] = [];
    public REFERENCE_TYPE = ReferenceType;

    get references(): EntryReferenceSummary[] {
        return this._references;
    }
    @Input() set references(value) {
        this._references = value;
        this.displayReferences = this.createDisplayReferences(this._references);
    }

    private readonly SAME_TYPE_SEPARATOR = ',';
    private readonly DIFFERENT_TYPE_SEPARATOR = ';';

    private _references: EntryReferenceSummary[] = [];

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly translateService: TranslateService
    ) {}

    private createDisplayReferences(references: EntryReferenceSummary[]): RenderReference[] {
        if (!references?.length) {
            return [];
        }

        const mappedReferences: RenderReference[] = [];
        for (let i = 0; i < references.length; i++) {
            const reference = references[i];
            const next = i + 1 < references.length ? references[i + 1] : undefined;
            const prev = i - 1 >= 0 ? references[i - 1] : undefined;

            const mapped = this.createRenderReference(reference, prev, next);

            mappedReferences.push(mapped);
        }

        return mappedReferences;
    }

    private createRenderReference(
        reference: EntryReferenceSummary,
        previousReference?: EntryReferenceSummary,
        nextReference?: EntryReferenceSummary
    ): RenderReference {
        const mapped = {
            entryId: reference.entryId,
            text: reference.text,
        } as RenderReference;

        if (!previousReference || previousReference?.type !== reference.type) {
            mapped.type = reference.type;
        } else {
            mapped.type = ReferenceType.None;
        }

        if (nextReference) {
            mapped.separator = nextReference.type === reference.type ? this.SAME_TYPE_SEPARATOR : this.DIFFERENT_TYPE_SEPARATOR;
        }

        return mapped;
    }
}
