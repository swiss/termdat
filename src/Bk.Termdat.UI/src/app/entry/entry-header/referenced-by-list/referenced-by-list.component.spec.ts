import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ActivatedRoute, RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {ReferenceType} from 'api/ReferenceType';
import {MockModule} from 'ng-mocks';
import {Subject} from 'rxjs';
import {LanguageService} from '../../../shared/language.service';
import {ReferencedByListComponent} from './referenced-by-list.component';

describe('ReferencedByListComponent', () => {
    let component: ReferencedByListComponent;
    let fixture: ComponentFixture<ReferencedByListComponent>;

    const activatedRouteMock = {
        snapshot: {
            queryParams: {},
        },
    };

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        onLangChange: onLangChangeSubject.asObservable(),
    };

    const languageServiceMock = {
        getCurrentLanguageId: jest.fn(),
    };
    let references: EntryReferenceDetail[] = [];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [RouterModule, MockModule(TranslateModule), ReferencedByListComponent],
            providers: [
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
            ],
        }).compileComponents();

        translateServiceMock.getCurrentLang = jest.fn(() => 'de');
        languageServiceMock.getCurrentLanguageId.mockReturnValue(2);

        references = [
            {
                type: ReferenceType.None,
                entryId: 1,
                languages: [
                    {languageId: 1, text: 'Reference 1 Lang 1'},
                    {languageId: 2, text: 'Reference 1 Lang 2'},
                ],
            },
            {
                type: ReferenceType.Antonym,
                entryId: 2,
                languages: [
                    {languageId: 1, text: 'Reference 2 Lang 1'},
                    {languageId: 2, text: 'Reference 2 Lang 2'},
                ],
            },
            {
                type: ReferenceType.General,
                entryId: 5,
                languages: [
                    {languageId: 2, text: 'Reference 5 Lang 2'},
                    {languageId: 3, text: 'Reference 5 Lang 3'},
                ],
            },
            {
                type: ReferenceType.Specific,
                entryId: 3,
                languages: [
                    {languageId: 1, text: 'Reference 3 Lang 1'},
                    {languageId: 3, text: 'Reference 3 Lang 3'},
                ],
            },
            {
                type: ReferenceType.None,
                entryId: 4,
                languages: [
                    {languageId: 1, text: 'Reference 4 Lang 1'},
                    {languageId: 2, text: 'Reference 4 Lang 2'},
                    {languageId: 3, text: 'Reference 4 Lang 3'},
                ],
            },
            {
                type: ReferenceType.General,
                entryId: 6,
                languages: [{languageId: 2, text: 'Reference 6 Lang 2'}],
            },
        ];

        fixture = TestBed.createComponent(ReferencedByListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update display references', async () => {
        expect(component.displayReferences).toEqual([]);

        component.references = references;
        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.displayReferences).toEqual([
            {
                entryId: 1,
                text: 'Reference 1 Lang 2',
                type: 0,
            },
            {
                entryId: 2,
                text: 'Reference 2 Lang 2',
                type: 3,
            },
            {
                entryId: 4,
                text: 'Reference 4 Lang 2',
                type: 0,
            },
            {
                entryId: 5,
                text: 'Reference 5 Lang 2',
                type: 1,
            },
            {
                entryId: 6,
                text: 'Reference 6 Lang 2',
                type: 1,
            },
        ]);
    });

    it('should update display references when onLangChange is emitted', async () => {
        component.references = references;

        translateServiceMock.getCurrentLang = jest.fn(() => 'fr');
        languageServiceMock.getCurrentLanguageId.mockReturnValue(1);
        onLangChangeSubject.next(true);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.displayReferences).toEqual([
            {
                entryId: 1,
                text: 'Reference 1 Lang 1',
                type: 0,
            },
            {
                entryId: 2,
                text: 'Reference 2 Lang 1',
                type: 3,
            },
            {
                entryId: 3,
                text: 'Reference 3 Lang 1',
                type: 2,
            },
            {
                entryId: 4,
                text: 'Reference 4 Lang 1',
                type: 0,
            },
        ]);
    });

    it('should handle empty or null for references', () => {
        expect(async () => {
            component.references = [];

            fixture.detectChanges();
            await fixture.whenStable();
        }).not.toThrow();

        expect(async () => {
            component.references = null as unknown as EntryReferenceDetail[];

            fixture.detectChanges();
            await fixture.whenStable();
        }).not.toThrow();
    });

    it('should not display duplicate entries', async () => {
        component.references = [references[0], references[0], references[1], references[2], references[1], references[0]];

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.displayReferences).toEqual([
            {
                entryId: 1,
                text: 'Reference 1 Lang 2',
                type: 0,
            },
            {
                entryId: 2,
                text: 'Reference 2 Lang 2',
                type: 3,
            },
            {
                entryId: 5,
                text: 'Reference 5 Lang 2',
                type: 1,
            },
        ]);
    });
});
