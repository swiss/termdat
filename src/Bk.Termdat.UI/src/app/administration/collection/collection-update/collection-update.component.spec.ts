import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {CollectionFormComponent} from '../collection-form/collection-form.component';
import {CollectionService} from '../collection.service';
import {CollectionUpdateComponent} from './collection-update.component';

describe('CollectionUpdateComponent', () => {
    let component: CollectionUpdateComponent;
    let fixture: ComponentFixture<CollectionUpdateComponent>;

    const collectionServiceMock = {
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
            imports: [CollectionUpdateComponent, MockComponent(CollectionFormComponent)],
            providers: [
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
            ],
        }).compileComponents();

        collectionServiceMock.update.mockReturnValue(of({}));
        collectionServiceMock.get.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(CollectionUpdateComponent);
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
        const form = element.query(By.directive(CollectionFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(collectionServiceMock.update).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        collectionServiceMock.update.mockReturnValue(throwError(() => ({})));

        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(CollectionFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(collectionServiceMock.update).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
