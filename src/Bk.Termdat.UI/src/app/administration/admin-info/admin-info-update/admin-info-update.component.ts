import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {AdminInfoModification} from 'api/AdminInfoModification';
import {switchMap, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {AdminInfoFormComponent} from '../admin-info-form/admin-info-form.component';
import {AdminInfoService} from '../admin-info.service';

@Component({
    selector: 'td-admin-info-update',
    templateUrl: './admin-info-update.component.html',
    styleUrls: ['./admin-info-update.component.scss'],
    imports: [AdminInfoFormComponent],
})
export class AdminInfoUpdateComponent implements OnInit {
    public adminInfo!: AdminInfoModification;

    constructor(
        private readonly adminInfoService: AdminInfoService,
        private readonly notificationService: ObNotificationService,
        private readonly router: Router,
        private readonly errorService: ErrorService,
        private readonly route: ActivatedRoute,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
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
                    const adminInfoId = +idParam!;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.adminInfoService.get(adminInfoId).pipe(
                        tap({
                            next: data => {
                                this.adminInfo = data;
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

    save(update: AdminInfoModification): void {
        const adminInfoUpdate = {...this.adminInfo, ...update};
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.adminInfoService.update(adminInfoUpdate).subscribe({
            next: () => {
                this.notificationService.success('adminInfo.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'adminInfo.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'adminInfo']);
    }
}
