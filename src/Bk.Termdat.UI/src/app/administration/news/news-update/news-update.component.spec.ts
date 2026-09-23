import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatIconModule} from '@angular/material/icon';
import {ActivatedRoute, convertToParamMap, ParamMap, Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {MockComponent, MockModule} from 'ng-mocks';
import {Observable, of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {NewsFormComponent} from '../news-form/news-form.component';
import {NewsService} from '../news.service';
import {NewsUpdateComponent} from './news-update.component';

const newsService = {
    get: jest.fn(),
    update: jest.fn(),
};

const router = {
    navigate: jest.fn(),
};

const notificationService = {
    success: jest.fn(),
};

const httpInterceptorEvents = {
    deactivateNotificationOnNextAPICalls: jest.fn(),
};

const extendedSearchFilterService = {
    clear: jest.fn(),
};

const errorService = {
    handleHttpError: jest.fn(),
};

const translateService = {
    getCurrentLang: jest.fn().mockReturnValue('de'),
    onLangChange: new Subject(),
    get: jest.fn((key: string) => of(key)),
    instant: jest.fn((key: string) => key),
};

describe('NewsUpdateComponent', () => {
    let component: NewsUpdateComponent;
    let fixture: ComponentFixture<NewsUpdateComponent>;

    let routeMock: {paramMap: Observable<ParamMap>};

    beforeEach(async () => {
        routeMock = {
            paramMap: of(
                convertToParamMap({
                    id: '42',
                })
            ),
        };

        await TestBed.configureTestingModule({
            imports: [NewsUpdateComponent, MockComponent(NewsFormComponent), MockModule(MatIconModule), MockModule(TranslateModule)],
            providers: [
                {provide: NewsService, useValue: newsService},
                {provide: Router, useValue: router},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: ObNotificationService, useValue: notificationService},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEvents},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFilterService},
                {provide: ErrorService, useValue: errorService},
                {provide: TranslateService, useValue: translateService},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(NewsUpdateComponent);
        component = fixture.componentInstance;
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load news on init', () => {
        const news = {
            id: 42,
            titleEn: 'News',
        } as NewsItemModification;

        newsService.get.mockReturnValue(of(news));

        fixture.detectChanges();

        expect(newsService.get).toHaveBeenCalledWith(42);
        expect(component.newsItem).toEqual(news);
        expect(httpInterceptorEvents.deactivateNotificationOnNextAPICalls).toHaveBeenCalledWith(1);
    });

    it('should navigate back when id is missing', () => {
        routeMock.paramMap = of(convertToParamMap({}));

        fixture = TestBed.createComponent(NewsUpdateComponent);
        fixture.detectChanges();

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news']);
    });

    it('should update news successfully', () => {
        component.newsItem = {
            id: 42,
            titleEn: 'Old title',
        } as NewsItemModification;

        const update = {
            titleEn: 'New title',
        } as NewsItemModification;

        newsService.update.mockReturnValue(of(null));

        component.save(update);

        expect(newsService.update).toHaveBeenCalledWith({
            id: 42,
            titleEn: 'New title',
        });

        expect(notificationService.success).toHaveBeenCalledWith('news.notification.saveSuccess');

        expect(extendedSearchFilterService.clear).toHaveBeenCalled();

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news']);
    });

    it('should handle update error', () => {
        component.newsItem = {
            id: 42,
        } as NewsItemModification;

        const error = new Error('error');

        newsService.update.mockReturnValue(throwError(() => error));

        component.save({} as NewsItemModification);

        expect(errorService.handleHttpError).toHaveBeenCalledWith(error, 'news.notification.saveError');
    });

    it('should handle loading error', () => {
        const error = new Error('error');

        newsService.get.mockReturnValue(throwError(() => error));

        fixture.detectChanges();

        expect(newsService.get).toHaveBeenCalledWith(42);

        expect(errorService.handleHttpError).toHaveBeenCalledWith(error, 'news.notification.saveError');

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news']);
    });
});
