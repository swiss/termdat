import {Pipe, PipeTransform} from '@angular/core';
import {DomSanitizer, SafeHtml} from '@angular/platform-browser';

@Pipe({name: 'hyperlink'})
export class HyperlinkPipe implements PipeTransform {
    private static readonly HYPERLINK_REGEX = /\[([^\]]+)]\(([^)]+)\)/;

    constructor(private readonly sanitizer: DomSanitizer) {}

    transform(sourceText: string): SafeHtml {
        let matchedLink: RegExpExecArray | null;

        while ((matchedLink = HyperlinkPipe.HYPERLINK_REGEX.exec(sourceText))) {
            const htmlLink = `<a href="${matchedLink[2]}" target="_blank" rel="noopener noreferrer">${matchedLink[1]}</a>`;
            sourceText = sourceText.replace(matchedLink[0].toString(), htmlLink);
        }

        return this.sanitizer.bypassSecurityTrustHtml(sourceText);
    }
}
