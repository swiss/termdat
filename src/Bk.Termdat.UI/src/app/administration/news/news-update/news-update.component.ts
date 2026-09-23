import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {EMPTY} from 'rxjs';
import {switchMap, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {NewsFormComponent} from '../news-form/news-form.component';
import {NewsService} from '../news.service';

@Component({
    selector: 'td-news-update',
    imports: [NewsFormComponent],
    templateUrl: './news-update.component.html',
    styleUrl: './news-update.component.scss',
})
export class NewsUpdateComponent implements OnInit {
    public newsItem!: NewsItemModification;

    constructor(
        private readonly newsService: NewsService,
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
                        return EMPTY;
                    }
                    const newsId = +idParam;
                    this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.newsService.get(newsId).pipe(
                        tap({
                            next: data => {
                                this.newsItem = data;
                            },
                            error: error => {
                                this.errorService.handleHttpError(error, 'news.notification.saveError');
                                this.back();
                            },
                        })
                    );
                })
            )
            .subscribe();
    }

    save(update: NewsItemModification): void {
        const newsItemUpdate = {...this.newsItem, ...update};
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.newsService.update(newsItemUpdate).subscribe({
            next: () => {
                this.notificationService.success('news.notification.saveSuccess');
                this.extendedSearchFilterService.clear();
                this.back();
            },
            error: error => this.errorService.handleHttpError(error, 'news.notification.saveError'),
        });
    }

    private back(): void {
        void this.router.navigate(['administration', 'news']);
    }
}
