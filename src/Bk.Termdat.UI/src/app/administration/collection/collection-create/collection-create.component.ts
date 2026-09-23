import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {CollectionModification} from 'api/CollectionModification';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {CollectionFormComponent} from '../collection-form/collection-form.component';
import {CollectionService} from '../collection.service';

@Component({
    selector: 'td-collection-create',
    templateUrl: './collection-create.component.html',
    styleUrls: ['./collection-create.component.scss'],
    imports: [CollectionFormComponent],
})
export class CollectionCreateComponent {
    constructor(
        private readonly collectionService: CollectionService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly router: Router
    ) {}

    save(collection: CollectionModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.collectionService.create(collection).subscribe({
            next: () => {
                this.notificationService.success('collection.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                void this.router.navigate(['administration', 'collection']);
            },
            error: error => this.errorService.handleHttpError(error, 'collection.notification.saveError'),
        });
    }
}
