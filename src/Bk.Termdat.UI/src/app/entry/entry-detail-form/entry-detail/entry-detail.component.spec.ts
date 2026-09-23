import {CommonModule} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatCheckbox} from '@angular/material/checkbox';
import {By} from '@angular/platform-browser';
import {TranslatePipe} from '@ngx-translate/core';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryDetail} from 'api/EntryDetail';
import {LanguageSummary} from 'api/LanguageSummary';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject, Subject} from 'rxjs';
import {SearchService} from '../../../search/search.service';
import {LanguageService} from '../../../shared/language.service';
import {LanguageDetailComponent} from '../language-detail/language-detail.component';
import {EntryDetailComponent} from './entry-detail.component';

describe('EntryDetailComponent', () => {
    let component: EntryDetailComponent;
    let fixture: ComponentFixture<EntryDetailComponent>;

    let languages: LanguageSummary[] = [];

    const languageSubject = new BehaviorSubject(languages);
    const languageServiceMock = {
        getLanguageName: jest.fn(),
        languageSummaries$: languageSubject.asObservable(),
    };

    const languageFilterSubject = new Subject();
    const searchServiceMock = {
        languageFilter$: languageFilterSubject.asObservable(),
        getVisibleLanguages: jest.fn(),
    };

    let entryDetail: EntryDetail;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                EntryDetailComponent,
                MockPipe(TranslatePipe),
                MockComponent(MatCheckbox),
                MockComponent(LanguageDetailComponent),
                MockModule(CommonModule),
            ],
            providers: [
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: SearchService, useValue: searchServiceMock},
            ],
        }).compileComponents();

        languages = [
            {id: 1, name: 'Deutsch', sortIndex: 1, isUiLanguage: true, isoCode: 'de'},
            {id: 2, name: 'Englisch', sortIndex: 2, isUiLanguage: true, isoCode: 'en'},
        ];
        searchServiceMock.getVisibleLanguages.mockReturnValue([]);

        languageServiceMock.getLanguageName.mockImplementation(
            languageId => languages.find(languageTranslation => languageTranslation.id === languageId)?.name
        );

        entryDetail = {
            id: 1,
            collection: {
                id: 10,
                code: 'col',
                name: 'colname',
            } as CollectionSummary,
            languageDetails: [
                {
                    abbreviation: 'short',
                    definition: 'def',
                    languageId: 1,
                    name: 'Entry Detail',
                    phraseology: 'phraseology',
                    sequence: 0,
                    terminus: 'term',
                },
                {
                    abbreviation: 'short 2',
                    definition: 'def 2',
                    languageId: 2,
                    name: 'Entry Detail 2',
                    phraseology: 'phraseology 2',
                    sequence: 0,
                    terminus: 'term 2',
                },
            ],
        } as EntryDetail;

        fixture = TestBed.createComponent(EntryDetailComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        component.entry = entryDetail;

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update the details when languages$ emits', () => {
        expect(component.details).toEqual({
            Deutsch: [entryDetail.languageDetails![0]],
            Englisch: [entryDetail.languageDetails![1]],
        });
        expect(component.detailsCount).toEqual(2);

        languages[0].name = 'German';
        languages[1].name = 'English';
        languageSubject.next(languages);

        expect(component.details).toEqual({
            German: [entryDetail.languageDetails![0]],
            English: [entryDetail.languageDetails![1]],
        });
        expect(component.detailsCount).toEqual(2);
    });

    it('sets correct class for obsolete, avoid or rare', async () => {
        expect(fixture.debugElement.query(By.css('.rare, .obsolete, .avoid'))).toBeFalsy();

        component.details[languages[0].name][0].isRare = true;
        fixture.detectChanges();
        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('.rare'))).toBeTruthy();

        component.details[languages[0].name][0].isRare = true;
        component.details[languages[0].name][0].isObsolete = true;
        fixture.detectChanges();
        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('.rare'))).toBeTruthy();
        expect(fixture.debugElement.query(By.css('.obsolete'))).toBeTruthy();

        component.details[languages[0].name][0].isRare = true;
        component.details[languages[0].name][0].isObsolete = true;
        component.details[languages[0].name][0].shouldAvoid = true;
        fixture.detectChanges();
        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('.rare'))).toBeTruthy();
        expect(fixture.debugElement.query(By.css('.obsolete'))).toBeTruthy();
        expect(fixture.debugElement.query(By.css('.avoid'))).toBeTruthy();
    });
});
