import {AsyncPipe} from '@angular/common';
import {ChangeDetectionStrategy, Component} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {ConceptSystemModification} from 'api/ConceptSystemModification';
import {EMPTY, Observable} from 'rxjs';
import {catchError, switchMap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {ConceptSystemFormComponent} from '../concept-system-form/concept-system-form.component';
import {ConceptSystemService} from '../concept-system.service';

@Component({
    selector: 'td-concept-system-update',
    templateUrl: './concept-system-update.component.html',
    styleUrls: ['./concept-system-update.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [ConceptSystemFormComponent, AsyncPipe],
})
export class ConceptSystemUpdateComponent {
    public readonly conceptSystem$: Observable<ConceptSystemModification>;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly conceptSystemService: ConceptSystemService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService
    ) {
        this.conceptSystem$ = this.route.paramMap.pipe(
            switchMap(params => {
                const idParam = params.get('id');
                if (!idParam) {
                    this.back();
                    return EMPTY;
                }
                const id = +idParam;
                this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                return this.conceptSystemService.get(id).pipe(
                    catchError(error => {
                        this.errorService.handleHttpError(error, 'conceptSystem.notification.loadError');
                        this.back();
                        return EMPTY;
                    })
                );
            })
        );
    }

    save(update: ConceptSystemModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.conceptSystemService.update(update).subscribe({
            next: () => {
                this.notificationService.success('conceptSystem.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'conceptSystem.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'conceptSystem']);
    }
}
