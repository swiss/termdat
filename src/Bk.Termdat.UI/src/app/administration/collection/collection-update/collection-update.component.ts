import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {CollectionModification} from 'api/CollectionModification';
import {switchMap, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {CollectionFormComponent} from '../collection-form/collection-form.component';
import {CollectionService} from '../collection.service';

@Component({
    selector: 'td-collection-update',
    templateUrl: './collection-update.component.html',
    styleUrls: ['./collection-update.component.scss'],
    imports: [CollectionFormComponent],
})
export class CollectionUpdateComponent implements OnInit {
    public collection!: CollectionModification;

    constructor(
        private readonly collectionService: CollectionService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) {}

    ngOnInit(): void {
        this.route.paramMap
            .pipe(
                switchMap(params => {
                    const idParam = params.get('id');
                    if (!idParam) {
                        this.back();
                    }
                    const collectionId = +idParam!;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.collectionService.get(collectionId).pipe(
                        tap({
                            next: data => {
                                this.collection = data;
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

    save(update: CollectionModification): void {
        const collectionUpdate = {...this.collection, ...update};
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.collectionService.update(collectionUpdate).subscribe({
            next: () => {
                this.notificationService.success('collection.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'collection.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'collection']);
    }
}
