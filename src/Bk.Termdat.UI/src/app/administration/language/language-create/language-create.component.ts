import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {LanguageModification} from 'api/LanguageModification';
import {ErrorService} from '../../../shared/error.service';
import {LanguageService} from '../../../shared/language.service';
import {LanguageFormComponent} from '../language-form/language-form.component';

@Component({
    selector: 'td-language-create',
    templateUrl: './language-create.component.html',
    styleUrls: ['./language-create.component.scss'],
    imports: [LanguageFormComponent],
})
export class LanguageCreateComponent {
    constructor(
        private readonly languageService: LanguageService,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService,
        private readonly router: Router,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    save(language: LanguageModification): void {
        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.languageService.create(language).subscribe({
            next: () => {
                this.notificationService.success('administration.language.notification.saveSuccess');
                this.languageService.updateLanguageSummaries();
                void this.router.navigate(['administration', 'language']);
            },
            error: error => this.errorService.handleHttpError(error, 'administration.language.notification.saveError'),
        });
    }
}
