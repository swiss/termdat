import {DatePipe} from '@angular/common';
import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatDatepicker, MatDatepickerInput, MatDatepickerToggle} from '@angular/material/datepicker';
import {MatDialog} from '@angular/material/dialog';
import {MatFormField, MatLabel, MatSuffix} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatPaginator, PageEvent} from '@angular/material/paginator';
import {MatSort, MatSortHeader, Sort} from '@angular/material/sort';
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
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObInputClearDirective, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {StatisticSearchResult} from 'api/StatisticSearchResult';
import {StatisticSummary} from 'api/StatisticSummary';
import {merge, Observable, Subject} from 'rxjs';
import {catchError, filter, finalize, switchMap, takeUntil} from 'rxjs/operators';
import {ConfirmData} from '../../shared/confirm-delete-dialog/confirm-data';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {Pagination} from '../../shared/Pagination';
import {mapToPagination} from '../../shared/PaginationMapper';
import {mapToSortString} from '../../shared/SortMapper';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {StatisticService} from './statistic.service';

@Component({
    selector: 'td-statistic',
    templateUrl: './statistic.component.html',
    styleUrls: ['./statistic.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        ObInputClearDirective,
        MatIcon,
        MatDatepickerInput,
        MatDatepickerToggle,
        MatSuffix,
        MatDatepicker,
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
        ObUnsavedChangesDirective,
    ],
})
export class StatisticComponent implements OnInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    fromDateMin = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
    toDateMax = new Date(new Date().setDate(new Date().getDate() + 1));

    fromDateMax?: Date;
    toDateMin?: Date;

    fromDate?: Date;
    toDate?: Date;

    dateFilterChanged = false;

    events: string[] = [];

    statistics?: StatisticSummary[] | undefined;
    searchResult?: StatisticSearchResult;

    displayedColumns: string[] = ['id', 'searchText', 'languages', 'searchFields', 'numberOfSearches', 'dateLastSearch', 'action'];
    dataSource: TdMatTableDataSource<StatisticSummary> = new TdMatTableDataSource();

    pagination?: Pagination;
    sortState?: Sort;
    sortField = '-dateLastSearch';

    formGroup: FormGroup;

    private filterTextControl!: AbstractControl;
    private fromDateControl!: AbstractControl;
    private toDateControl!: AbstractControl;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly statisticService: StatisticService,
        private readonly translateService: TranslateService,
        private readonly errorService: ErrorService,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService
    ) {
        this.formGroup = this.formBuilder.group({});
    }

    ngOnInit(): void {
        this.setupForm();
        this.loadPagination();

        merge(
            this.reload,
            this.translateService.onLangChange,
            this.fromDateControl.valueChanges,
            this.toDateControl.valueChanges,
            this.filterTextControl.valueChanges
        )
            .pipe(
                switchMap(() => this.search()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(result => {
                this.updateDatePickerMinMaxValues();
                this.searchResult = result;
                this.dataSource.data = result.statistics;
                this.statistics = result.statistics;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.reload.next();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.reload.complete();
    }

    loadPagination() {
        this.pagination = this.defaultPagination();
    }

    sort(sort: Sort): void {
        this.sortState = sort;

        this.search().subscribe(result => {
            this.searchResult = result;
            this.dataSource.data = result.statistics;
            this.statistics = result.statistics;
            this.dataSource.language = this.translateService.getCurrentLang();
        });
    }

    page(pageEvent: PageEvent) {
        this.pagination = mapToPagination(pageEvent);

        this.search().subscribe(result => {
            this.searchResult = result;
            this.dataSource.data = result.statistics;
            this.statistics = result.statistics;
            this.dataSource.language = this.translateService.getCurrentLang();
        });
    }

    search(): Observable<StatisticSearchResult> {
        this.sortField = mapToSortString(this.sortState!);

        return this.statisticService.getStatisticsBySearchQuery(
            this.pagination!,
            this.sortField,
            this.filterTextControl.value,
            this.fromDateControl.value,
            this.toDateControl.value
        );
    }

    clearFilter(): void {
        this.dataSource.filter = '';
        this.filterTextControl.setValue('');

        this.dataSource.paginator?.firstPage();

        this.reload.next();
    }

    delete(statistic: StatisticSummary): void {
        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'statistic.delete.title',
                    text: 'statistic.delete.text',
                    textValue: statistic,
                    confirm: 'statistic.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => this.statisticService.delete(statistic.id)),
                catchError(error => this.errorService.handleHttpError(error, 'statistic.notification.deleteError')),
                finalize(() => {
                    this.reload.next();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'statistic.delete.successTitle',
                    message: 'statistic.delete.successMessage',
                })
            );
    }

    bulkDelete(): void {
        const phrase = this.filterTextControl.value;
        const fromDate = this.fromDateControl.value;
        const toDate = this.toDateControl.value;

        this.deleteSelection(phrase, fromDate, toDate);
    }

    private readonly defaultPagination = (): Pagination => {
        return {
            pageSize: 20,
            pageIndex: 0,
        };
    };

    private deleteSelection(phrase: string, fromDate: Date, toDate: Date): void {
        const deleteRecords = this.dataSource.data.length;

        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'statistic.bulkDelete.title',
                    text: 'statistic.bulkDelete.text',
                    textValue: {deleteRecords},
                    confirm: 'statistic.bulkDelete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => this.statisticService.deleteSelection(this.pagination!, this.sortField, phrase, fromDate, toDate)),
                catchError(error => this.errorService.handleHttpError(error, 'statistic.notification.deleteError')),
                finalize(() => {
                    this.reload.next();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'statistic.bulkDelete.successTitle',
                    message: 'statistic.bulkDelete.successMessage',
                })
            );
    }

    private updateDatePickerMinMaxValues() {
        this.dateFilterChanged = true;

        const dateFrom = this.fromDateControl.valid ? this.fromDateControl.value : undefined;
        const dateTo = this.toDateControl.valid ? this.toDateControl.value : undefined;

        this.fromDate = dateFrom;
        this.toDate = dateTo;

        this.fromDateMax = dateTo;
        this.toDateMin = dateFrom;
    }

    private setupForm(): void {
        this.formGroup.reset();

        this.fromDateMax?.setDate(this.toDateMax.getDate() - 1);
        this.toDateMin?.setDate(this.fromDateMin.getDate() + 1);

        this.formGroup.setControl('filterText', this.formBuilder.control(null));
        this.formGroup.setControl('fromDate', this.formBuilder.control(this.fromDateMin ?? null));
        this.formGroup.setControl('toDate', this.formBuilder.control(this.toDateMax ?? null));

        this.filterTextControl = this.formGroup.get('filterText') as AbstractControl;
        this.fromDateControl = this.formGroup.get('fromDate') as AbstractControl;
        this.toDateControl = this.formGroup.get('toDate') as AbstractControl;
    }
}
