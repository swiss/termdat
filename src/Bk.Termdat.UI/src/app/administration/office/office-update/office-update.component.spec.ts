import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {OfficeFormComponent} from '../office-form/office-form.component';
import {OfficeService} from '../office.service';
import {OfficeUpdateComponent} from './office-update.component';

describe('OfficeUpdateComponent', () => {
    let component: OfficeUpdateComponent;
    let fixture: ComponentFixture<OfficeUpdateComponent>;

    const officeServiceMock = {
        get: jest.fn(),
        update: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
        paramMap: jest.fn(),
    };
    const paramMock = {get: jest.fn()};
    const routeMock = {
        paramMap: of(paramMock),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [OfficeUpdateComponent, MockComponent(OfficeFormComponent)],
            providers: [
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: OfficeService, useValue: officeServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        officeServiceMock.get.mockReturnValue(of({}));
        officeServiceMock.update.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(OfficeUpdateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
    it('should update on save', () => {
        const form = fixture.debugElement.query(By.directive(OfficeFormComponent)).componentInstance;
        form.saved.emit({});

        expect(officeServiceMock.update).toHaveBeenCalledTimes(1);
        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on update', () => {
        officeServiceMock.update.mockReturnValue(throwError(() => ({})));

        const form = fixture.debugElement.query(By.directive(OfficeFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(officeServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
