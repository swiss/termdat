import {AsyncPipe} from '@angular/common';
import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {MatAnchor, MatIconButton} from '@angular/material/button';
import {DateAdapter} from '@angular/material/core';
import {MatIcon} from '@angular/material/icon';
import {MatTooltip} from '@angular/material/tooltip';
import {ActivatedRoute, Router, RouterLink, RouterOutlet} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {
    ObButtonDirective,
    ObDocumentMetaService,
    ObExternalLinkDirective,
    ObHttpApiInterceptorConfig,
    ObHttpApiInterceptorEvents,
    ObINavigationLink,
    ObMasterLayoutConfig,
    ObMasterLayoutModule,
    ObNotificationConfig,
    ObNotificationService,
} from '@oblique/oblique';
import {Observable, Subject, zip} from 'rxjs';
import {map, takeUntil} from 'rxjs/operators';
import {AuthService} from './auth/auth.service';
import {ConfigsService, FrontendConfig} from './config.service';
import {InformationService} from './information.service';
import {EasterEggService} from './shared/easter-egg-dialog/easter-egg.service';
import {WINDOW} from './shared/injection.tokens';
import {LanguageService} from './shared/language.service';
import {UnsavedChangesService} from './shared/unsaved-changes/unsaved-changes.service';

interface I18nLinks {
    [key: string]: string;
}

@Component({
    selector: 'td-app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    imports: [
        MatTooltip,
        ObMasterLayoutModule,
        TranslatePipe,
        AsyncPipe,
        RouterLink,
        MatIcon,
        ObExternalLinkDirective,
        RouterOutlet,
        ObButtonDirective,
        MatIconButton,
        MatAnchor,
    ],
})
export class AppComponent implements OnInit, OnDestroy {
    navigation: ObINavigationLink[] = [
        {
            url: 'search',
            label: 'navigation.search',
            id: 'search',
            isExternal: false,
        },
    ];
    currentLanguage = 'de';
    currentYear = new Date().getFullYear();

    isAuthenticated = false;
    userName$: Observable<string>;

    readonly helpLinks: I18nLinks = {
        de: 'https://www.bk.admin.ch/de/termdat-hilfe',
        fr: 'https://www.bk.admin.ch/fr/termdat-aide',
        it: 'https://www.bk.admin.ch/it/usare-termdat',
        rm: 'https://www.bk.admin.ch/rm/termdat-agid',
        en: 'https://www.bk.admin.ch/en/termdat-help',
    };
    readonly legalFrameWorkLinks: I18nLinks = {
        de: 'https://www.admin.ch/de/rechtliches',
        fr: 'https://www.admin.ch/fr/conditions-utilisation',
        it: 'https://www.admin.ch/it/basi-legali',
        rm: 'https://www.admin.ch/rm/infurmaziuns-giuridicas',
        en: 'https://www.admin.ch/en/terms-and-conditions',
    };
    readonly impressumLinks: I18nLinks = {
        de: 'https://www.bk.admin.ch/impressum',
        fr: 'https://www.bk.admin.ch/fr/impressum-2',
        it: 'https://www.bk.admin.ch/it/impressum-3',
        rm: 'https://www.bk.admin.ch/rm/impressum-4',
        en: 'https://www.bk.admin.ch/en/impressum-5',
    };
    readonly accessibilityLinks: I18nLinks = {
        de: 'https://www.bk.admin.ch/de/barrierefreiheit',
        fr: 'https://www.bk.admin.ch/fr/accessibilite',
        it: 'https://www.bk.admin.ch/it/accessibilita',
        rm: 'https://www.bk.admin.ch/rm/access-senza-barrieras',
        en: 'https://www.bk.admin.ch/en/accessibility',
    };
    readonly aboutLinks: I18nLinks = {
        de: 'https://www.bk.admin.ch/de/termdat-die-terminologie-datenbank-der-bundesverwaltung',
        fr: 'https://www.bk.admin.ch/fr/termdat-banque-de-donnees-terminologiques-de-ladministration-federale',
        it: 'https://www.bk.admin.ch/it/termdat-la-banca-dati-terminologica-dellamministrazione-federale',
        rm: 'https://www.bk.admin.ch/rm/termdat-la-banca-da-datas-terminologica-da-ladministraziun-federala',
        en: 'https://www.bk.admin.ch/en/termdat-the-federal-administrations-terminology-database',
    };

