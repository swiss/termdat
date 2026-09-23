import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatMenuModule} from '@angular/material/menu';
import {MatTableModule} from '@angular/material/table';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {MockComponent, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {SourceTemplatesService} from '../../../entry/entry-form/source-templates/source-templates.service';
import {ConfirmDeleteDialogComponent} from '../../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../../shared/error.service';
import {SourceTemplateFormComponent} from '../source-template-form/source-template-form.component';
import {SourceTemplateListComponent} from './source-template-list.component';

describe('SourceTemplateListComponent', () => {
    let component: SourceTemplateListComponent;
    let fixture: ComponentFixture<SourceTemplateListComponent>;

    const sourceTemplateDto = {
        id: 1,
        textDe: 'Text DE',
        textFr: 'Text FR',
        textIt: 'Text IT',
        textEn: 'Text EN',
        textRm: 'Text RM',
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
    };

    const errorServiceMock = {
        handleHttpError: jest.fn(() => throwError(() => ({error: {}}))),
    };

    const dialogMock = {
        open: jest.fn(() => dialogRefMock),
    };

    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    const sourceTemplatesServiceMock = {
        create: jest.fn(),
        update: jest.fn(),
        delete: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatExpansionModule),
                MockModule(MatIconModule),
                MockModule(MatButtonModule),
                MockModule(MatInputModule),
                MockModule(MatTableModule),
                MockModule(TranslateModule),
                MockModule(MatMenuModule),
                ReactiveFormsModule,
                SourceTemplateListComponent,
                MockComponent(ConfirmDeleteDialogComponent),
                MockComponent(SourceTemplateFormComponent),
            ],
            providers: [
                {provide: MatDialog, useValue: dialogMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: SourceTemplatesService, useValue: sourceTemplatesServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                provideHttpClient(withInterceptorsFromDi()),
            ],
        }).compileComponents();

        sourceTemplatesServiceMock.delete.mockReturnValue(of({}));
        sourceTemplatesServiceMock.create.mockReturnValue(of({}));
        sourceTemplatesServiceMock.update.mockReturnValue(of({}));

        fixture = TestBed.createComponent(SourceTemplateListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create component', () => {
        expect(component).toBeTruthy();
    });

    it('should open dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.delete(sourceTemplateDto);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'profile.sourceTemplates.delete.title',
                text: 'profile.sourceTemplates.delete.text',
                textValue: sourceTemplateDto,
                confirm: 'common.delete',
            },
        });
    });

    it('should delete when confirmed', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));
        jest.spyOn(component.reload, 'next');

        component.delete(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'profile.sourceTemplates.delete.successTitle',
            message: 'profile.sourceTemplates.delete.successMessage',
        });
        expect(component.reload.next).toHaveBeenCalledTimes(1);
    });

    it('should not delete when rejected', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(undefined));

        component.delete(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(sourceTemplatesServiceMock.delete).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should handle error on delete', () => {
        sourceTemplatesServiceMock.delete.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should update', () => {
        jest.spyOn(component.reload, 'next');

        component.update(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.update).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            message: 'profile.sourceTemplates.edit.successMessage',
        });
        expect(component.reload.next).toHaveBeenCalledTimes(1);
    });

    it('should handle error on update', () => {
        sourceTemplatesServiceMock.update.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.update(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.update).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });

    it('should create', () => {
        jest.spyOn(component.reload, 'next');

        component.create(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.create).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            message: 'profile.sourceTemplates.create.successMessage',
        });
        expect(component.reload.next).toHaveBeenCalledTimes(1);
    });

    it('should handle error on create', () => {
        sourceTemplatesServiceMock.create.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.create(sourceTemplateDto);

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(sourceTemplatesServiceMock.create).toHaveBeenCalledTimes(1);
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
    });
});
