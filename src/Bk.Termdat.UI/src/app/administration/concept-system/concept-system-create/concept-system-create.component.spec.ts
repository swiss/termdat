import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {ConceptSystemFormComponent} from '../concept-system-form/concept-system-form.component';
import {ConceptSystemService} from '../concept-system.service';
import {ConceptSystemCreateComponent} from './concept-system-create.component';

describe('ConceptSystemCreateComponent', () => {
    let component: ConceptSystemCreateComponent;
    let fixture: ComponentFixture<ConceptSystemCreateComponent>;

    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const conceptSystemServiceMock = {
        create: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ConceptSystemCreateComponent, MockComponent(ConceptSystemFormComponent)],
            providers: [
                {provide: ConceptSystemService, useValue: conceptSystemServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(ConceptSystemCreateComponent);
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

    it('should create on save', () => {
        conceptSystemServiceMock.create.mockReturnValue(of({}));

        const form = fixture.debugElement.query(By.directive(ConceptSystemFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.create).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        conceptSystemServiceMock.create.mockReturnValue(throwError(() => ({})));

        const form = fixture.debugElement.query(By.directive(ConceptSystemFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(conceptSystemServiceMock.create).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
