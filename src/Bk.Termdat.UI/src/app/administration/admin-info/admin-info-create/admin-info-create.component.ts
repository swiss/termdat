import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {AdminInfoModification} from 'api/AdminInfoModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {AdminInfoFormComponent} from '../admin-info-form/admin-info-form.component';
import {AdminInfoService} from '../admin-info.service';

@Component({
    selector: 'td-admin-info-create',
    templateUrl: './admin-info-create.component.html',
    styleUrls: ['./admin-info-create.component.scss'],
    imports: [AdminInfoFormComponent],
})
export class AdminInfoCreateComponent {
    constructor(
        private readonly adminInfoService: AdminInfoService,
        private readonly notificationService: ObNotificationService,
        private readonly router: Router,
        private readonly errorService: ErrorService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    save(adminInfo: AdminInfoModification): void {
        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.adminInfoService.create(adminInfo).subscribe({
            next: () => {
                this.notificationService.success('adminInfo.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'adminInfo']);
            },
            error: error => this.errorService.handleHttpError(error, 'adminInfo.notification.saveError'),
        });
    }
}
