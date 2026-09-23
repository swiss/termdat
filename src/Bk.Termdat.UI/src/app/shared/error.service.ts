import {HttpErrorResponse} from '@angular/common/http';
import {Inject, Injectable} from '@angular/core';
import {ValidationErrors} from '@angular/forms';
import {Router} from '@angular/router';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, throwError} from 'rxjs';
import {CONSOLE} from './injection.tokens';

@Injectable({
    providedIn: 'root',
})
export class ErrorService {
    constructor(
        private readonly translateService: TranslateService,
        private readonly notificationService: ObNotificationService,
        private readonly router: Router,
        @Inject(CONSOLE) private readonly console: Console
    ) {}

    handleHttpError(error: HttpErrorResponse, unknownTranslationKey: string): Observable<never> {
        if (error.status === 400) {
            const errors = error.error?.errors ?? {
                unknown: this.translateService.instant(unknownTranslationKey),
            };

            Object.values<string>(errors)
                .flat()
                .forEach(errorMessage => {
                    this.notificationService.warning({
                        message: errorMessage,
                        sticky: false,
                        timeout: 5000,
                    });

                    this.console.error(errorMessage);
                });
        } else if (error.status === 404) {
            this.notificationService.warning({
                message: error.error,
                sticky: false,
                timeout: 5000,
            });
        } else if (error.status === 403) {
            // Oblique already creates a notification with "i18n.oblique.http.error.status.403"
            void this.router.navigate(['/unauthorized']);
        } else if (error.status === 409) {
            this.notificationService.error({
                message: this.translateService.instant('i18n.oblique.http.error.status.409'),
                sticky: false,
                timeout: 5000,
            });
        }

        return throwError(() => error);
    }

    getControlError(errors: ValidationErrors | null, label: string, labelPrefix: string): string {
        if (errors === null) {
            return '';
        }

        const errorKeys = Object.keys(errors);

        return this.translateService.instant(`${labelPrefix}.${errorKeys[0]}`, {field: label});
    }
}
