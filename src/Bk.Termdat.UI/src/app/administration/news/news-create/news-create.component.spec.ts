import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatIconModule} from '@angular/material/icon';
import {ActivatedRoute, Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {MockComponent, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ErrorService} from '../../../shared/error.service';
import {NewsFormComponent} from '../news-form/news-form.component';
import {NewsService} from '../news.service';
import {NewsCreateComponent} from './news-create.component';

const newsService = {
    create: jest.fn(),
    get: jest.fn(),
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

const errorService = {
    handleHttpError: jest.fn(),
};

const routeMock = {
    queryParamMap: of({
        get: (key: string) => {
            if (key === 'copyFrom') {
                return '5';
            }
            return null;
        },
    }),
};

const translateService = {
    getCurrentLang: jest.fn().mockReturnValue('de'),
    onLangChange: new Subject(),
    get: jest.fn((key: string) => of(key)),
    instant: jest.fn((key: string) => key),
};

describe('NewsCreateComponent', () => {
    let component: NewsCreateComponent;
    let fixture: ComponentFixture<NewsCreateComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [NewsCreateComponent, MockComponent(NewsFormComponent), MockModule(MatIconModule), MockModule(TranslateModule)],
            providers: [
                {provide: NewsService, useValue: newsService},
                {provide: Router, useValue: router},
                {provide: ObNotificationService, useValue: notificationService},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEvents},
                {provide: ErrorService, useValue: errorService},
                {provide: TranslateService, useValue: translateService},
                {provide: ActivatedRoute, useValue: routeMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(NewsCreateComponent);
        component = fixture.componentInstance;
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should create news successfully', () => {
        const news = {
            isEnabled: true,
            titleEn: 'Test news',
        } as NewsItemModification;

        newsService.create.mockReturnValue(of(null));

        component.save(news);

        expect(httpInterceptorEvents.deactivateNotificationOnNextAPICalls).toHaveBeenCalledWith(1);

        expect(newsService.create).toHaveBeenCalledWith(news);

        expect(notificationService.success).toHaveBeenCalledWith('news.notification.saveSuccess');

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news']);
    });

    it('should handle create error', () => {
        const news = {
            isEnabled: true,
            titleEn: 'Test news',
        } as NewsItemModification;

        const error = new Error('error');

        newsService.create.mockReturnValue(throwError(() => error));

        component.save(news);

        expect(errorService.handleHttpError).toHaveBeenCalledWith(error, 'news.notification.saveError');
    });

    it('should load copy source from query param', () => {
        const copySource = {
            titleDe: 'Copied',
        } as NewsItemModification;

        newsService.get.mockReturnValue(of(copySource));

        fixture = TestBed.createComponent(NewsCreateComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();

        expect(newsService.get).toHaveBeenCalledWith(5);
    });
});
