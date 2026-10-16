import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'hyperlink'})
export class HyperlinkPipe implements PipeTransform {
    private static readonly HYPERLINK_REGEX = /\[([^\]]+)]\(([^)]+)\)/;

    transform(sourceText: string): string {
        let matchedLink: RegExpExecArray | null;

        while ((matchedLink = HyperlinkPipe.HYPERLINK_REGEX.exec(sourceText))) {
            const htmlLink = `<a href="${matchedLink[2]}" target="_blank" rel="noopener noreferrer">${matchedLink[1]}</a>`;
            sourceText = sourceText.replace(matchedLink[0].toString(), htmlLink);
        }

        return sourceText;
    }
}
