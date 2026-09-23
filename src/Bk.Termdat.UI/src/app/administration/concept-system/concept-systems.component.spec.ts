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
import {ConceptSystemDetail} from 'api/ConceptSystemDetail';
import {MockDirective, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {ConceptSystemService} from './concept-system.service';
import {ConceptSystemsComponent} from './concept-systems.component';

describe('ConcepSystemsComponent', () => {
    let component: ConceptSystemsComponent;
    let fixture: ComponentFixture<ConceptSystemsComponent>;

    const routerMock = {
        navigate: jest.fn(),
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    let conceptSystems: ConceptSystemDetail[] = [];
    const conceptSystemServiceMock = {
        getConceptSystemDetails: jest.fn(),
        delete: jest.fn(),
    };

    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };

    const notificationServiceMock = {
        success: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };

    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
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
                MockDirective(RouterLink),
                ConceptSystemsComponent,
            ],
            providers: [
                {provide: Router, useValue: routerMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ConceptSystemService, useValue: conceptSystemServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        conceptSystems = [
            {
                id: 10000,
                title: 'ConceptSystem1',
                collection: 'code1',
                createdBy: 'test@test.ch',
                entryCount: 0,
                modifiedBy: 'test2@test.ch',
                modified: new Date('2013-02-22T23:05:05.78'),
                created: new Date('2015-02-22T23:05:05.78'),
            },
            {
                id: 20000,
                title: 'ConceptSystem2',
                collection: 'code2',
                createdBy: 'test3@test.ch',
                entryCount: 1,
                modifiedBy: 'test4@test.ch',
                modified: new Date('2014-02-22T23:05:05.78'),
                created: new Date('2016-02-22T23:05:05.78'),
            },
            {
                id: 30000,
                title: 'ConceptSystem3',
                collection: 'code3',
            },
        ] as ConceptSystemDetail[];

        conceptSystemServiceMock.getConceptSystemDetails.mockReturnValue(of(conceptSystems));
        conceptSystemServiceMock.delete.mockReturnValue(of({}));
        dialogMock.open.mockReturnValue(dialogRefMock);

        fixture = TestBed.createComponent(ConceptSystemsComponent);
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

    it('should load concept systems on init', () => {
        expect(conceptSystemServiceMock.getConceptSystemDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(conceptSystems);
    });

    it('should reload concept systems on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(conceptSystemServiceMock.getConceptSystemDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(conceptSystems);
    });

    it.each([
        {filter: 'code1'},
        {filter: '20000'},
        {filter: 'ConceptSystem1'},
        {filter: 'ystem1'},
        {filter: '10000'},
        {filter: 'test@test.ch'},
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

        component.applyFilter({target: {value: 'ConceptSystem1'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should navigate on update', () => {
        component.update(conceptSystems[0]);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['administration', 'conceptSystem', 10000]);
    });

    it('should not delete when concept system is in use', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete(conceptSystems[1]);

        expect(dialogMock.open).not.toHaveBeenCalled();
        expect(conceptSystemServiceMock.delete).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should open dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.delete(conceptSystems[0]);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'conceptSystem.delete.title',
                text: 'conceptSystem.delete.text',
                textValue: {...conceptSystems[0]},
                confirm: 'conceptSystem.delete.confirm',
            },
        });
    });

    it('should delete when confirmed', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete(conceptSystems[0]);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.delete).toHaveBeenCalledWith(10000);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.getConceptSystemDetails).toHaveBeenCalledTimes(2);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'conceptSystem.delete.successTitle',
            message: 'conceptSystem.delete.successMessage',
        });
    });

    it('should not delete when rejected', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete(conceptSystems[0]);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(conceptSystemServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(conceptSystemServiceMock.getConceptSystemDetails).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should handle error on delete', () => {
        conceptSystemServiceMock.delete.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete(conceptSystems[2]);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.delete).toHaveBeenCalledWith(30000);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(conceptSystemServiceMock.getConceptSystemDetails).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });
});
