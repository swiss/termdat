import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {CollectionFormComponent} from '../collection-form/collection-form.component';
import {CollectionService} from '../collection.service';
import {CollectionCreateComponent} from './collection-create.component';

describe('CollectionsCreateComponent', () => {
    let component: CollectionCreateComponent;
    let fixture: ComponentFixture<CollectionCreateComponent>;

    const collectionServiceMock = {
        create: jest.fn(),
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
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [CollectionCreateComponent, MockComponent(CollectionFormComponent)],
            providers: [
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        collectionServiceMock.create.mockReturnValue(of({}));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(CollectionCreateComponent);
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
        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(CollectionFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.create).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        collectionServiceMock.create.mockReturnValue(throwError(() => ({})));

        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(CollectionFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.create).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
