import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {AdminInfoFormComponent} from '../admin-info-form/admin-info-form.component';
import {AdminInfoService} from '../admin-info.service';
import {AdminInfoCreateComponent} from './admin-info-create.component';

describe('AdminInfoCreateComponent', () => {
    let component: AdminInfoCreateComponent;
    let fixture: ComponentFixture<AdminInfoCreateComponent>;
    const adminInfoServiceMock = {
        create: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AdminInfoCreateComponent, MockComponent(AdminInfoFormComponent)],
            providers: [
                {provide: AdminInfoService, useValue: adminInfoServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        adminInfoServiceMock.create.mockReturnValue(of({}));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(AdminInfoCreateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should create on save', () => {
        const form = fixture.debugElement.query(By.directive(AdminInfoFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(adminInfoServiceMock.create).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should handle error on save', () => {
        adminInfoServiceMock.create.mockReturnValue(throwError(() => ({})));

        const form = fixture.debugElement.query(By.directive(AdminInfoFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(adminInfoServiceMock.create).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
