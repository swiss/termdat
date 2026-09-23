import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {TermdatClassificationSubjectFormComponent} from '../subject-form/subject-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';
import {TermdatClassificationCreateComponent} from './classification-create.component';

describe('TermdatClassificationCreateComponent', () => {
    let component: TermdatClassificationCreateComponent;
    let fixture: ComponentFixture<TermdatClassificationCreateComponent>;

    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const classificationServiceMock = {
        createSubject: jest.fn(),
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
            imports: [TermdatClassificationCreateComponent, MockComponent(TermdatClassificationSubjectFormComponent)],
            providers: [
                {provide: TermdatClassificationService, useValue: classificationServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(TermdatClassificationCreateComponent);
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
        classificationServiceMock.createSubject.mockReturnValue(of({}));
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(classificationServiceMock.createSubject).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should handle error on save-error', () => {
        classificationServiceMock.createSubject.mockReturnValue(throwError(() => {}));
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(classificationServiceMock.createSubject).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should navigate back on cancel', () => {
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.canceled.emit();

        expect(routerMock.navigate).toHaveBeenCalled();
    });
});
