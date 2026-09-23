import {Component, OnInit} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle} from '@angular/material/expansion';
import {MatIcon} from '@angular/material/icon';
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
import {Router} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {EntryTemplateSummary} from 'api/EntryTemplateSummary';
import {EMPTY, merge, Subject, switchMap, takeUntil} from 'rxjs';
import {catchError, filter, tap} from 'rxjs/operators';
import {ConfirmDeleteDialogComponent} from '../../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {EntryTemplateService} from '../../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../../shared/error.service';
import {TdMatTableDataSource} from '../../../shared/td-mat-table-data-source';

@Component({
    selector: 'td-entry-template-list',
    templateUrl: './entry-template-list.component.html',
    styleUrls: ['./entry-template-list.component.scss'],
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
        MatIconButton,
        ObButtonDirective,
        MatIcon,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        TranslatePipe,
    ],
})
export class EntryTemplateListComponent implements OnInit {
    displayedColumns: string[] = ['templateName', 'action'];
    dataSource: TdMatTableDataSource<EntryTemplateSummary> = new TdMatTableDataSource();
    entryTemplates!: EntryTemplateSummary[];
    readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly translateService: TranslateService,
        private readonly entryTemplateService: EntryTemplateService,
        private readonly router: Router,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.entryTemplateService.getEntryTemplates()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(entryTemplates => {
                this.dataSource.data = entryTemplates;
                this.entryTemplates = entryTemplates;
            });

        this.reload.next();
    }

    update(id: number): void {
        void this.router.navigate(['/search/entry', id, 'template-update']);
    }

    delete(template: EntryTemplateSummary): void {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'profile.entryTemplates.delete.title',
                    text: 'profile.entryTemplates.delete.text',
                    textValue: template,
                    confirm: 'common.delete',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                tap(() => this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1)),
                switchMap(() =>
                    this.entryTemplateService.delete(template.id).pipe(
                        catchError(error => {
                            this.errorService.handleHttpError(error, 'profile.entryTemplates.notification.error.delete');
                            return EMPTY;
                        })
                    )
                )
            )
            .subscribe(() => {
                this.notificationService.success({
                    title: 'profile.entryTemplates.delete.successTitle',
                    message: 'profile.entryTemplates.delete.successMessage',
                });
                this.reload.next();
            });
    }
}
