import {HttpErrorResponse} from '@angular/common/http';
import {TestBed} from '@angular/core/testing';
import {MatDialog, MatDialogRef} from '@angular/material/dialog';
import {Router} from '@angular/router';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {of} from 'rxjs';
import {ErrorService} from './error.service';
import {CONSOLE} from './injection.tokens';

describe('ErrorService', () => {
    let service: ErrorService;

    const translateServiceMock = {
        instant: jest.fn(),
    };
    const notificationServiceMock = {
        error: jest.fn(),
        warning: jest.fn(),
    };
    const consoleMock = {
        error: jest.fn(),
    };
    const matDialogMock = {
        open: jest.fn(),
    };
    const matDialogRefMock = {
        afterClosed: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: MatDialog, useValue: matDialogMock},
                {provide: MatDialogRef, useValue: matDialogRefMock},
                {provide: CONSOLE, useValue: consoleMock},
                {provide: Router, useValue: routerMock},
            ],
        });

        translateServiceMock.instant.mockImplementation(value => value);
        matDialogMock.open.mockReturnValue(matDialogRefMock);
        matDialogRefMock.afterClosed.mockReturnValue(of({}));

        service = TestBed.inject(ErrorService);
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('can handle http errors', () => {
        const errors = {
            status: 400,
            error: {
                errors: {
                    test1: 'message1',
                    test2: ['message2', 'message3'],
                },
            },
        };

        service.handleHttpError(errors as HttpErrorResponse, 'my-test-key');

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(3);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({
            message: 'message1',
            sticky: false,
            timeout: 5000,
        });
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({
            message: 'message2',
            sticky: false,
            timeout: 5000,
        });
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({
            message: 'message3',
            sticky: false,
            timeout: 5000,
        });
        expect(translateServiceMock.instant).not.toHaveBeenCalled();
    });

    it('can handle http errors with unknown structure', () => {
        const errors = {
            status: 400,
            error: {
                test1: 'message1',
                test2: ['message2', 'message3'],
            },
        };

        service.handleHttpError(errors as HttpErrorResponse, 'my-test-key');

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({
            message: 'my-test-key',
            sticky: false,
            timeout: 5000,
        });
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(1);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('my-test-key');
    });

    it('can handle http errors with null', () => {
        const errors = {
            status: 400,
            error: null,
        };

        service.handleHttpError(errors as HttpErrorResponse, 'my-test-key');

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.warning).toHaveBeenCalledWith({
            message: 'my-test-key',
            sticky: false,
            timeout: 5000,
        });
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(1);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('my-test-key');
    });

    it('should get control error', () => {
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(0);

        expect(service.getControlError(null, 'test.label', 'test.prefix')).toEqual('');
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(0);

        expect(service.getControlError({error: true, 'another-error': false}, 'test.label', 'test.prefix')).toEqual('test.prefix.error');
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(1);
        expect(translateServiceMock.instant).toHaveBeenLastCalledWith('test.prefix.error', {field: 'test.label'});
    });

    it('redirects to unauthorized for errors with statusCode 403', () => {
        const httpErrorResponse = {
            status: 403,
        } as HttpErrorResponse;

        service.handleHttpError(httpErrorResponse, 'test');

        expect(routerMock.navigate).toHaveBeenCalledWith(['/unauthorized']);
        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(0);
    });
});