    private readonly authenticatedNavigationItems: ObINavigationLink[] = [
        {
            url: 'watchlist',
            label: 'navigation.watchlist',
            id: 'watchlist',
            isExternal: false,
        },
    ];

    private readonly adminNavigationItems: ObINavigationLink[] = [
        {
            url: 'administration',
            label: 'navigation.administration',
            id: 'administration',
            isExternal: false,
            children: [
                {url: 'mailbox', label: 'navigation.administration.mailbox'},
                {url: 'termdatClassification', label: 'navigation.administration.termdatClassifications'},
                {url: 'lenochClassification', label: 'navigation.administration.lenochClassifications'},
                {url: 'collection', label: 'navigation.administration.collections'},
                {url: 'glossary', label: 'navigation.administration.glossaries'},
                {url: 'conceptSystem', label: 'navigation.administration.conceptSystems'},
                {url: 'office', label: 'navigation.administration.offices'},
                {url: 'adminInfo', label: 'navigation.administration.adminInfos'},
                {url: 'language', label: 'navigation.administration.languages'},
                {url: 'userAdministration', label: 'navigation.administration.userAdministration'},
                {url: 'news', label: 'navigation.administration.news'},
            ],
        },
    ];

    private readonly statisticNavigationItems: ObINavigationLink[] = [
        {
            url: 'statistic',
            label: 'navigation.statistic',
            id: 'statistic',
            isExternal: false,
            children: [{url: 'termdat', label: 'navigation.statistic.termdat'}],
        },
    ];

    private readonly importNavigationItems: ObINavigationLink[] = [
        {
            url: 'import',
            label: 'navigation.import',
            id: 'import',
            isExternal: false,
            children: [{url: 'fileimport', label: 'navigation.import.fileimport'}],
        },
    ];

    private readonly duplicateCheckNavigationItems: ObINavigationLink[] = [
        {
            url: 'duplicate-check',
            label: 'navigation.duplicateCheck',
            id: 'duplicateCheck',
            isExternal: false,
        },
    ];

