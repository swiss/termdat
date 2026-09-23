import {DomSanitizer} from '@angular/platform-browser';
import {HyperlinkPipe} from './hyperlink.pipe';

describe('HyperlinkPipe', () => {
    const domSanitizerMock = {
        bypassSecurityTrustHtml: jest.fn(v => v),
    } as unknown as DomSanitizer;

    it('should create an instance', () => {
        const pipe = new HyperlinkPipe(domSanitizerMock);
        expect(pipe).toBeTruthy();
    });

    it.each([
        ['no link here', 'no link here'],
        ['link alone is ignored: www.test.ch', 'link alone is ignored: www.test.ch'],
        [
            'Link in migrated format ([Linktext, Datum](www.test.ch/test/))',
            'Link in migrated format (<a href="www.test.ch/test/" target="_blank" rel="noopener noreferrer">Linktext, Datum</a>)',
        ],
        [
            'Link without outer round brackets [Linktext, Datum](www.test.ch/test/)',
            'Link without outer round brackets <a href="www.test.ch/test/" target="_blank" rel="noopener noreferrer">Linktext, Datum</a>',
        ],
        [
            '[Just link without text, Datum](www.test.ch/test/)',
            '<a href="www.test.ch/test/" target="_blank" rel="noopener noreferrer">Just link without text, Datum</a>',
        ],
        [
            'Link1 ([Text1, Datum](www.test1.ch/test/)) Link2 ([Text2, Datum](www.test2.ch/test/))',
            'Link1 (<a href="www.test1.ch/test/" target="_blank" rel="noopener noreferrer">Text1, Datum</a>) ' +
                'Link2 (<a href="www.test2.ch/test/" target="_blank" rel="noopener noreferrer">Text2, Datum</a>)',
        ],
        ['incomplete old link is ignored [{text} www.test.ch', 'incomplete old link is ignored [{text} www.test.ch'],
        ['[{}]', '[{}]'],
        ['()', '()'],
        ['', ''],
        ['(Text), ()', '(Text), ()'],
        ['(www.test.ch)', '(www.test.ch)'],
        ['((www.test.ch))', '((www.test.ch))'],
        ['([Text1, Datum])', '([Text1, Datum])'],
        [null as unknown as string, null],
        [undefined as unknown as string, undefined],
    ])('should transform', (link: string, expected: string | null | undefined) => {
        const pipe = new HyperlinkPipe(domSanitizerMock);

        expect(pipe.transform(link)).toEqual(expected);
    });
});
