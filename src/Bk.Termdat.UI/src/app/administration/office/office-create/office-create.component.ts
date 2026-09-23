import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {OfficeModification} from '../../../../api/OfficeModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {OfficeFormComponent} from '../office-form/office-form.component';
import {OfficeService} from '../office.service';

@Component({
    selector: 'td-office-create',
    templateUrl: './office-create.component.html',
    styleUrls: ['./office-create.component.scss'],
    imports: [OfficeFormComponent],
})
export class OfficeCreateComponent {
    constructor(
        private readonly officeService: OfficeService,
        private readonly notificationService: ObNotificationService,
        private readonly router: Router,
        private readonly errorService: ErrorService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    save(office: OfficeModification): void {
        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.officeService.create(office).subscribe({
            next: () => {
                this.notificationService.success('office.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'office']);
            },
            error: error => this.errorService.handleHttpError(error, 'office.notification.saveError'),
        });
    }
}
