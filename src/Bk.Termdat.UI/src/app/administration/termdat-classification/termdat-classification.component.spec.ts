import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule, SortDirection} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObInputClearModule, ObNotificationService} from '@oblique/oblique';
import {TermdatClassificationDetail} from 'api/TermdatClassificationDetail';
import {MockDirective, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ErrorService} from '../../shared/error.service';
import {TermdatClassificationComponent} from './termdat-classification.component';
import {TermdatClassificationService} from './termdat-classification.service';

describe('TermdatClassificationComponent', () => {
    let component: TermdatClassificationComponent;
    let fixture: ComponentFixture<TermdatClassificationComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const notificationServiceMock = {
        warning: jest.fn(),
        success: jest.fn(),
    };

    const errorServiceMock = {
        handleHttpError: jest.fn(() => throwError(() => ({error: {}}))),
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(() => dialogRefMock),
    };

    let classifications: TermdatClassificationDetail[] = [];
    const termdatClassificationServiceMock = {
        getSubjectDetails: jest.fn(),
        deleteSubject: jest.fn(() => of({})),
        deleteDomain: jest.fn(() => of({})),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    const changeTracking = {
        createdBy: 'test@test1.ch',
        modifiedBy: 'test@test1.ch',
        modified: new Date('2010-02-22T08:05:05.78'),
        created: new Date('2011-04-24T08:05:05.78'),
    };

    const httpInterceptorEventsMock = {
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
                TermdatClassificationComponent,
                MockDirective(RouterLink),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: TermdatClassificationService, useValue: termdatClassificationServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
            ],
        }).compileComponents();

        classifications = [
            {
                id: 1,
                subjectName: 'Arbeit',
                domainId: null,
                domainName: '',
                entryCount: 100,
                createdBy: 'test@test1.ch',
                modifiedBy: 'test@test1.ch',
                modified: new Date('2010-02-22T08:05:05.78'),
                created: new Date('2011-04-24T08:05:05.78'),
            } as unknown as TermdatClassificationDetail,
            {
                id: 1,
                subjectName: 'Wörk',
                domainId: 100001,
                domainName: 'Domain11',
                entryCount: 105,
                createdBy: 'test@test2.ch',
                modifiedBy: 'test@test2.ch',
                modified: new Date('2012-02-22T08:05:05.78'),
                created: new Date('2013-04-24T08:05:05.78'),
            },
            {
                id: 1,
                subjectName: 'Öffentlicher Verkehr',
                domainId: 100002,
                domainName: 'Domain12',
                entryCount: 12,
                createdBy: 'test@test3.ch',
                modifiedBy: 'test@test3.ch',
                modified: new Date('2014-02-22T08:05:05.78'),
                created: new Date('2015-04-24T08:05:05.78'),
            },
            {
                id: 2,
                subjectName: 'Subject2',
                entryCount: 55,
                createdBy: 'test@test4.ch',
                modifiedBy: 'test@test4.ch',
                modified: new Date('2016-02-22T08:05:05.78'),
                created: new Date('2017-04-24T08:05:05.78'),
                domains: [],
                domainDetails: [],
            } as unknown as TermdatClassificationDetail,
            {
                id: 2,
                subjectName: 'Subject2',
                domainId: 100003,
                domainName: 'Domain13',
                entryCount: 50,
                createdBy: 'test@test5.ch',
                modifiedBy: 'test@test5.ch',
                modified: new Date('2018-02-22T08:05:05.78'),
                created: new Date('2019-04-24T08:05:05.78'),
            },
            {
                id: 3,
                subjectName: 'Subject3',
                domainId: 100004,
                domainName: 'Domain16',
                entryCount: 101,
                createdBy: 'test@test6.ch',
                modifiedBy: 'test@test6.ch',
                modified: new Date('2020-02-22T08:05:05.78'),
                created: new Date('2021-04-24T08:05:05.78'),
            },
        ];

        termdatClassificationServiceMock.getSubjectDetails.mockReturnValue(of(classifications));
        dialogRefMock.afterClosed.mockReturnValue(of({}));

        fixture = TestBed.createComponent(TermdatClassificationComponent);
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

    it.each([
        {order: 'asc', first: 'Arbeit', last: 'Wörk'},
        {order: 'desc', first: 'Wörk', last: 'Arbeit'},
    ])('should order subjects by name', async ({order, first, last}) => {
        expect(component.dataSource.data).toEqual(classifications);
        component.sort.direction = order as SortDirection;
        component.sort.active = 'subjectName';
        component.sort.sortChange.emit();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.data[0].subjectName).toEqual(first);
        expect(component.dataSource.data[component.dataSource.data.length - 1].subjectName).toEqual(last);
    });

    it.each([
        {order: 'asc', first: 1, last: 3},
        {order: 'desc', first: 3, last: 1},
    ])('should order subjects by id', async ({order, first, last}) => {
        expect(component.dataSource.data).toEqual(classifications);
        component.sort.direction = order as SortDirection;
        component.sort.active = 'id';
        component.sort.sortChange.emit();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.data[0].id).toEqual(first);
        expect(component.dataSource.data[component.dataSource.data.length - 1].id).toEqual(last);
    });

    it('should load termdat classification on init', async () => {
        expect(termdatClassificationServiceMock.getSubjectDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(classifications);
    });

    it('should reload termdat classification on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(termdatClassificationServiceMock.getSubjectDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(classifications);
    });

    it.each([
        {filter: 'Subject3'},
        {filter: '004'},
        {filter: '100001'},
        {filter: 'main13'},
        {filter: 'Domain16'},
        {filter: 'test4'},
        {filter: 'test@test6.ch'},
        {filter: 'test@test5.ch'},
        {filter: '22.02.2018'},
        {filter: '24.04.2021'},
        {filter: '2016'},
    ])('should apply filter for $filter and clear filter', async ({filter}) => {
        expect(component.dataSource.filteredData.length).toBe(6);

        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(6);
    });

    it('can apply empty filter', async () => {
        component.applyFilter({target: {value: null} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(6);

        component.applyFilter({target: {value: undefined} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(6);
    });

    it('can apply filter and clear filter without paginator', async () => {
        // @ts-ignore
        component.paginator = null;
        component.dataSource.paginator = null;

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(6);

        component.applyFilter({target: {value: 'subject3'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(6);
    });

    it('should call subject detail route', () => {
        const data = {id: 1, domainId: 4} as TermdatClassificationDetail;

        component.updateTermdatClassification(data);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['administration', 'termdatClassification', 'subject', data.id, 'domain', data.domainId]);
    });

    it('should call domain detail route', () => {
        const data = {id: 42} as TermdatClassificationDetail;

        component.updateTermdatClassification(data);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['administration', 'termdatClassification', 'subject', data.id]);
    });

    it('should delete subject', async () => {
        const tClassificationSubject: TermdatClassificationDetail = {
            id: 1,
            subjectName: 'Subject',
            entryCount: 0,
            ...changeTracking,
        };

        component.deleteTermdatClassification(tClassificationSubject);

        await fixture.whenStable();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(termdatClassificationServiceMock.deleteSubject).toHaveBeenCalled();
        expect(termdatClassificationServiceMock.deleteDomain).not.toHaveBeenCalled();
    });

    it('should delete domain', async () => {
        const tClassification: TermdatClassificationDetail = {
            id: 1,
            subjectName: 'Subject',
            domainId: 100,
            domainName: 'domain',
            entryCount: 0,
            ...changeTracking,
        };

        component.deleteTermdatClassification(tClassification);

        await fixture.whenStable();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(termdatClassificationServiceMock.deleteSubject).not.toHaveBeenCalled();
        expect(termdatClassificationServiceMock.deleteDomain).toHaveBeenCalled();
    });

    it('should warn if tried to delete classification with entries', async () => {
        const tClassification: TermdatClassificationDetail = {
            id: 1,
            subjectName: 'Subject',
            domainId: 100,
            domainName: 'domain',
            entryCount: 1,
            ...changeTracking,
        };

        component.deleteTermdatClassification(tClassification);
        await fixture.whenStable();

        expect(notificationServiceMock.warning).toHaveBeenCalled();
        expect(termdatClassificationServiceMock.deleteDomain).not.toHaveBeenCalled();
        expect(termdatClassificationServiceMock.deleteSubject).not.toHaveBeenCalled();
    });

    it('should handle http error on delete domain', async () => {
        termdatClassificationServiceMock.deleteDomain.mockReturnValueOnce(throwError(() => 'Error'));
        const tClassification: TermdatClassificationDetail = {
            id: 1,
            subjectName: 'Subject',
            domainId: 100,
            domainName: 'domain',
            entryCount: 0,
            ...changeTracking,
        };

        component.deleteTermdatClassification(tClassification);
        await fixture.whenStable();

        expect(termdatClassificationServiceMock.deleteDomain).toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalled();
    });

    it('should handle http error on delete subject', async () => {
        termdatClassificationServiceMock.deleteSubject.mockReturnValueOnce(throwError(() => 'Error'));
        const tClassification: TermdatClassificationDetail = {
            id: 1,
            subjectName: 'Subject',
            entryCount: 0,
            ...changeTracking,
        };

        component.deleteTermdatClassification(tClassification);

        expect(termdatClassificationServiceMock.deleteSubject).toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalled();
    });
});
