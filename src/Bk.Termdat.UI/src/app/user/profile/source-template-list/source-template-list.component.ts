import {Component, EventEmitter, Input, Output} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from '@angular/material/expansion';
import {
    MatCell,
    MatCellDef,
    MatColumnDef,
    MatHeaderCell,
    MatHeaderCellDef,
    MatHeaderRow,
    MatHeaderRowDef,
    MatRow,
    MatRowDef,
    MatTable,
} from '@angular/material/table';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {SourceTemplateModification} from 'api/SourceTemplateModification';
import {SourceTemplateSummary} from 'api/SourceTemplateSummary';
import {EMPTY} from 'rxjs';
import {catchError, filter, switchMap, tap} from 'rxjs/operators';
import {SourceTemplatesService} from '../../../../app/entry/entry-form/source-templates/source-templates.service';
import {ErrorService} from '../../../../app/shared/error.service';
import {ConfirmDeleteDialogComponent} from '../../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {TdMatTableDataSource} from '../../../shared/td-mat-table-data-source';
import {SourceTemplateFormComponent} from '../source-template-form/source-template-form.component';

@Component({
    selector: 'td-source-template-list',
    templateUrl: './source-template-list.component.html',
    styleUrls: ['./source-template-list.component.scss'],
    imports: [
        MatExpansionPanel,
        MatExpansionPanelHeader,
        MatExpansionPanelTitle,
        MatTable,
        MatColumnDef,
        MatHeaderCellDef,
        MatHeaderCell,
        MatTooltip,
        MatCellDef,
        MatCell,
        SourceTemplateFormComponent,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        TranslatePipe,
    ],
})
export class SourceTemplateListComponent {
    @Output() readonly reload = new EventEmitter<void>();
    dataSource: TdMatTableDataSource<SourceTemplateSummary> = new TdMatTableDataSource();
    displayedColumns: string[] = ['displayText', 'action'];

    private _sourceTemplates: SourceTemplateSummary[] | undefined;

    @Input()
    set sourceTemplates(value: SourceTemplateSummary[] | undefined) {
        this._sourceTemplates = value;
        if (this._sourceTemplates !== undefined) {
            this._sourceTemplates.forEach(sourceTemplate => {
                sourceTemplate.displayText = sourceTemplate.textDe
                    ? sourceTemplate.textDe
                    : sourceTemplate.textFr
                      ? sourceTemplate.textFr
                      : sourceTemplate.textIt
                        ? sourceTemplate.textIt
                        : sourceTemplate.textRm
                          ? sourceTemplate.textRm
                          : sourceTemplate.textEn
                            ? sourceTemplate.textEn
                            : '';
            });
            this.dataSource.data = this._sourceTemplates;
        }
    }

    constructor(
        private readonly dialog: MatDialog,
        private readonly sourceTemplatesService: SourceTemplatesService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService
    ) {}

    delete(template: SourceTemplateSummary) {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'profile.sourceTemplates.delete.title',
                    text: 'profile.sourceTemplates.delete.text',
                    textValue: template,
                    confirm: 'common.delete',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                tap(() => this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1)),
                switchMap(() =>
                    this.sourceTemplatesService.delete(template.id!).pipe(
                        catchError(error => {
                            this.errorService.handleHttpError(error, 'profile.sourceTemplates.notification.error.delete');
                            return EMPTY;
                        })
                    )
                )
            )
            .subscribe(() => {
                this.notificationService.success({
                    title: 'profile.sourceTemplates.delete.successTitle',
                    message: 'profile.sourceTemplates.delete.successMessage',
                });
                this.reload.next();
            });
    }

    create(sourceTemplate: SourceTemplateModification) {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

        this.sourceTemplatesService
            .create(sourceTemplate)
            .pipe(
                catchError(error => {
                    this.errorService.handleHttpError(error, 'profile.sourceTemplates.notification.error.create');
                    return EMPTY;
                })
            )
            .subscribe(() => {
                this.notificationService.success({
                    message: 'profile.sourceTemplates.create.successMessage',
                });

                this.reload.next();
            });
    }

    update(sourceTemplate: SourceTemplateModification) {
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

        this.sourceTemplatesService
            .update(sourceTemplate)
            .pipe(
                catchError(error => {
                    this.errorService.handleHttpError(error, 'profile.sourceTemplates.notification.error.edit');
                    return EMPTY;
                })
            )
            .subscribe(() => {
                this.notificationService.success({
                    message: 'profile.sourceTemplates.edit.successMessage',
                });

                this.reload.next();
            });
    }
}
