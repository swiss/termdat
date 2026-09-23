import {AsyncPipe} from '@angular/common';
import {ChangeDetectionStrategy, Component} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {SubjectModification} from 'api/SubjectModification';
import {EMPTY, Observable} from 'rxjs';
import {catchError, map, switchMap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {TermdatClassificationSubjectFormComponent} from '../subject-form/subject-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';

@Component({
    selector: 'td-termdat-domain-update',
    templateUrl: './domain-update.component.html',
    styleUrls: ['./domain-update.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [TermdatClassificationSubjectFormComponent, AsyncPipe],
})
export class TermdatClassificationDomainUpdateComponent {
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
                const subjectIdParam = params.get('subjectId');
                const domainIdParam = params.get('domainId');
                if (!subjectIdParam || !domainIdParam) {
                    this.back();
                    return EMPTY;
                }
                const subjectId = +subjectIdParam;
                const domainId = +domainIdParam;
                this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                return this.classificationService.getSubject(subjectId).pipe(
                    map(response => {
                        response.domains = response.domains.filter(item => item.id === domainId);
                        return response;
                    }),
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
        if (update.domains.length !== 1) {
            return;
        }

        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.classificationService
            .updateDomain(update.domains[0])
            .pipe(
                catchError(error => {
                    this.errorService.handleHttpError(error, 'domain.notification.saveError');
                    return EMPTY;
                })
            )
            .subscribe(() => {
                this.notificationService.success('domain.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            });
    }

    back(): void {
        void this.router.navigate(['administration', 'termdatClassification']);
    }
}
