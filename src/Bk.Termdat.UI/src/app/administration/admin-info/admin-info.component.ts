import {DatePipe} from '@angular/common';
import {AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort, MatSortHeader} from '@angular/material/sort';
import {
    MatCell,
    MatCellDef,
    MatColumnDef,
    MatHeaderCell,
    MatHeaderCellDef,
    MatHeaderRow,
    MatHeaderRowDef,
    MatNoDataRow,
    MatRow,
    MatRowDef,
    MatTable,
} from '@angular/material/table';
import {MatTooltip} from '@angular/material/tooltip';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObInputClearDirective, ObNotificationService} from '@oblique/oblique';
import {AdminInfoDetail} from 'api/AdminInfoDetail';
import {merge, Subject} from 'rxjs';
import {catchError, filter, finalize, switchMap, takeUntil} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmData} from '../../shared/confirm-delete-dialog/confirm-data';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {AdminInfoService} from './admin-info.service';

@Component({
    selector: 'td-admin-info',
    templateUrl: './admin-info.component.html',
    styleUrls: ['./admin-info.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        RouterLink,
        MatIcon,
        MatFormField,
        MatLabel,
        MatInput,
        ObInputClearDirective,
        MatPaginator,
        MatTable,
        MatSort,
        MatColumnDef,
        MatHeaderCellDef,
        MatHeaderCell,
        MatSortHeader,
        MatTooltip,
        MatCellDef,
        MatCell,
        MatIconButton,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        MatNoDataRow,
        DatePipe,
        TranslatePipe,
    ],
})
export class AdminInfoComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    displayedColumns: string[] = ['id', 'code', 'description', 'entryCount', 'created', 'createdBy', 'modified', 'modifiedBy', 'action'];
    dataSource: TdMatTableDataSource<AdminInfoDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly adminInfoService: AdminInfoService,
        private readonly translateService: TranslateService,
        private readonly errorService: ErrorService,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService,
        private readonly router: Router,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.adminInfoService.getAdminInfoDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(adminInfos => {
                this.dataSource.data = adminInfos;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.code.toLocaleLowerCase().includes(search) ||
                data.entryCount.toString().includes(search) ||
                data.description?.toLocaleLowerCase().includes(search) ||
                data.created?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                data.createdBy?.toLocaleLowerCase().includes(search) ||
                data.modified?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                data.modifiedBy?.toLocaleLowerCase().includes(search)
            );
        };
        this.reload.next();
    }

    ngAfterViewInit(): void {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement)?.value;
        this.dataSource.filter = filterValue?.trim() ?? '';

        this.dataSource.paginator?.firstPage();
    }

    clearFilter(): void {
        this.dataSource.filter = '';

        this.dataSource.paginator?.firstPage();
    }

    update(adminInfo: AdminInfoDetail): void {
        void this.router.navigate(['administration', 'adminInfo', adminInfo.id]);
    }

    delete(adminInfo: AdminInfoDetail): void {
        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'adminInfo.delete.title',
                    text: 'adminInfo.delete.text',
                    textValue: adminInfo,
                    confirm: 'adminInfo.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => this.adminInfoService.delete(adminInfo.id)),
                catchError(error => this.errorService.handleHttpError(error, 'adminInfo.notification.deleteError')),
                finalize(() => {
                    this.reload.next();
                    this.extendedSearchFilterService.clear();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'adminInfo.delete.successTitle',
                    message: 'adminInfo.delete.successMessage',
                })
            );
    }
}
