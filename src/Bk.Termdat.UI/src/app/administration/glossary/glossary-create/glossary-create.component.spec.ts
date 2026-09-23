import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {GlossaryFormComponent} from '../glossary-form/glossary-form.component';
import {GlossaryService} from '../glossary.service';
import {GlossaryCreateComponent} from './glossary-create.component';

describe('GlossaryCreateComponent', () => {
    let component: GlossaryCreateComponent;
    let fixture: ComponentFixture<GlossaryCreateComponent>;

    const glossaryServiceMock = {
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
            imports: [GlossaryCreateComponent, MockComponent(GlossaryFormComponent)],
            providers: [
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        glossaryServiceMock.create.mockReturnValue(of({}));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(GlossaryCreateComponent);
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
        const form = element.query(By.directive(GlossaryFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(glossaryServiceMock.create).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        glossaryServiceMock.create.mockReturnValue(throwError(() => ({})));

        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(GlossaryFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(glossaryServiceMock.create).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
