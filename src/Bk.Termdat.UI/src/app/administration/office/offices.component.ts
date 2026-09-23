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
import {OfficeDetail} from 'api/OfficeDetail';
import {merge, Subject} from 'rxjs';
import {catchError, filter, finalize, switchMap, takeUntil} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmData} from '../../shared/confirm-delete-dialog/confirm-data';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {OfficeService} from './office.service';

@Component({
    selector: 'td-offices',
    templateUrl: './offices.component.html',
    styleUrls: ['./offices.component.scss'],
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
export class OfficesComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    displayedColumns: string[] = ['id', 'code', 'name', 'entryCount', 'created', 'createdBy', 'modified', 'modifiedBy', 'action'];
    dataSource: TdMatTableDataSource<OfficeDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly officeService: OfficeService,
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
                switchMap(() => this.officeService.getOfficeDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(offices => {
                this.dataSource.data = offices;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.code.toLocaleLowerCase().includes(search) ||
                data.entryCount.toString().includes(search) ||
                data.name.toLocaleLowerCase().includes(search) ||
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

    update(office: OfficeDetail): void {
        void this.router.navigate(['administration', 'office', office.id]);
    }

    delete(office: OfficeDetail): void {
        if (office.entryCount > 0) {
            this.notificationService.warning('office.notification.deleteError');
            return;
        }

        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'office.delete.title',
                    text: 'office.delete.text',
                    textValue: office,
                    confirm: 'office.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => this.officeService.delete(office.id)),
                catchError(error => this.errorService.handleHttpError(error, 'office.notification.deleteError')),
                finalize(() => {
                    this.reload.next();
                    this.extendedSearchFilterService.clear();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'office.delete.successTitle',
                    message: 'office.delete.successMessage',
                })
            );
    }
}
