import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatDialog} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObInputClearModule, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {StatisticSearchResult} from 'api/StatisticSearchResult';
import {StatisticSummary} from 'api/StatisticSummary';
import {MockComponent, MockDirective, MockModule} from 'ng-mocks';
import {EMPTY, of, Subject, throwError} from 'rxjs';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {StatisticComponent} from './statistic.component';
import {StatisticService} from './statistic.service';

describe('StatisticComponent', () => {
    let component: StatisticComponent;
    let fixture: ComponentFixture<StatisticComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(() => throwError(() => ({error: {}}))),
    };
    const httpInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(() => dialogRefMock),
    };
    const notificationServiceMock = {
        warning: jest.fn(),
    };

    let statisticSummaries: StatisticSummary[] = [];

    let searchResult: StatisticSearchResult;

    const statisticServiceMock = {
        getStatisticsBySearchQuery: jest.fn(),
        delete: jest.fn(),
        deleteSelection: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatPaginatorModule),
                MockModule(MatDatepickerModule),
                MockModule(MatFormFieldModule),
                MockModule(MatTableModule),
                MockModule(MatInputModule),
                MockModule(MatSortModule),
                MockModule(MatTooltipModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockModule(TranslateModule),
                MockModule(ObInputClearModule),
                ReactiveFormsModule,
                StatisticComponent,
                MockComponent(ConfirmDeleteDialogComponent),
                MockDirective(ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: StatisticService, useValue: statisticServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        statisticSummaries = [
            {
                id: 10000,
                searchText: 'SearchText1',
                numberOfSearches: 53,
                languages: 'DE, FR, EN',
                searchFields: 'AB, Na, Md',
                dateLastSearch: new Date(),
            },
            {
                id: 20000,
                searchText: 'SearchText2',
                numberOfSearches: 15,
                languages: 'DE, FR, EN, IT',
                searchFields: 'AB, Na, Md, PH, VE',
                dateLastSearch: new Date(),
            },
        ];

        searchResult = {
            statistics: statisticSummaries,
            totalCount: 2,
        };

        statisticServiceMock.getStatisticsBySearchQuery.mockReturnValue(of(searchResult));
        dialogRefMock.afterClosed.mockReturnValue(of({}));

        fixture = TestBed.createComponent(StatisticComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set paginator and sort', () => {
        expect(component.pagination).toBeDefined();
        expect(component.sort).toBeDefined();
    });

    it('should load statistics on init', () => {
        expect(statisticServiceMock.getStatisticsBySearchQuery).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(statisticSummaries);
    });

    it('should reload statistics on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(statisticServiceMock.getStatisticsBySearchQuery).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(statisticSummaries);
    });

    it('should call statistic service when searching', () => {
        expect(component.dataSource.filteredData.length).toBe(2);

        component.search();

        expect(statisticServiceMock.getStatisticsBySearchQuery).toHaveBeenCalledTimes(2);
    });

    it('should display confirm dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(EMPTY);

        component.delete({
            id: 100,
            searchText: 'test',
            numberOfSearches: 33,
            languages: 'DE, FR',
            searchFields: 'AB, CD',
            dateLastSearch: new Date(),
        });

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
    });

    it('should not delete statistic if confirmation is declined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete({
            id: 100,
            searchText: 'test',
            numberOfSearches: 33,
            languages: 'DE, FR',
            searchFields: 'AB, CD',
            dateLastSearch: new Date(),
        });

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(statisticServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should delete statistic if confirmed', () => {
        jest.clearAllMocks();

        dialogRefMock.afterClosed.mockReturnValue(of(true));
        statisticServiceMock.delete.mockReturnValue(of({}));

        component.delete({
            id: 100,
            searchText: 'test',
            numberOfSearches: 33,
            languages: 'DE, FR',
            searchFields: 'AB, CD',
            dateLastSearch: new Date(),
        });

        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(statisticServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(statisticServiceMock.delete).toHaveBeenCalledWith(100);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(statisticServiceMock.getStatisticsBySearchQuery).toHaveBeenCalled();
        expect(statisticServiceMock.getStatisticsBySearchQuery).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toBe(statisticSummaries);
    });

    it('should display confirm dialog on bulk delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(EMPTY);

        component.bulkDelete();

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
    });
});
