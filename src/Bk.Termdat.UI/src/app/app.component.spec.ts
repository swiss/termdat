import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {DateAdapter} from '@angular/material/core';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObDocumentMetaService, ObMasterLayoutConfig, ObMasterLayoutModule, ObNotificationService} from '@oblique/oblique';
import {LanguageSummary} from 'api/LanguageSummary';
import {MockModule, MockPipe, MockService} from 'ng-mocks';
import {BehaviorSubject} from 'rxjs';
import {first} from 'rxjs/operators';
import {AppComponent} from './app.component';
import {AuthService} from './auth/auth.service';
import {Role} from './auth/Role';
import {UserInfo} from './auth/UserInfo';
import {ConfigsService} from './config.service';
import {InformationService} from './information.service';
import {EasterEggService} from './shared/easter-egg-dialog/easter-egg.service';
import {WINDOW} from './shared/injection.tokens';
import {LanguageService} from './shared/language.service';
import {UnsavedChangesService} from './shared/unsaved-changes/unsaved-changes.service';

describe('AppComponent', () => {
    let component: AppComponent;
    let fixture: ComponentFixture<AppComponent>;

    const languageTranslations: LanguageSummary[] = [
        {
            id: 1,
            name: 'Deutsch',
            sortIndex: 1,
        } as LanguageSummary,
        {
            id: 2,
            name: 'Englisch',
            sortIndex: 2,
        } as LanguageSummary,
    ];

    const langChangeSubject = new BehaviorSubject<{lang: string}>({lang: 'de'});
    const translateServiceMock = {
        getCurrentLang: jest.fn(),
        onLangChange: langChangeSubject.asObservable(),
    };

    const languageServiceMock = {
        init: jest.fn(),
        getLanguageName: jest.fn(),
        getLanguageTranslations: jest.fn(),
        isLangActive: jest.fn(),
        changeLang: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    const queryParamsSubject = new BehaviorSubject({});
    const routeMock = {
        queryParams: queryParamsSubject.asObservable(),
    };
    const notificationServiceMock = {
        info: jest.fn(),
    };
    const informationServiceMock = {
        loadApplicationVersion: jest.fn(),
    };
    const windowMock = {
        setTimeout: jest.fn().mockImplementation(fn => fn()),
        open: jest.fn(),
    };
    const userInfo$Subject = new BehaviorSubject<UserInfo>({name: 'fancy name'});
    const checkAuthSubject = new BehaviorSubject(true);
    const isAuthenticatedSubject = new BehaviorSubject(false);
    const rolesSubject = new BehaviorSubject<Role[]>([]);
    const isAdminSubject = new BehaviorSubject(false);
    const isSuperUserSubject = new BehaviorSubject(false);
    const authServiceMock = {
        userInfo$: userInfo$Subject.asObservable(),
        checkAuth: () => checkAuthSubject.asObservable(),
        roles$: rolesSubject.asObservable(),
        isAuthenticated$: isAuthenticatedSubject.asObservable(),
        isAdmin$: isAdminSubject.asObservable(),
        isSuperUser$: isSuperUserSubject.asObservable(),
    };

    const unsavedChangesServiceMock = {
        shouldConfirmUnload: jest.fn(),
    };
    const obMasterLayoutConfigMock = {
        locale: {
            disabled: false,
            locales: [],
        },
        layout: {},
        header: {},
        footer: {},
    };

    const configServiceMock = {
        frontendConfig: {
            analytics: {},
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [AppComponent],
            imports: [
                MockModule(RouterModule),
                MockModule(ObMasterLayoutModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockModule(MatTooltipModule),
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: UnsavedChangesService, useValue: unsavedChangesServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: EasterEggService, useValue: {init: jest.fn()}},
                {provide: AuthService, useValue: authServiceMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: Router, useValue: routerMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObMasterLayoutConfig, useValue: obMasterLayoutConfigMock},
                {provide: ObDocumentMetaService, useValue: MockService(ObDocumentMetaService)},
                {provide: DateAdapter<Date>, useValue: MockService(DateAdapter<Date>)},
                {provide: ConfigsService, useValue: configServiceMock},
                {provide: InformationService, useValue: informationServiceMock},
                {provide: WINDOW, useValue: windowMock},
            ],
        }).compileComponents();

        languageServiceMock.getLanguageName.mockImplementation(
            languageId => languageTranslations.find(languageTranslation => languageTranslation.id === languageId)?.name
        );
        languageServiceMock.getLanguageTranslations.mockReturnValue(languageTranslations);

        fixture = TestBed.createComponent(AppComponent);
        component = fixture.componentInstance;

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create the app', () => {
        expect(component).toBeTruthy();
        expect(languageServiceMock.init).toHaveBeenCalled();
    });

    it('should have userName$ observable', done => {
        component.userName$.pipe(first()).subscribe(name => {
            expect(name).toBe('fancy name');
        });

        userInfo$Subject.next({name: 'no fancy name'});
        component.userName$.pipe(first()).subscribe(name => {
            expect(name).toBe('no fancy name');
        });
        userInfo$Subject.next({});
        component.userName$.pipe(first()).subscribe(name => {
            expect(name).toBe('');
            done();
        });
    });

    it.each([{isAuthenticated: true}, {isAuthenticated: false}])(`should set isAuthenticated`, async ({isAuthenticated}) => {
        isAuthenticatedSubject.next(isAuthenticated);

        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(component.isAuthenticated).toBe(isAuthenticated);
    });

    it('should navigate to unauthorized when authenticated with no roles', async () => {
        routerMock.navigate.mockClear();

        isAuthenticatedSubject.next(true);
        isAdminSubject.next(true);
        rolesSubject.next([]);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(routerMock.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should load admin role navigation', async () => {
        component.navigation = [];
        isAdminSubject.next(true);
        isAuthenticatedSubject.next(true);
        rolesSubject.next([Role.Admin]);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(component.navigation.length).toBe(5);
    });

    it('should load any role navigation', async () => {
        component.navigation = [];
        isAdminSubject.next(false);
        isAuthenticatedSubject.next(true);
        rolesSubject.next([Role.Admin]);
        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();

        expect(component.navigation.length).toBe(1);
    });

    it(`should show logout info when reason=logout`, () => {
        queryParamsSubject.next({reason: 'logout'});
        notificationServiceMock.info.mockReset();
        routerMock.navigate.mockReset();

        component.ngOnInit();

        expect(notificationServiceMock.info).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.info).toHaveBeenCalledWith({message: 'application.loggedOut', timeout: 7000});
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith([], {relativeTo: routeMock, queryParams: {reason: null}, queryParamsHandling: 'merge'});
    });

    it('should change current language', async () => {
        langChangeSubject.next({lang: 'en'});
        await fixture.whenStable();

        expect(component.currentLanguage).toBe('en');
    });

    it('should open help in new tab/window', async () => {
        langChangeSubject.next({lang: 'fr'});
        await fixture.whenStable();

        component.showHelp();

        expect(windowMock.open).toHaveBeenCalledTimes(1);
        expect(windowMock.open).toHaveBeenCalledWith('https://www.bk.admin.ch/fr/termdat-aide', '_blank');
    });

    it('should load application version', async () => {
        component.ngOnInit();
        await fixture.whenStable();

        expect(informationServiceMock.loadApplicationVersion).toHaveBeenCalledTimes(1);
    });
});
