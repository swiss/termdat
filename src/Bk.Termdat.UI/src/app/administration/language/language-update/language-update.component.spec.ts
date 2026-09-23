import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, Router} from '@angular/router';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent} from 'ng-mocks';
import {of, throwError} from 'rxjs';
import {ErrorService} from '../../../shared/error.service';
import {LanguageService} from '../../../shared/language.service';
import {LanguageFormComponent} from '../language-form/language-form.component';
import {LanguageUpdateComponent} from './language-update.component';

describe('LanguageUpdateComponent', () => {
    let component: LanguageUpdateComponent;
    let fixture: ComponentFixture<LanguageUpdateComponent>;

    const languageServiceMock = {
        get: jest.fn(),
        update: jest.fn(),
        updateLanguageSummaries: jest.fn(),
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

    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [LanguageUpdateComponent, MockComponent(LanguageFormComponent)],
            providers: [
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        languageServiceMock.update.mockReturnValue(of({}));
        languageServiceMock.get.mockReturnValue(of({}));
        paramMock.get.mockReturnValue(1);
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(LanguageUpdateComponent);
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
        const form = element.query(By.directive(LanguageFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(languageServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(languageServiceMock.updateLanguageSummaries).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on save', () => {
        languageServiceMock.update.mockReturnValue(throwError(() => ({})));

        const element: DebugElement = fixture.debugElement;
        const form = element.query(By.directive(LanguageFormComponent)).componentInstance;
        form.saved.emit({});

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(2);
        expect(languageServiceMock.update).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(routerMock.navigate).not.toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });
});
