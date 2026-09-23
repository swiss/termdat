import {Location} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors} from '@angular/forms';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {BrowserModule} from '@angular/platform-browser';
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {LanguageSummary} from 'api/LanguageSummary';
import {MockComponent, MockDirectives, MockModule} from 'ng-mocks';
import {BehaviorSubject, of, Subject, throwError} from 'rxjs';
import {MultiEditAction} from '../../../api/MultiEditAction';
import {EntryService} from '../../entry/entry.service';
import {ErrorService} from '../../shared/error.service';
import {LanguageService} from '../../shared/language.service';
import {SelectAllComponent} from '../../shared/select-all/select-all.component';
import {WatchlistService} from '../watchlist.service';
import {MultiEditSourceComponent} from './multi-edit-source.component';

describe('MultiEditSourceComponent', () => {
    let component: MultiEditSourceComponent;
    let fixture: ComponentFixture<MultiEditSourceComponent>;

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    const locationMock = {
        getState: jest.fn(),
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
        updateMultipleSource: jest.fn(),
    };

    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
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
                MockModule(MatCheckboxModule),
                MockComponent(SelectAllComponent),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
                MultiEditSourceComponent,
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: Location, useValue: locationMock},
                {provide: Router, useValue: routerMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(MultiEditSourceComponent);
        component = fixture.componentInstance;
        locationMock.getState.mockReturnValue({ids: [1, 2, 3, 4, 5]});
        fixture.detectChanges();
        translateServiceMock.instant.mockImplementation(value => value);
        entryServiceMock.updateMultipleSource.mockReturnValue(of({notFullyUpdatedEntryIds: [], dataWasChanged: true, lockInfos: []}));
        errorServiceMock.getControlError.mockImplementation((errors: ValidationErrors | null, label: string, labelPrefix: string) => {
            if (errors === null) {
                return '';
            }

            const errorKeys = Object.keys(errors);
            return `${labelPrefix}.${errorKeys[0]}`;
        });
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(component.languageOptionDefaultValues.length).toBe(5);
    });

    it('should not update when form is invalid', async () => {
        component.form.controls.source.setValue('source');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([]);

        component.add();

        expect(entryServiceMock.updateMultipleSource).not.toHaveBeenCalled();
    });

    it('should update an entry and show success message', async () => {
        locationMock.getState.mockReturnValue({ids: [1, 2, 3, 4, 5]});
        component.form.controls.source.setValue('source');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([1, 2]);

        component.add();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleSource).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleSource).toHaveBeenCalledWith({
            ids: [1, 2, 3, 4, 5],
            updateValue: 'source',
            targetFields: ['noteSource'],
            languageIds: [1, 2],
            action: 0,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
    });

    it('should show an error when error is thrown', async () => {
        component.form.controls.source.setValue('a');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([1, 2]);

        entryServiceMock.updateMultipleSource.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.add();

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should show "locked entries" warning', async () => {
        entryServiceMock.updateMultipleSource.mockReturnValue(
            of({
                dataWasChanged: true,
                lockInfos: [
                    {lockedEntryId: 1, lockedBy: 'user1'},
                    {lockedEntryId: 2, lockedBy: 'user2'},
                ],
                nonAffectedEntryIds: [],
            })
        );

        component.form.controls.source.setValue('newSource');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([1, 2]);
        component.callUpdate(MultiEditAction.Set);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleSource).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.displayUpdateInfoWarning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
    });

    it('should show "not fully updated" warning', async () => {
        entryServiceMock.updateMultipleSource.mockReturnValue(
            of({
                dataWasChanged: true,
                lockInfos: [],
                notFullyUpdatedEntryIds: [1, 2, 3],
            })
        );

        component.form.controls.source.setValue('newSource');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([1, 2]);
        component.callUpdate(MultiEditAction.Set);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleSource).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith('watchlist.multiEditSource.notFullyUpdatedEntries');
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
    });

    it('should show "no changes" warning', async () => {
        entryServiceMock.updateMultipleSource.mockReturnValue(
            of({
                dataWasChanged: false,
                lockInfos: [],
                notFullyUpdatedEntryIds: [],
            })
        );

        component.form.controls.source.setValue('newSource');
        (component.form.controls.sources as FormGroup).controls.noteSource.setValue(true);
        component.form.controls.language.setValue([1, 2]);
        component.callUpdate(MultiEditAction.Set);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleSource).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith('entry.notification.noChanges');
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(0);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(0);
    });
});
