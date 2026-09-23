import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {switchMap, tap} from 'rxjs/operators';
import {OfficeModification} from '../../../../api/OfficeModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {OfficeFormComponent} from '../office-form/office-form.component';
import {OfficeService} from '../office.service';

@Component({
    selector: 'td-office-update',
    templateUrl: './office-update.component.html',
    styleUrls: ['./office-update.component.scss'],
    imports: [OfficeFormComponent],
})
export class OfficeUpdateComponent implements OnInit {
    public office!: OfficeModification;

    constructor(
        private readonly officeService: OfficeService,
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
                    const officeId = +idParam!;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.officeService.get(officeId).pipe(
                        tap({
                            next: data => {
                                this.office = data;
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

    save(update: OfficeModification): void {
        const officeUpdate = {...this.office, ...update};
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.officeService.update(officeUpdate).subscribe({
            next: () => {
                this.notificationService.success('office.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'office.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'office']);
    }
}
