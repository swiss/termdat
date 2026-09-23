import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {MockComponent, MockDirectives, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../../shared/autocomplete/autocomplete.component';
import {ChiplistAutocompleteComponent} from '../../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../../shared/error.service';
import {UserAccountService} from '../user-account.service';
import {UserAdministrationUpdateComponent} from './user-administration-update.component';

describe('UserAdministrationUpdateComponent', () => {
    let component: UserAdministrationUpdateComponent;
    let fixture: ComponentFixture<UserAdministrationUpdateComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
        instant: jest.fn(),
    };
    const userAccountServiceMock = {
        get: jest.fn(),
        update: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        getCollections: jest.fn(),
        getGlossaries: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
        paramMap: jest.fn(),
    };
    const paramMock = {get: jest.fn()};
    const routeMock = {
        paramMap: of(paramMock),
    };
    const collections = [{id: 1, name: 'Collection1', code: 'COL1'} as CollectionSummary, {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary];
    const glossaries = [
        {id: 1, name: 'Glossary1', code: 'GLO1'},
        {id: 2, name: 'Glossary2', code: 'GLO2'},
    ];
    const userAccountDetailWriter: UserAccountDetail = {
        id: 1,
        email: '',
        firstName: 'Herbert',
        lastName: 'Herrmann',
        hasFullAccess: true,
        isWriter: true,
        lastAction: undefined,
        role: 'Writer',
        glossaries: [{code: 'g1', id: 1, name: 'glossary one'}],
        collections: [{id: 1, code: 'c1', name: 'collection one'} as CollectionSummary],
        hasRole: false,
    };

    const userAccountDetailSuperReader: UserAccountDetail = {
        id: 1,
        email: '',
        firstName: 'Herbert',
        lastName: 'Herrmann',
        hasFullAccess: true,
        isWriter: false,
        lastAction: undefined,
        role: 'SuperReader',
        glossaries: [{code: 'g1', id: 1, name: 'glossary one'}],
        collections: [{id: 1, code: 'c1', name: 'collection one'} as CollectionSummary],
        hasRole: false,
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                MockModule(TranslateModule),
                UserAdministrationUpdateComponent,
                MockComponent(ChiplistAutocompleteComponent),
                MockComponent(AutocompleteComponent),
                MockDirectives(RouterLink, ObUnsavedChangesDirective, CdkTextareaAutosize),
            ],
            providers: [
                {provide: UserAccountService, useValue: userAccountServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        }).compileComponents();
        userAccountServiceMock.get.mockReturnValue(of(userAccountDetailWriter));
        extendedSearchFiltersServiceMock.getCollections.mockReturnValue(of(collections));
        extendedSearchFiltersServiceMock.getGlossaries.mockReturnValue(of(glossaries));
        userAccountServiceMock.update.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);

        fixture = TestBed.createComponent(UserAdministrationUpdateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        jest.clearAllMocks();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update on save', () => {
        component.save();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should handle error on save', () => {
        userAccountServiceMock.update.mockReturnValue(throwError(() => ({})));

        component.save();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('cannot save when form invalid', async () => {
        component.form.controls.collections.patchValue(null);

        component.save();

        expect(notificationServiceMock.success).toHaveBeenCalledTimes(0);
        expect(component.form.invalid).toBe(true);
        expect(component.form.controls.collections.invalid).toBe(true);
    });

    it('should disable field glossaries for non-writer users', async () => {
        userAccountServiceMock.get.mockReturnValue(of(userAccountDetailSuperReader));

        component.ngOnInit();

        expect(component.form.controls.glossaries.disabled).toBe(true);
    });

    it('should enable field glossaries for writer users', async () => {
        userAccountServiceMock.get.mockReturnValue(of(userAccountDetailWriter));

        component.ngOnInit();

        expect(component.form.controls.glossaries.disabled).toBe(false);
    });
});
