import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {ActivatedRoute, Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObInputClearModule, ObNotificationService} from '@oblique/oblique';
import {AdminInfoDetail} from 'api/AdminInfoDetail';
import {MockComponent, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {AdminInfoComponent} from './admin-info.component';
import {AdminInfoService} from './admin-info.service';

describe('AdminInfoComponent', () => {
    let component: AdminInfoComponent;
    let fixture: ComponentFixture<AdminInfoComponent>;

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

    let adminInfos: AdminInfoDetail[] = [];

    const adminInfoServiceMock = {
        getAdminInfoDetails: jest.fn(),
        delete: jest.fn(() => of({})),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MatPaginatorModule,
                MatFormFieldModule,
                MatTableModule,
                MatInputModule,
                MatSortModule,
                MatTooltipModule,
                MatButtonModule,
                MockModule(MatIconModule),
                MockModule(TranslateModule),
                MockModule(ObInputClearModule),
                AdminInfoComponent,
                MockComponent(ConfirmDeleteDialogComponent),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: AdminInfoService, useValue: adminInfoServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: {}},
            ],
        }).compileComponents();

        adminInfos = [
            {
                id: 10000,
                code: 'Info1',
                entryCount: 200,
                createdBy: 'test@test.ch',
                modifiedBy: 'test2@test.ch',
                modified: new Date('2013-02-22T08:05:05.78'),
                created: new Date('2015-04-24T08:05:05.78'),
            } as AdminInfoDetail,
            {
                id: 20000,
                code: 'Info2',
                description: 'Description 2',
                entryCount: 300,
                createdBy: 'test3@test.ch',
                modifiedBy: 'test4@test.ch',
                modified: new Date('2014-02-22T14:05:05.78'),
                created: new Date('2016-02-22T15:05:05.78'),
            },
            {
                id: 30000,
                code: 'Info3',
                entryCount: 400,
                createdBy: 'test@test.ch',
                modifiedBy: 'test@test.ch',
                modified: new Date('2014-02-22T11:05:05.78'),
                created: new Date('2014-02-22T11:05:05.78'),
            } as AdminInfoDetail,
        ];

        adminInfoServiceMock.getAdminInfoDetails.mockReturnValue(of(adminInfos));
        dialogRefMock.afterClosed.mockReturnValue(of({}));

        fixture = TestBed.createComponent(AdminInfoComponent);
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
        expect(component.paginator).toBeDefined();
        expect(component.dataSource.paginator).toEqual(component.paginator);

        expect(component.sort).toBeDefined();
        expect(component.dataSource.sort).toEqual(component.sort);
    });

    it('should load admininfos on init', () => {
        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(adminInfos);
    });

    it('should reload admininfos on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(adminInfos);
    });

    it.each([
        {filter: 'Info1'},
        {filter: '20000'},
        {filter: 'description'},
        {filter: 'ript'},
        {filter: '400'},
        {filter: 'test3@test.ch'},
        {filter: 'test4@test.ch'},
        {filter: '22.02.2013'},
        {filter: '22.02.2016'},
        {filter: '2016'},
    ])('should apply filter for $filter and clear filter', async ({filter}) => {
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('can apply empty filter', async () => {
        component.applyFilter({target: {value: null} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: undefined} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('can apply filter and clear filter without paginator', async () => {
        // @ts-ignore
        component.paginator = null;
        component.dataSource.paginator = null;

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: 'Info1'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should not delete admininfo if confirmation is declined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete({
            id: 1,
            code: 'CODE',
            description: 'Description',
            entryCount: 0,
        } as AdminInfoDetail);

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(adminInfoServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should delete admininfo if confirmed', () => {
        jest.clearAllMocks();

        dialogRefMock.afterClosed.mockReturnValue(of(true));
        adminInfoServiceMock.delete.mockReturnValue(of({}));

        const adminInfo = {
            id: 1,
            code: 'CODE',
            description: 'Description',
            entryCount: 0,
        } as AdminInfoDetail;

        component.delete(adminInfo);

        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(adminInfoServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(adminInfoServiceMock.delete).toHaveBeenCalledWith(adminInfo.id);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalled();
        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toBe(adminInfos);
    });

    it('should handle delete admininfo http error', () => {
        jest.clearAllMocks();

        const error = {error: {}};
        dialogRefMock.afterClosed.mockReturnValue(of(true));
        adminInfoServiceMock.delete.mockReturnValue(throwError(() => error));
        const adminInfo = {
            id: 1,
            code: 'CODE',
            description: 'Description',
            entryCount: 0,
        } as AdminInfoDetail;

        component.delete(adminInfo);

        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(adminInfoServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(adminInfoServiceMock.delete).toHaveBeenCalledWith(adminInfo.id);
        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalled();
        expect(adminInfoServiceMock.getAdminInfoDetails).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledWith(error, 'adminInfo.notification.deleteError');
    });
});
