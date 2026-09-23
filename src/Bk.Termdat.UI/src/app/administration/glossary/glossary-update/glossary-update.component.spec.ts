import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {GlossaryFormComponent} from '../glossary-form/glossary-form.component';
import {GlossaryService} from '../glossary.service';
import {GlossaryUpdateComponent} from './glossary-update.component';

describe('GlossaryUpdateComponent', () => {
    let component: GlossaryUpdateComponent;
    let fixture: ComponentFixture<GlossaryUpdateComponent>;

    const glossaryServiceMock = {
        get: jest.fn(),
        update: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
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

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [GlossaryUpdateComponent, MockComponent(GlossaryFormComponent)],
            providers: [
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
            ],
        }).compileComponents();
        glossaryServiceMock.update.mockReturnValue(of({}));
        glossaryServiceMock.get.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(GlossaryUpdateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update on save', () => {
        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(GlossaryFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(glossaryServiceMock.update).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        glossaryServiceMock.update.mockReturnValue(throwError(() => ({})));

        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(GlossaryFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(glossaryServiceMock.update).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
