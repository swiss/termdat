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
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObInputClearModule, ObNotificationService} from '@oblique/oblique';
import {OfficeDetail} from 'api/OfficeDetail';
import {MockComponent, MockDirective, MockModule} from 'ng-mocks';
import {EMPTY, of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {OfficeService} from './office.service';
import {OfficesComponent} from './offices.component';

describe('OfficesComponent', () => {
    let component: OfficesComponent;
    let fixture: ComponentFixture<OfficesComponent>;

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

    let offices: OfficeDetail[] = [];
    const officeServiceMock = {
        getOfficeDetails: jest.fn(),
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
                OfficesComponent,
                MockComponent(ConfirmDeleteDialogComponent),
                MockDirective(RouterLink),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: OfficeService, useValue: officeServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        offices = [
            {
                id: 10000,
                name: 'Office1',
                code: 'OFF1',
                entryCount: 200,
                createdBy: 'test@test.ch',
                modifiedBy: 'test2@test.ch',
                modified: new Date('2013-02-22T08:05:05.78'),
                created: new Date('2015-04-24T08:05:05.78'),
            },
            {
                id: 20000,
                name: 'Office2',
                code: 'OFF2',
                entryCount: 300,
                createdBy: 'test3@test.ch',
                modifiedBy: 'test4@test.ch',
                modified: new Date('2014-02-22T14:05:05.78'),
                created: new Date('2016-02-22T15:05:05.78'),
            },
            {
                id: 30000,
                name: 'Office3',
                code: 'OFF3',
                entryCount: 400,
                createdBy: 'test@test.ch',
                modifiedBy: 'test@test.ch',
                modified: new Date('2014-02-22T11:05:05.78'),
                created: new Date('2014-02-22T11:05:05.78'),
            },
        ];

        officeServiceMock.getOfficeDetails.mockReturnValue(of(offices));
        dialogRefMock.afterClosed.mockReturnValue(of({}));

        fixture = TestBed.createComponent(OfficesComponent);
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

    it('should load offices on init', () => {
        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(offices);
    });

    it('should reload offices on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(offices);
    });

    it.each([
        {filter: 'OFF1'},
        {filter: '20000'},
        {filter: 'office1'},
        {filter: 'ice1'},
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

        component.applyFilter({target: {value: 'office1'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should display warning notification on delete if entryCount is not zero', () => {
        component.delete({entryCount: 1} as OfficeDetail);

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith('office.notification.deleteError');
        expect(dialogMock.open).not.toHaveBeenCalled();
    });

    it('should display confirm dialog on delete if entryCount is zero', () => {
        dialogRefMock.afterClosed.mockReturnValue(EMPTY);

        component.delete({
            id: 1,
            code: 'CODE',
            name: 'Office',
            entryCount: 0,
        } as OfficeDetail);

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
    });

    it('should not delete office if confirmation is declined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete({
            id: 1,
            code: 'CODE',
            name: 'Office',
            entryCount: 0,
        } as OfficeDetail);

        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(officeServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should delete office if confirmed', () => {
        jest.clearAllMocks();

        dialogRefMock.afterClosed.mockReturnValue(of(true));
        officeServiceMock.delete.mockReturnValue(of({}));

        const office = {
            id: 1,
            code: 'CODE',
            name: 'Office',
            entryCount: 0,
        } as OfficeDetail;

        component.delete(office);

        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(officeServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(officeServiceMock.delete).toHaveBeenCalledWith(office.id);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalled();
        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toBe(offices);
    });

    it('should handle delete office http error', () => {
        jest.clearAllMocks();

        const error = {error: {}};
        dialogRefMock.afterClosed.mockReturnValue(of(true));
        officeServiceMock.delete.mockReturnValue(throwError(() => error));
        const office = {
            id: 1,
            code: 'CODE',
            name: 'Office',
            entryCount: 0,
        } as OfficeDetail;

        component.delete(office);

        expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        expect(dialogMock.open).toHaveBeenCalled();
        expect(dialogRefMock.afterClosed).toHaveBeenCalled();
        expect(officeServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(officeServiceMock.delete).toHaveBeenCalledWith(office.id);
        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalled();
        expect(officeServiceMock.getOfficeDetails).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledWith(error, 'office.notification.deleteError');
    });
});
