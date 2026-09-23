import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {GlossaryModification} from 'api/GlossaryModification';
import {switchMap, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {GlossaryFormComponent} from '../glossary-form/glossary-form.component';
import {GlossaryService} from '../glossary.service';

@Component({
    selector: 'td-glossary-update',
    templateUrl: './glossary-update.component.html',
    styleUrls: ['./glossary-update.component.scss'],
    imports: [GlossaryFormComponent],
})
export class GlossaryUpdateComponent implements OnInit {
    public glossary!: GlossaryModification;

    constructor(
        private readonly glossaryService: GlossaryService,
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
                    const glossaryId = +idParam!;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.glossaryService.get(glossaryId).pipe(
                        tap({
                            next: data => {
                                this.glossary = data;
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

    save(update: GlossaryModification): void {
        const glossaryUpdate = {...this.glossary, ...update};
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.glossaryService.update(glossaryUpdate).subscribe({
            next: () => {
                this.notificationService.success('glossary.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'glossary.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'glossary']);
    }
}
