import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {GlossaryModification} from 'api/GlossaryModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {GlossaryFormComponent} from '../glossary-form/glossary-form.component';
import {GlossaryService} from '../glossary.service';

@Component({
    selector: 'td-glossary-create',
    templateUrl: './glossary-create.component.html',
    styleUrls: ['./glossary-create.component.scss'],
    imports: [GlossaryFormComponent],
})
export class GlossaryCreateComponent {
    constructor(
        private readonly glossaryService: GlossaryService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly router: Router
    ) {}

    save(glossary: GlossaryModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.glossaryService.create(glossary).subscribe({
            next: () => {
                this.notificationService.success('glossary.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'glossary']);
            },
            error: error => this.errorService.handleHttpError(error, 'glossary.notification.saveError'),
        });
    }
}
