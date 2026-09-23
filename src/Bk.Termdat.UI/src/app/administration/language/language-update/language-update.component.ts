import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {LanguageModification} from 'api/LanguageModification';
import {switchMap, tap} from 'rxjs/operators';
import {ErrorService} from '../../../shared/error.service';
import {LanguageService} from '../../../shared/language.service';
import {LanguageFormComponent} from '../language-form/language-form.component';

@Component({
    selector: 'td-language-update',
    templateUrl: './language-update.component.html',
    styleUrls: ['./language-update.component.scss'],
    imports: [LanguageFormComponent],
})
export class LanguageUpdateComponent implements OnInit {
    public language!: LanguageModification;

    constructor(
        private readonly languageService: LanguageService,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService,
        private readonly router: Router,
        private readonly route: ActivatedRoute,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    ngOnInit(): void {
        this.route.paramMap
            .pipe(
                switchMap(params => {
                    const idParam = params.get('id');
                    if (!idParam) {
                        this.back();
                    }
                    const languageId = +idParam!;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.languageService.get(languageId).pipe(
                        tap({
                            next: data => {
                                this.language = data;
                            },
                            error: error => {
                                this.errorService.handleHttpError(error, 'collection.notification.saveError');
                                this.back();
                            },
                        })
                    );
                })
            )
            .subscribe();
    }

    save(update: LanguageModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        const languageUpdate = {...this.language, ...update};
        this.languageService.update(languageUpdate).subscribe({
            next: () => {
                this.notificationService.success('administration.language.notification.saveSuccess');
                this.languageService.updateLanguageSummaries();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'administration.language.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'language']);
    }
}
