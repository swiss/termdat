import {Location} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormsModule, ReactiveFormsModule, ValidationErrors} from '@angular/forms';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatRadioModule} from '@angular/material/radio';
import {MatSelectModule} from '@angular/material/select';
import {BrowserModule} from '@angular/platform-browser';
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {LanguageSummary} from 'api/LanguageSummary';
import {MockComponent, MockDirectives, MockModule} from 'ng-mocks';
import {BehaviorSubject, of, Subject, throwError} from 'rxjs';
import {EntryService} from '../../entry/entry.service';
import {StorageKeys} from '../../search/StorageKeys';
import {ErrorService} from '../../shared/error.service';
import {LOCAL_STORAGE} from '../../shared/injection.tokens';
import {LanguageService} from '../../shared/language.service';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../watchlist.service';
import {WatchlistMultipleSearchAndReplaceComponent} from './watchlist-multiple-search-and-replace.component';

describe('WatchlistMultipleSearchAndReplaceComponent', () => {
    let component: WatchlistMultipleSearchAndReplaceComponent;
    let fixture: ComponentFixture<WatchlistMultipleSearchAndReplaceComponent>;

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const errorServiceMock = {
        getControlError: jest.fn(),
    };
    const locationMock = {
        getState: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    const languages: LanguageSummary[] = [
        {id: 1, isoCode: 'DE', name: 'German', sortIndex: 1, isUiLanguage: true},
        {id: 2, isoCode: 'FR', name: 'French', sortIndex: 2, isUiLanguage: true},
        {id: 3, isoCode: 'EN', name: 'English', sortIndex: 3, isUiLanguage: true},
        {id: 4, isoCode: 'IT', name: 'Italian', sortIndex: 4, isUiLanguage: true},
        {id: 5, isoCode: 'RM', name: 'Romansh', sortIndex: 5, isUiLanguage: true},
    ];
    const languageSubject = new BehaviorSubject(languages);
    const languageServiceMock = {
        languageSummaries$: languageSubject.asObservable(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        warning: jest.fn(),
        error: jest.fn(),
    };
    const entryServiceMock = {
        multipleSearchAndReplace: jest.fn(),
    };
    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };
    const localStorageMock = {
        getItem: jest.fn(),
        setItem: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                FormsModule,
                BrowserModule,
                MockModule(TranslateModule),
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatInputModule),
                MockModule(MatOptionModule),
                MockModule(MatSelectModule),
                MockModule(MatRadioModule),
                WatchlistMultipleSearchAndReplaceComponent,
                MockComponent(SelectAllComponent),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: LOCAL_STORAGE, useValue: localStorageMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: Location, useValue: locationMock},
                {provide: Router, useValue: routerMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
            ],
        }).compileComponents();

        locationMock.getState.mockReturnValue({ids: [1, 2, 3, 4, 5]});
        translateServiceMock.instant.mockImplementation(input => input);
        errorServiceMock.getControlError.mockImplementation((errors: ValidationErrors | null, label: string, labelPrefix: string) => {
            if (errors === null) {
                return '';
            }

            const errorKeys = Object.keys(errors);
            return `${labelPrefix}.${errorKeys[0]}`;
        });
        entryServiceMock.multipleSearchAndReplace.mockReturnValue(
            of({
                entryCount: 1,
                affectedOccurences: 1,
                lockInfos: null,
            })
        );
    });

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [WatchlistMultipleSearchAndReplaceComponent],
        }).compileComponents();
    });

    beforeEach(async () => {
        fixture = TestBed.createComponent(WatchlistMultipleSearchAndReplaceComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should not update when form is invalid', async () => {
        component.form.controls.searchValue.setValue('');
        component.form.controls.replaceValue.setValue('b');
        component.form.controls.searchField.setValue('note');
        component.form.controls.language.setValue([1, 2]);

        component.save();

        expect(entryServiceMock.multipleSearchAndReplace).not.toHaveBeenCalled();

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should update an entry and show success message', async () => {
        component.form.controls.searchValue.setValue('a');
        component.form.controls.replaceValue.setValue('b');
        component.form.controls.searchField.setValue('note');
        component.form.controls.language.setValue([1, 2]);

        component.save();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.multipleSearchAndReplace).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.multipleSearchAndReplace).toHaveBeenCalledWith({
            ids: [1, 2, 3, 4, 5],
            searchValue: 'a',
            replaceValue: 'b',
            searchField: 'note',
            languageIds: [1, 2],
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);
    });

    it('should show warning if value was not found', async () => {
        component.form.controls.searchValue.setValue('xyz');
        component.form.controls.replaceValue.setValue('b');
        component.form.controls.searchField.setValue('note');
        component.form.controls.language.setValue([1, 2]);
        entryServiceMock.multipleSearchAndReplace.mockReturnValue(
            of({
                entryCount: 0,
                affectedOccurences: 0,
                lockInfos: null,
            })
        );

        component.save();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.multipleSearchAndReplace).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.multipleSearchAndReplace).toHaveBeenCalledWith({
            ids: [1, 2, 3, 4, 5],
            searchValue: 'xyz',
            replaceValue: 'b',
            searchField: 'note',
            languageIds: [1, 2],
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(0);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(0);
    });

    it('should show an error when error is thrown', async () => {
        component.form.controls.searchValue.setValue('a');
        component.form.controls.replaceValue.setValue('b');
        component.form.controls.searchField.setValue('note');
        component.form.controls.language.setValue([1, 2]);

        entryServiceMock.multipleSearchAndReplace.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.save();

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should save selected languages in local storage', async () => {
        component.form.controls.language.setValue([1, 2]);

        expect(localStorageMock.setItem).toHaveBeenCalledWith(StorageKeys.SELECTED_SEARCH_AND_REPLACE_LANGUAGES_LOCAL_STORAGE_KEY, '[1,2]');
    });

    it('should use selected languages from local storage', async () => {
        localStorageMock.getItem.mockImplementation(x => (x === StorageKeys.SELECTED_SEARCH_AND_REPLACE_LANGUAGES_LOCAL_STORAGE_KEY ? '[1,2]' : ''));

        component.ngOnInit();
        expect(component.languageControl.value).toStrictEqual([1, 2]);
    });

    it('should use default languages', async () => {
        localStorageMock.getItem.mockImplementation(_ => '');

        component.ngOnInit();
        expect(component.form.controls.language.value).toEqual(component.languageOptionDefaultValues);
        expect(component.form.controls.language.value).toEqual(component.languageOptionDefaultValues);
    });
});
