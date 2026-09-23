import {NgClass} from '@angular/common';
import {Component, Input, OnChanges} from '@angular/core';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {NoteSubCodeTooltip} from 'api/NoteSubCodeTooltip';
import {NoteToken} from 'api/NoteToken';
import {DisplayMode} from '../../../shared/DisplayMode';
import {HyperlinkPipe} from '../../../shared/hyperlink.pipe';
import {ReferenceListComponent} from '../reference-list/reference-list.component';

@Component({
    selector: 'td-language-detail',
    templateUrl: './language-detail.component.html',
    styleUrls: ['./language-detail.component.scss'],
    imports: [NgClass, ReferenceListComponent, HyperlinkPipe, TranslatePipe, MatTooltipModule],
})
export class LanguageDetailComponent implements OnChanges {
    @Input() languageDetail!: EntryLanguageDetail;
    @Input() displayMode = DisplayMode.Complete;
    noteTokens: NoteToken[] = [];

    get displayComplete() {
        return this.displayMode === DisplayMode.Complete;
    }

    get displayDefinition() {
        return this.displayMode === DisplayMode.Definition || this.displayComplete;
    }

    get displayContext() {
        return this.displayMode === DisplayMode.Context || this.displayComplete;
    }

    get displayContextOrDefinition() {
        return this.displayContext || this.displayDefinition;
    }

    ngOnChanges(): void {
        if (!this.languageDetail?.note) {
            return;
        }
        this.noteTokens = this.buildTokens(this.languageDetail.note, this.languageDetail.noteTooltips ?? []);
    }

    private buildTokens(note: string, tooltips: NoteSubCodeTooltip[]): NoteToken[] {
        const map = new Map(tooltips.map(t => [t.code, t.tooltip]));

        const regex = /\b([A-Z]{3}):/g;

        const result: NoteToken[] = [];
        let lastIndex = 0;
        let match: RegExpExecArray | null;

        while ((match = regex.exec(note)) !== null) {
            const start = match.index;

            if (start > lastIndex) {
                result.push({
                    text: note.slice(lastIndex, start),
                });
            }

            const code = match[1];

            result.push({
                text: `${code}:`,
                tooltip: map.get(code),
                isCode: true,
            });

            lastIndex = regex.lastIndex;
        }

        if (lastIndex < note.length) {
            result.push({
                text: note.slice(lastIndex),
            });
        }

        return result;
    }
}
