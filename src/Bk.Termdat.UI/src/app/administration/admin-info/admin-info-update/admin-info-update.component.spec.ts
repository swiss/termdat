import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {AdminInfoFormComponent} from '../admin-info-form/admin-info-form.component';
import {AdminInfoService} from '../admin-info.service';
import {AdminInfoUpdateComponent} from './admin-info-update.component';

describe('AdminInfoUpdateComponent', () => {
    let component: AdminInfoUpdateComponent;
    let fixture: ComponentFixture<AdminInfoUpdateComponent>;

    const adminInfoServiceMock = {
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
            imports: [AdminInfoUpdateComponent, MockComponent(AdminInfoFormComponent)],
            providers: [
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: AdminInfoService, useValue: adminInfoServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        adminInfoServiceMock.get.mockReturnValue(of({}));
        adminInfoServiceMock.update.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(AdminInfoUpdateComponent);
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
        const form = fixture.debugElement.query(By.directive(AdminInfoFormComponent)).componentInstance;
        form.saved.emit({});

        expect(adminInfoServiceMock.update).toHaveBeenCalledTimes(1);
        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on update', () => {
        adminInfoServiceMock.update.mockReturnValue(throwError(() => ({})));

        const form = fixture.debugElement.query(By.directive(AdminInfoFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(adminInfoServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