    private readonly frontendConfig: FrontendConfig;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly unsavedChangesService: UnsavedChangesService,
        private readonly languageService: LanguageService,
        private readonly translateService: TranslateService,
        private readonly easterEggService: EasterEggService,
        private readonly authService: AuthService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly notificationService: ObNotificationService,
        private readonly informationService: InformationService,
        @Inject(WINDOW) private readonly window: Window,
        documentMetaService: ObDocumentMetaService,
        masterLayoutConfig: ObMasterLayoutConfig,
        notificationConfig: ObNotificationConfig,
        httpApiInterceptorConfig: ObHttpApiInterceptorConfig,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents,
        dateAdapter: DateAdapter<Date>,
        readonly configService: ConfigsService
    ) {
        dateAdapter.setLocale(`${translateService.getCurrentLang()}`);

        translateService.onLangChange.subscribe(data => {
            dateAdapter.setLocale(`${data.lang}`);
        });

        httpApiInterceptorEvents.sessionExpired.subscribe(async () => {
            if (this.isAuthenticated) {
                await router.navigate(['/unauthorized']);
            }
        });

        documentMetaService.titleSuffix = 'application.title';
        documentMetaService.description = 'application.description';

        masterLayoutConfig.homePageRoute = '/search';

        masterLayoutConfig.layout.hasLayout = false;
        masterLayoutConfig.layout.hasMainNavigation = true;

        masterLayoutConfig.header.isSticky = true;
        masterLayoutConfig.header.isSmall = true;
        masterLayoutConfig.footer.isSticky = true;

        notificationConfig.timeout = 8500;

        httpApiInterceptorConfig.api.spinner = true;
        httpApiInterceptorConfig.api.url = '/api/';

        this.languageService.init();
        this.easterEggService.init();
        this.userName$ = this.authService.userInfo$.pipe(map(userInfo => userInfo.name ?? ''));

        this.frontendConfig = configService.frontendConfig;

        if (this.frontendConfig.analytics.enabled) {
            this.statisticNavigationItems[0].children!.push({
                url: configService.frontendConfig.analytics.trackerUrl,
                label: 'navigation.statistic.matomo',
                isExternal: true,
            });
        }

        this.translateService.onLangChange
            .pipe(
                takeUntil(this.unsubscribe),
                map(item => item.lang)
            )
            .subscribe(language => {
                this.currentLanguage = language;
            });
    }

    ngOnInit(): void {
        this.informationService.loadApplicationVersion();

        if (this.frontendConfig.analytics.enabled) {
            const matomoTagManagerScript = document.createElement('script');
            matomoTagManagerScript.type = 'text/javascript';
            matomoTagManagerScript.setAttribute('nonce', 'MatomoTagManager');
            matomoTagManagerScript.innerHTML = `
                <!-- Matomo Tag Manager -->
                var _mtm = window._mtm = window._mtm || [];
                _mtm.push({'mtm.startTime': (new Date().getTime()), 'event': 'mtm.Start'});
                (function() {
                    var d=document, g=d.createElement('script'), s=d.getElementsByTagName('script')[0];
                    g.async=true; g.src='${this.frontendConfig.analytics.containerUrl}'; s.parentNode.insertBefore(g,s);
                })();
                <!-- End Matomo Tag Manager -->
            `;
            document.head.appendChild(matomoTagManagerScript);
        }

        this.route.queryParams.subscribe(async params => {
            if (params.reason === 'logout') {
                this.notificationService.info({message: 'application.loggedOut', timeout: 7000});

                await this.router.navigate([], {
                    relativeTo: this.route,
                    queryParams: {reason: null},
                    queryParamsHandling: 'merge',
                });
            }
        });

        this.httpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
        this.authService.checkAuth().subscribe();

        zip(this.authService.isAdmin$, this.authService.isAuthenticated$, this.authService.roles$).subscribe(([isAdmin, isAuthenticated, roles]) => {
            this.isAuthenticated = isAuthenticated;
            if (isAuthenticated && !roles.length) {
                this.window.setTimeout(async () => {
                    await this.router.navigate(['/unauthorized']);
                }, 50);
                return;
            }
            if (isAuthenticated && !this.navigation.some(nav => this.authenticatedNavigationItems.some(authnav => authnav.id === nav.id))) {
                this.navigation.push(...this.authenticatedNavigationItems);
            }
            if (isAdmin && !this.navigation.some(nav => this.adminNavigationItems.some(authnav => authnav.id === nav.id))) {
                this.navigation.push(...this.adminNavigationItems);
            }
            if (isAdmin && !this.navigation.some(nav => this.statisticNavigationItems.some(authnav => authnav.id === nav.id))) {
                this.navigation.push(...this.statisticNavigationItems);
            }
            if (isAdmin && !this.navigation.some(nav => this.importNavigationItems.some(authnav => authnav.id === nav.id))) {
                this.navigation.push(...this.importNavigationItems);
            }
            if (isAdmin && !this.navigation.some(nav => this.duplicateCheckNavigationItems.some(authnav => authnav.id === nav.id))) {
                this.navigation.push(...this.duplicateCheckNavigationItems);
            }

            // This is needed otherwise the change is not picked up
            this.navigation = [...this.navigation];
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    applicationVersion(): string | undefined {
        return this.informationService.applicationVersion;
    }

    login() {
        this.authService.login();
    }

    logout() {
        this.authService.logout();
    }

    showHelp(): void {
        this.window.open(this.helpLinks[this.currentLanguage], '_blank');
    }
}
