import {ChangeDetectionStrategy, Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {ConceptSystemModification} from 'api/ConceptSystemModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {ConceptSystemFormComponent} from '../concept-system-form/concept-system-form.component';
import {ConceptSystemService} from '../concept-system.service';

@Component({
    selector: 'td-concept-system-create',
    templateUrl: './concept-system-create.component.html',
    styleUrls: ['./concept-system-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [ConceptSystemFormComponent],
})
export class ConceptSystemCreateComponent {
    constructor(
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly conceptSystemService: ConceptSystemService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService,
        private readonly router: Router
    ) {}

    save(conceptSystem: ConceptSystemModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.conceptSystemService.create(conceptSystem).subscribe({
            next: () => {
                this.notificationService.success('conceptSystem.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'conceptSystem']);
            },
            error: error => this.errorService.handleHttpError(error, 'conceptSystem.notification.saveError'),
        });
    }
}
