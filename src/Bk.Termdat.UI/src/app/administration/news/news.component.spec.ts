import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatDialog} from '@angular/material/dialog';
import {MatIconModule} from '@angular/material/icon';
import {MatPaginator} from '@angular/material/paginator';
import {ActivatedRoute, Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObNotificationService, WINDOW} from '@oblique/oblique';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {MockModule} from 'ng-mocks';
import {EMPTY, of, Subject, throwError} from 'rxjs';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {NewsComponent} from './news.component';
import {NewsService} from './news.service';

const newsService = {
    getNewsSummaries: jest.fn(),
    delete: jest.fn(),
    moveUp: jest.fn(),
    moveDown: jest.fn(),
};

const router = {
    navigate: jest.fn(),
};

const paramMock = {get: jest.fn()};
const routeMock = {
    paramMap: of(paramMock),
};

const translateService = {
    getCurrentLang: jest.fn().mockReturnValue('de'),
    onLangChange: new Subject(),
    get: jest.fn((key: string) => of(key)),
    instant: jest.fn((key: string) => key),
};

const dialog = {
    open: jest.fn(),
};

const errorService = {
    handleHttpError: jest.fn(),
};

const notificationService = {
    success: jest.fn(),
};

describe('NewsComponent', () => {
    let component: NewsComponent;
    let fixture: ComponentFixture<NewsComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [NewsComponent, MockModule(MatIconModule), MockModule(TranslateModule)],
            providers: [
                {provide: NewsService, useValue: newsService},
                {provide: Router, useValue: router},
                {provide: TranslateService, useValue: translateService},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: MatDialog, useValue: dialog},
                {provide: ErrorService, useValue: errorService},
                {provide: ObNotificationService, useValue: notificationService},
                {provide: WINDOW, useValue: window},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(NewsComponent);
        component = fixture.componentInstance;
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load news on init', () => {
        const news = [
            {
                id: 1,
                title: 'News',
            },
        ] as NewsItemSummary[];

        newsService.getNewsSummaries.mockReturnValue(of(news));

        fixture.detectChanges();

        expect(newsService.getNewsSummaries).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(news);
    });

    it('should navigate to update page', () => {
        component.update({id: 42} as NewsItemSummary);

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news', 42]);
    });

    it('should navigate to create page when create is called', () => {
        component.create();

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news', 'create']);
    });

    it('should reload news on language change', () => {
        newsService.getNewsSummaries.mockReturnValue(of([]));

        fixture.detectChanges();

        expect(newsService.getNewsSummaries).toHaveBeenCalledTimes(1);

        translateService.onLangChange.next({lang: 'fr'});

        expect(newsService.getNewsSummaries).toHaveBeenCalledTimes(2);
    });

    describe('delete', () => {
        const news = {
            id: 42,
            title: 'Test news',
        } as NewsItemSummary;

        it('should open delete confirmation dialog', () => {
            dialog.open.mockReturnValue({
                afterClosed: () => of(false),
            });

            component.delete(news);

            expect(dialog.open).toHaveBeenCalledWith(
                ConfirmDeleteDialogComponent,
                expect.objectContaining({
                    data: {
                        title: 'news.delete.title',
                        text: 'news.delete.text',
                        textValue: {
                            title: 'Test news',
                        },
                        confirm: 'news.delete.confirm',
                    },
                })
            );
        });

        it('should delete news after confirmation', () => {
            dialog.open.mockReturnValue({
                afterClosed: () => of(true),
            });

            newsService.delete.mockReturnValue(of(undefined));

            component.delete(news);

            expect(newsService.delete).toHaveBeenCalledWith(42);
            expect(notificationService.success).toHaveBeenCalledWith({
                title: 'news.delete.successTitle',
                message: 'news.delete.successMessage',
            });
        });

        it('should not delete news when dialog is cancelled', () => {
            dialog.open.mockReturnValue({
                afterClosed: () => of(false),
            });

            component.delete(news);

            expect(newsService.delete).not.toHaveBeenCalled();
            expect(notificationService.success).not.toHaveBeenCalled();
        });

        it('should handle delete errors', () => {
            const error = new Error('Delete failed');

            dialog.open.mockReturnValue({
                afterClosed: () => of(true),
            });

            newsService.delete.mockReturnValue(throwError(() => error));

            errorService.handleHttpError.mockReturnValue(EMPTY);

            component.delete(news);

            expect(newsService.delete).toHaveBeenCalledWith(42);
            expect(errorService.handleHttpError).toHaveBeenCalledWith(error, 'news.notification.deleteError');
            expect(notificationService.success).not.toHaveBeenCalled();
        });
    });

    describe('filter methods', () => {
        beforeEach(() => {
            const mockNewsItem = {
                id: 1,
                title: 'Test title',
                text: 'Test text',
                created: new Date(2026, 7, 7),
            } as NewsItemSummary;

            component.dataSource.data = [mockNewsItem];

            fixture.detectChanges();
        });

        it('should clear filter and reset paginator page when clearFilter is called', () => {
            component.dataSource.filter = 'test';

            const firstPage = jest.fn();

            component.dataSource.paginator = {
                firstPage,
            } as unknown as MatPaginator;

            component.clearFilter();

            expect(component.dataSource.filter).toBe('');
            expect(firstPage).toHaveBeenCalled();
        });

        it('should apply trimmed filter value and reset paginator page', () => {
            const firstPage = jest.fn();

            component.dataSource.paginator = {
                firstPage,
            } as unknown as MatPaginator;

            const event = {
                target: {
                    value: '  test filter  ',
                },
            } as unknown as Event;

            component.applyFilter(event);

            expect(component.dataSource.filter).toBe('test filter');
            expect(firstPage).toHaveBeenCalled();
        });

        it('should apply empty filter when event value is missing', () => {
            const firstPage = jest.fn();

            component.dataSource.paginator = {
                firstPage,
            } as unknown as MatPaginator;

            const event = {
                target: {},
            } as Event;

            component.applyFilter(event);

            expect(component.dataSource.filter).toBe('');
            expect(firstPage).toHaveBeenCalled();
        });
    });

    it('should navigate to create page with copy source', () => {
        component.copy({id: 42} as NewsItemSummary);

        expect(router.navigate).toHaveBeenCalledWith(['administration', 'news', 'create'], {
            queryParams: {
                copyFrom: 42,
            },
        });
    });

    it('should set custom sorted when non-default sorting is applied', () => {
        component.onSortChange({
            active: 'title',
            direction: 'asc',
        });
        // eslint-disable-next-line dot-notation
        expect(component['isCustomSorted']()).toBe(true);
    });

    it('should not set custom sorted for default sort index ascending', () => {
        component.onSortChange({
            active: 'sortIndex',
            direction: 'asc',
        });
        // eslint-disable-next-line dot-notation
        expect(component['isCustomSorted']()).toBe(false);
    });

    it('should set filtered state when applying non-empty filter', () => {
        const event = {
            target: {
                value: ' test ',
            },
        } as unknown as Event;

        component.applyFilter(event);

        expect(component.dataSource.filter).toBe('test');
        // eslint-disable-next-line dot-notation
        expect(component['isFiltered']()).toBe(true);
    });

    it('should set filtered state when applying non-empty filter', () => {
        const event = {
            target: {
                value: ' test ',
            },
        } as unknown as Event;

        component.applyFilter(event);

        expect(component.dataSource.filter).toBe('test');
        // eslint-disable-next-line dot-notation
        expect(component['isFiltered']()).toBe(true);
    });

    it('should reset filtered state when clearing filter', () => {
        const event = {
            target: {
                value: ' test ',
            },
        } as unknown as Event;

        component.applyFilter(event);

        // eslint-disable-next-line dot-notation
        expect(component['isFiltered']()).toBe(true);

        component.clearFilter();

        expect(component.dataSource.filter).toBe('');
        // eslint-disable-next-line dot-notation
        expect(component['isFiltered']()).toBe(false);
    });

    it('should move news item up and reload', () => {
        newsService.moveUp.mockReturnValue(of(undefined));

        component.moveUp({id: 42} as NewsItemSummary);

        expect(newsService.moveUp).toHaveBeenCalledWith(42);
    });

    it('should move news item down and reload', () => {
        newsService.moveDown.mockReturnValue(of(undefined));

        component.moveDown({id: 42} as NewsItemSummary);

        expect(newsService.moveDown).toHaveBeenCalledWith(42);
    });
});
