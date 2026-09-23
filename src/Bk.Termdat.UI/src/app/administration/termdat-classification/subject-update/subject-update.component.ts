import {AsyncPipe} from '@angular/common';
import {ChangeDetectionStrategy, Component} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {SubjectModification} from 'api/SubjectModification';
import {EMPTY, Observable} from 'rxjs';
import {catchError, switchMap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {TermdatClassificationSubjectFormComponent} from '../subject-form/subject-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';

@Component({
    selector: 'td-termdat-subject-update',
    templateUrl: './subject-update.component.html',
    styleUrls: ['./subject-update.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [TermdatClassificationSubjectFormComponent, AsyncPipe],
})
export class TermdatClassificationSubjectUpdateComponent {
    public readonly subject$: Observable<SubjectModification>;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly classificationService: TermdatClassificationService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService
    ) {
        this.subject$ = this.route.paramMap.pipe(
            switchMap(params => {
                const idParam = params.get('id');
                if (!idParam) {
                    this.back();
                    return EMPTY;
                }
                const id = +idParam;
                this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                return this.classificationService.getSubject(id).pipe(
                    catchError(error => {
                        this.errorService.handleHttpError(error, 'subject.notification.loadError');
                        this.back();
                        return EMPTY;
                    })
                );
            })
        );
    }

    save(update: SubjectModification): void {
        // Only pass newly created domains to the api
        update.domains = update.domains.filter(item => !item.id);

        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.classificationService
            .updateSubject(update)
            .pipe(
                catchError(error => {
                    this.errorService.handleHttpError(error, 'subject.notification.saveError');
                    return EMPTY;
                })
            )
            .subscribe(() => {
                this.notificationService.success('subject.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            });
    }

    back(): void {
        void this.router.navigate(['administration', 'termdatClassification']);
    }
}
