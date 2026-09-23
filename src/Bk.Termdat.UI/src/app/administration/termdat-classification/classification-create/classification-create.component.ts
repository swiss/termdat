import {ChangeDetectionStrategy, Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {SubjectModification} from 'api/SubjectModification';
import {EMPTY} from 'rxjs';
import {catchError} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {TermdatClassificationSubjectFormComponent} from '../subject-form/subject-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';

@Component({
    selector: 'td-classification-create',
    templateUrl: './classification-create.component.html',
    styleUrls: ['./classification-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [TermdatClassificationSubjectFormComponent],
})
export class TermdatClassificationCreateComponent {
    constructor(
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly classificationService: TermdatClassificationService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService,
        private readonly router: Router
    ) {}

    save(subject: SubjectModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.classificationService
            .createSubject(subject)
            .pipe(
                catchError(error => {
                    this.errorService.handleHttpError(error, 'subject.notification.saveError');
                    return EMPTY;
                })
            )
            .subscribe(() => {
                this.notificationService.success('subject.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'termdatClassification']);
            });
    }

    cancel(): void {
        void this.router.navigate(['administration', 'termdatClassification']);
    }
}
