import {Component, Signal} from '@angular/core';
import {toSignal} from '@angular/core/rxjs-interop';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {of, switchMap} from 'rxjs';
import {ErrorService} from '../../../shared/error.service';
import {NewsFormComponent} from '../news-form/news-form.component';
import {NewsService} from '../news.service';

@Component({
    selector: 'td-news-create',
    imports: [NewsFormComponent],
    templateUrl: './news-create.component.html',
    styleUrl: './news-create.component.scss',
})
export class NewsCreateComponent {
    readonly copySource: Signal<NewsItemModification | null>;

    constructor(
        private readonly newsService: NewsService,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) {
        this.copySource = toSignal(
            this.route.queryParamMap.pipe(
                switchMap(params => {
                    const id = params.get('copyFrom');
                    return id ? this.newsService.get(+id) : of(null);
                })
            ),
            {initialValue: null}
        );
    }

    save(news: NewsItemModification): void {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.newsService.create(news).subscribe({
            next: () => {
                this.notificationService.success('news.notification.saveSuccess');
                void this.router.navigate(['administration', 'news']);
            },
            error: error => this.errorService.handleHttpError(error, 'news.notification.saveError'),
        });
    }
}
