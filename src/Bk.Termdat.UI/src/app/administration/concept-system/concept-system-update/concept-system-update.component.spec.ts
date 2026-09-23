import {AsyncPipe} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {BehaviorSubject, of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {ConceptSystemFormComponent} from '../concept-system-form/concept-system-form.component';
import {ConceptSystemService} from '../concept-system.service';
import {ConceptSystemUpdateComponent} from './concept-system-update.component';

describe('ConceptSystemUpdateComponent', () => {
    let component: ConceptSystemUpdateComponent;
    let fixture: ComponentFixture<ConceptSystemUpdateComponent>;

    const conceptSystemServiceMock = {
        get: jest.fn(),
        update: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    const paramMock = {get: jest.fn()};
    const param$ = new BehaviorSubject(paramMock);
    const routeMock = {
        paramMap: param$.asObservable(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ConceptSystemUpdateComponent, MockComponent(ConceptSystemFormComponent), AsyncPipe],
            providers: [
                {provide: ConceptSystemService, useValue: conceptSystemServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
            ],
        }).compileComponents();

        conceptSystemServiceMock.update.mockReturnValue(of({}));
        conceptSystemServiceMock.get.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);

        fixture = TestBed.createComponent(ConceptSystemUpdateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update on save', () => {
        const form = fixture.debugElement.query(By.directive(ConceptSystemFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(conceptSystemServiceMock.get).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should handle error on save', () => {
        conceptSystemServiceMock.update.mockReturnValue(throwError(() => ({})));

        const form = fixture.debugElement.query(By.directive(ConceptSystemFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(conceptSystemServiceMock.get).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should navigate back when no id-param provided', () => {
        jest.clearAllMocks();

        paramMock.get.mockReturnValue(null);
        param$.next(paramMock);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(conceptSystemServiceMock.get).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });
});
