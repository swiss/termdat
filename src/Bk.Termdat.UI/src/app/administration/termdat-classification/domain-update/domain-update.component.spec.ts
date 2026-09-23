import {AsyncPipe} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {BehaviorSubject, of, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {ErrorService} from '../../../shared/error.service';
import {TermdatClassificationSubjectFormComponent} from '../subject-form/subject-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';
import {TermdatClassificationDomainUpdateComponent} from './domain-update.component';

describe('TermdatClassificationDomainUpdateComponent', () => {
    let component: TermdatClassificationDomainUpdateComponent;
    let fixture: ComponentFixture<TermdatClassificationDomainUpdateComponent>;

    const classificationServiceMock = {
        getSubject: jest.fn(),
        updateDomain: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const httpInterceptorEventsMock = {
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
            imports: [TermdatClassificationDomainUpdateComponent, MockComponent(TermdatClassificationSubjectFormComponent), AsyncPipe],
            providers: [
                {provide: TermdatClassificationService, useValue: classificationServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
            ],
        }).compileComponents();

        classificationServiceMock.getSubject.mockReturnValue(of({domains: []}));
        classificationServiceMock.updateDomain.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(11);

        fixture = TestBed.createComponent(TermdatClassificationDomainUpdateComponent);
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

    it('should navigate back when no id-param provided', () => {
        jest.clearAllMocks();

        paramMock.get.mockReturnValue(null);
        param$.next(paramMock);

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(classificationServiceMock.getSubject).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should load subject', () => {
        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(classificationServiceMock.getSubject).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should handle error on load-error', async () => {
        jest.clearAllMocks();

        classificationServiceMock.getSubject.mockReturnValue(throwError(() => ({})));

        fixture = TestBed.createComponent(TermdatClassificationDomainUpdateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        await fixture.whenStable();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalled();
        expect(classificationServiceMock.getSubject).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalled();
    });

    it('should navigate back on cancel', () => {
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;
        form.canceled.emit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should update on save', () => {
        jest.clearAllMocks();
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({domains: [{}]});

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(classificationServiceMock.updateDomain).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should not update when no domains', () => {
        jest.clearAllMocks();
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({domains: []});

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(classificationServiceMock.updateDomain).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should not update when more than one domain', () => {
        jest.clearAllMocks();
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({domains: [{}, {}]});

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(classificationServiceMock.updateDomain).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should handle error on save-error', () => {
        jest.clearAllMocks();
        classificationServiceMock.updateDomain.mockReturnValue(throwError(() => ({})));
        const form = fixture.debugElement.query(By.directive(TermdatClassificationSubjectFormComponent)).componentInstance;

        form.saved.emit({domains: [{}]});

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(classificationServiceMock.updateDomain).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(extendedSearchFiltersServiceMock.clear).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should navigate back', () => {
        component.back();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });
});
