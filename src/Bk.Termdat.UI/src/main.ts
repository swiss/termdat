import {registerLocaleData} from '@angular/common';
import {HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi} from '@angular/common/http';
import localeDECH from '@angular/common/locales/de-CH';
import localeFRCH from '@angular/common/locales/fr-CH';
import localeITCH from '@angular/common/locales/it-CH';
import {inject, LOCALE_ID, provideAppInitializer, provideZoneChangeDetection} from '@angular/core';
import {DateAdapter, MAT_DATE_FORMATS} from '@angular/material/core';
import {MAT_DIALOG_DEFAULT_OPTIONS} from '@angular/material/dialog';
import {MatPaginatorIntl} from '@angular/material/paginator';
import {bootstrapApplication} from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideRouter} from '@angular/router';
import {OB_BANNER, ObHttpApiInterceptor, provideObliqueConfiguration} from '@oblique/oblique';
import {firstValueFrom} from 'rxjs';
import {AppComponent} from './app/app.component';
import {routes} from './app/app.routes';
import {authErrorInterceptor} from './app/auth/auth-error.interceptor';
import {ConfigsService} from './app/config.service';
import {LanguageHeaderInterceptor} from './app/interceptor/language-header.interceptor';
import {CONSOLE, DOCUMENT, LOCAL_STORAGE, LOCATION, SESSION_STORAGE, WINDOW} from './app/shared/injection.tokens';
import {TD_DATE_FORMATS, TdDateAdapter} from './app/shared/td-date-adapter';
import {TermdatPaginatorIntl} from './app/shared/termdat-paginator-intl';

registerLocaleData(localeDECH);
registerLocaleData(localeFRCH);
registerLocaleData(localeITCH);

bootstrapApplication(AppComponent, {
    providers: [
        provideObliqueConfiguration({
            accessibilityStatement: {
                createdOn: new Date('2026-06-02'),
                conformity: 'none',
                applicationName: 'TERMDAT',
                applicationOperator: 'BK DevOps',
                contact: [{email: 'BK_DevOPS@bit.admin.ch'}],
            },
            translate: {
                additionalFiles: [
                    {prefix: './assets/i18n/oblique-', suffix: `.json?v=${new Date().toISOString()}`},
                    {prefix: './assets/i18n/', suffix: `.json?v=${new Date().toISOString()}`},
                ],
                locales: {
                    defaultLanguage: 'de',
                    locales: ['de', 'fr', 'it', 'rm', 'en'],
                    disabled: false,
                    languages: {
                        de: 'Deutsch',
                        fr: 'Français',
                        it: 'Italiano',
                        rm: 'Rumantsch',
                        en: 'English',
                    },
                },
            },
        }),
        {provide: LOCALE_ID, useValue: 'de-CH'},
        provideAppInitializer(() => {
            const configsService = inject(ConfigsService);
            return firstValueFrom(configsService.loadFrontendConfig());
        }),
        {provide: HTTP_INTERCEPTORS, useClass: ObHttpApiInterceptor, multi: true},
        {provide: HTTP_INTERCEPTORS, useClass: LanguageHeaderInterceptor, multi: true},
        {provide: MatPaginatorIntl, useClass: TermdatPaginatorIntl},
        {provide: DateAdapter, useClass: TdDateAdapter},
        {provide: MAT_DATE_FORMATS, useValue: TD_DATE_FORMATS},
        {provide: OB_BANNER, useFactory: (configsService: ConfigsService) => configsService.bannerInfo, deps: [ConfigsService]},
        {
            provide: MAT_DIALOG_DEFAULT_OPTIONS,
            useValue: {hasBackdrop: true, disableClose: true, minWidth: '24em', width: '40vw', maxWidth: '52em'},
        },
        {provide: CONSOLE, useValue: console},
        {provide: WINDOW, useValue: window},
        {provide: DOCUMENT, useValue: document},
        {provide: LOCAL_STORAGE, useValue: window.localStorage},
        {provide: SESSION_STORAGE, useValue: window.sessionStorage},
        {provide: LOCATION, useValue: window.location},
        provideHttpClient(withInterceptorsFromDi(), withInterceptors([authErrorInterceptor])),
        provideAnimations(),
        provideRouter(routes),
        provideZoneChangeDetection({
            eventCoalescing: true, // batch multiple DOM events into a single change-detection run
        }),
    ],
}).catch(err => console.error(err));
