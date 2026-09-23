import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject, of, throwError} from 'rxjs';
import {ConfirmDeleteDialogComponent} from '../../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {EntryTemplateService} from '../../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../../shared/error.service';
import {EntryTemplateListComponent} from './entry-template-list.component';

describe('EntryTemplateListComponent', () => {
    let component: EntryTemplateListComponent;
    let fixture: ComponentFixture<EntryTemplateListComponent>;

    const entryTemplateServiceMock = {
        update: jest.fn(),
        delete: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    const langChangeSubject = new BehaviorSubject<{lang: string}>({lang: 'de'});
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
    };
    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(() => dialogRefMock),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(() => throwError(() => ({}))),
    };
    const entryTemplate = {
        id: 1,
        templateName: 'test name',
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MatTableModule, MatTooltipModule, MatButtonModule, MockModule(MatExpansionModule), EntryTemplateListComponent, MockPipe(TranslatePipe)],
            providers: [
                {provide: EntryTemplateService, useValue: entryTemplateServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
            ],
        }).compileComponents();

        entryTemplateServiceMock.delete.mockReturnValue(of({}));
        entryTemplateServiceMock.update.mockReturnValue(of({}));

        fixture = TestBed.createComponent(EntryTemplateListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should route on update', () => {
        component.update(1);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['/search/entry', 1, 'template-update']);
    });

    it('should open dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.delete(entryTemplate);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'profile.entryTemplates.delete.title',
                text: 'profile.entryTemplates.delete.text',
                textValue: entryTemplate,
                confirm: 'common.delete',
            },
        });
    });

    it('should delete when confirmed', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));
        jest.spyOn(component.reload, 'next');

        component.delete(entryTemplate);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryTemplateServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'profile.entryTemplates.delete.successTitle',
            message: 'profile.entryTemplates.delete.successMessage',
        });
        expect(component.reload.next).toHaveBeenCalledTimes(1);
    });

    it('should not delete when rejected', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete(entryTemplate);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(entryTemplateServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should handle error on delete', () => {
        entryTemplateServiceMock.delete.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete(entryTemplate);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryTemplateServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });
});
