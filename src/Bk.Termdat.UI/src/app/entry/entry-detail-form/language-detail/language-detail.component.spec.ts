import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslatePipe} from '@ngx-translate/core';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {NoteSubCodeTooltip} from 'api/NoteSubCodeTooltip';
import {NoteToken} from 'api/NoteToken';
import {MockComponent, MockPipe} from 'ng-mocks';
import {DisplayMode} from '../../../shared/DisplayMode';
import {HyperlinkPipe} from '../../../shared/hyperlink.pipe';
import {ReferenceListComponent} from '../reference-list/reference-list.component';
import {LanguageDetailComponent} from './language-detail.component';

describe('LanguageDetailComponent', () => {
    let component: LanguageDetailComponent;
    let fixture: ComponentFixture<LanguageDetailComponent>;

    const langDetMock: EntryLanguageDetail = {
        id: 234,
        languageId: 123,
        isObsolete: false,
        isRare: false,
        sequence: 1,
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [LanguageDetailComponent, MockPipe(TranslatePipe), MockPipe(HyperlinkPipe), MockComponent(ReferenceListComponent)],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(LanguageDetailComponent);
        component = fixture.componentInstance;
        component.languageDetail = langDetMock;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set DisplayMode.Complete', () => {
        component.displayMode = DisplayMode.Complete;
        expect(component.displayComplete).toBeTruthy();
        expect(component.displayDefinition).toBeTruthy();
        expect(component.displayContext).toBeTruthy();
        expect(component.displayContextOrDefinition).toBeTruthy();
    });

    it('should set DisplayMode.Definition', () => {
        component.displayMode = DisplayMode.Definition;
        expect(component.displayComplete).toBeFalsy();
        expect(component.displayDefinition).toBeTruthy();
        expect(component.displayContext).toBeFalsy();
        expect(component.displayContextOrDefinition).toBeTruthy();
    });

    it('should set DisplayMode.Context', () => {
        component.displayMode = DisplayMode.Context;
        expect(component.displayComplete).toBeFalsy();
        expect(component.displayDefinition).toBeFalsy();
        expect(component.displayContext).toBeTruthy();
        expect(component.displayContextOrDefinition).toBeTruthy();
    });

    it('should set DisplayMode.Minimal', () => {
        component.displayMode = DisplayMode.Minimal;
        expect(component.displayComplete).toBeFalsy();
        expect(component.displayDefinition).toBeFalsy();
        expect(component.displayContext).toBeFalsy();
        expect(component.displayContextOrDefinition).toBeFalsy();
    });

    describe('Tokenize Note field', () => {
        it('should return plain text when no code is present', () => {
            component.languageDetail = {
                note: 'hello world',
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(component.noteTokens).toEqual([{text: 'hello world'}]);
        });

        it('should tokenize a single code with tooltip', () => {
            component.languageDetail = {
                note: 'ABC: apple',
                noteTooltips: [{code: 'ABC', tooltip: 'Apple tooltip'}],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(component.noteTokens).toEqual([{text: 'ABC:', tooltip: 'Apple tooltip', isCode: true}, {text: ' apple'}]);
        });

        it('should handle multiple codes in a single string', () => {
            component.languageDetail = {
                note: 'ABC: apple; DEF: pear',
                noteTooltips: [
                    {code: 'ABC', tooltip: 'Apple'},
                    {code: 'DEF', tooltip: 'Pear'},
                ],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            const codeTokens = component.noteTokens.filter(t => t.isCode);

            expect(codeTokens.length).toBe(2);
            expect(codeTokens[0]).toEqual({
                text: 'ABC:',
                tooltip: 'Apple',
                isCode: true,
            });
            expect(codeTokens[1]).toEqual({
                text: 'DEF:',
                tooltip: 'Pear',
                isCode: true,
            });
        });

        it('should handle adjacent codes without ;', () => {
            component.languageDetail = {
                note: 'ABC:apple DEF:pear',
                noteTooltips: [
                    {code: 'ABC', tooltip: 'Apple'},
                    {code: 'DEF', tooltip: 'Pear'},
                ],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            const codeTokens = component.noteTokens.filter(t => t.isCode);

            expect(codeTokens.length).toBe(2);
        });

        it('should not lose any text (lossless tokenization)', () => {
            const input = 'ABC: apple; DEF: pear; random text here';

            component.languageDetail = {
                note: input,
                noteTooltips: [
                    {code: 'ABC', tooltip: 'Apple'},
                    {code: 'DEF', tooltip: 'Pear'},
                ],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(joinTokens(component.noteTokens)).toBe(input);
        });

        it('should preserve full text when no codes are present', () => {
            const input = 'plain text without codes';

            component.languageDetail = {
                note: input,
                noteTooltips: [] as NoteSubCodeTooltip[],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(joinTokens(component.noteTokens)).toBe(input);
        });

        it('should preserve text when tooltip is missing', () => {
            const input = 'ABC: apple text';

            component.languageDetail = {
                note: input,
                noteTooltips: [] as NoteSubCodeTooltip[],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(joinTokens(component.noteTokens)).toBe(input);
        });

        it('should not break on invalid inline codes', () => {
            const input = 'APFEL: text BIRNE: value; ABC: apple';

            component.languageDetail = {
                note: input,
                noteTooltips: [{code: 'ABC', tooltip: 'Apple'}],
            } as EntryLanguageDetail;

            component.ngOnChanges();

            expect(joinTokens(component.noteTokens)).toBe(input);
        });
    });

    function joinTokens(tokens: NoteToken[]): string {
        return tokens.map(t => t.text).join('');
    }
});
