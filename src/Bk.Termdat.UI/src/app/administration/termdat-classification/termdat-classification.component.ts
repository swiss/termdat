import {DatePipe} from '@angular/common';
import {AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
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
    MatTableDataSource,
} from '@angular/material/table';
import {MatTooltip} from '@angular/material/tooltip';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObInputClearDirective, ObNotificationService} from '@oblique/oblique';
import {TermdatClassificationDetail} from 'api/TermdatClassificationDetail';
import {get, uniqWith} from 'lodash';
import {merge, Subject} from 'rxjs';
import {catchError, filter, switchMap, takeUntil} from 'rxjs/operators';
import {ConfirmData} from '../../shared/confirm-delete-dialog/confirm-data';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {TermdatClassificationService} from './termdat-classification.service';

@Component({
    selector: 'td-termdat-classification',
    templateUrl: './termdat-classification.component.html',
    styleUrls: ['./termdat-classification.component.scss'],
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
export class TermdatClassificationComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100, 1000];
    defaultPageSize = 20;

    rowSpans: {[column: string]: number[]} = {};

    displayedColumns: string[] = ['id', 'subjectName', 'domainId', 'domainName', 'entryCount', 'created', 'createdBy', 'modified', 'modifiedBy', 'action'];
    dataSource: MatTableDataSource<TermdatClassificationDetail> = new MatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly filterSubject = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly termdatClassificationService: TermdatClassificationService,
        private readonly translateService: TranslateService,
        private readonly dialog: MatDialog,
        private readonly errorService: ErrorService,
        private readonly notificationService: ObNotificationService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly router: Router,
        private readonly changeDetector: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.termdatClassificationService.getSubjectDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(termdatClassifications => {
                this.dataSource.data = termdatClassifications;

                this.rowSpans = this.generateRowSpans();

                this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
                    const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
                    const search = filterTerm?.toLocaleLowerCase() ?? '';

                    return (
                        data.id.toString().includes(search) ||
                        data.subjectName?.toLocaleLowerCase().includes(search) ||
                        data.domainId?.toString().includes(search) ||
                        data.domainName?.toLocaleLowerCase().includes(search) ||
                        data.entryCount.toString().includes(search) ||
                        data.created?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                        data.createdBy?.toLocaleLowerCase().includes(search) ||
                        data.modified?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                        data.modifiedBy?.toLocaleLowerCase().includes(search)
                    );
                };
            });

        this.reload.next();
    }

    ngAfterViewInit(): void {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.dataSource.sortData = (data, sort: MatSort) => {
            if (!sort.active || sort.direction === '') {
                return data;
            }

            const intlCollator = new Intl.Collator(this.translateService.getCurrentLang(), {sensitivity: 'base'});
            const isAsc = sort.direction === 'asc';

            data.sort((a, b) => {
                switch (sort.active) {
                    case 'subjectName':
                        return intlCollator.compare(a.subjectName, b.subjectName) * (isAsc ? 1 : -1);
                    case 'id':
                        return isAsc ? a.id - b.id : b.id - a.id;
                    default:
                        return 0;
                }
            });
            return data;
        };

        merge(this.paginator.page, this.sort.sortChange, this.filterSubject)
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(() => {
                this.rowSpans = this.generateRowSpans();
            });

        this.changeDetector.detectChanges();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.filterSubject.complete();
        this.unsubscribe.complete();
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement)?.value;
        this.dataSource.filter = filterValue?.trim() ?? '';

        this.dataSource.paginator?.firstPage();
        this.filterSubject.next();
    }

    clearFilter(): void {
        this.dataSource.filter = '';

        this.dataSource.paginator?.firstPage();
        this.filterSubject.next();
    }

    updateTermdatClassification(classification: TermdatClassificationDetail): void {
        if (classification.domainId) {
            void this.router.navigate(['administration', 'termdatClassification', 'subject', classification.id, 'domain', classification.domainId]);
        } else {
            void this.router.navigate(['administration', 'termdatClassification', 'subject', classification.id]);
        }
    }

    deleteTermdatClassification(classification: TermdatClassificationDetail): void {
        if (classification.entryCount > 0) {
            this.notificationService.warning('subject.delete.errorMessage');
            return;
        }

        if (classification.domainId != null) {
            this.deleteDomain(classification);
        } else {
            this.deleteSubject(classification);
        }
    }

    private deleteDomain(classification: TermdatClassificationDetail) {
        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'domain.delete.title',
                    text: 'domain.delete.text',
                    textValue: classification,
                    confirm: 'domain.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => {
                    this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.termdatClassificationService.deleteDomain(classification.domainId!);
                }),
                catchError(error => {
                    this.reload.next();
                    return this.errorService.handleHttpError(error, 'domain.notification.deleteError');
                })
            )
            .subscribe(() => {
                this.notificationService.success({
                    title: 'domain.delete.successTitle',
                    message: 'domain.delete.successMessage',
                });

                this.reload.next();
            });
    }

    private deleteSubject(classification: TermdatClassificationDetail) {
        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'subject.delete.title',
                    text: 'subject.delete.text',
                    textValue: classification,
                    confirm: 'subject.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => {
                    this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                    return this.termdatClassificationService.deleteSubject(classification.id);
                }),
                catchError(error => {
                    this.reload.next();
                    return this.errorService.handleHttpError(error, 'subject.delete.errorMessage');
                })
            )
            .subscribe(() => {
                this.notificationService.success({
                    title: 'subject.delete.successTitle',
                    message: 'subject.delete.successMessage',
                });
                this.reload.next();
            });
    }

    private generateRowSpans(): {[column: string]: number[]} {
        const data = this.getVisibleData();

        return {
            id: this.rowSpanDeep(['id'], data),
            domainId: this.rowSpanDeep(['id', 'domainId'], data),
        };
    }

    private rowSpanDeep(paths: (keyof TermdatClassificationDetail)[], data: TermdatClassificationDetail[]): number[] {
        if (!paths?.length) {
            return [...data]
                .map(() => 0)
                .fill(0)
                .fill(data.length, 0, 1);
        }

        const copyPaths = [...paths];
        const path = copyPaths.shift() as keyof TermdatClassificationDetail;

        const uniq = uniqWith(data, (a: TermdatClassificationDetail, b: TermdatClassificationDetail) => get(a, path) === get(b, path)).map(
            (item: TermdatClassificationDetail) => get(item, path)
        );

        const mapped = uniq.map(uniqItem =>
            this.rowSpanDeep(
                copyPaths,
                data.filter(item => uniqItem === get(item, path))
            )
        );

        return mapped.flat(paths.length) as number[];
    }

    private getVisibleData(): TermdatClassificationDetail[] {
        const data = this.sort ? this.dataSource.sortData(this.dataSource.filteredData, this.sort) : this.dataSource.filteredData;

        if (!this.paginator) {
            return data;
        }

        const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
        const endIndex = startIndex + this.paginator.pageSize;

        return data.slice(startIndex, endIndex);
    }
}
