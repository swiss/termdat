import {DatePipe} from '@angular/common';
import {AfterViewInit, Component, DestroyRef, inject, OnInit, signal, ViewChild} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatPaginator} from '@angular/material/paginator';
import {MatFormField, MatLabel} from '@angular/material/select';
import {MatSort, MatSortHeader, Sort} from '@angular/material/sort';
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
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObInputClearDirective, ObNotificationService} from '@oblique/oblique';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {merge, Subject} from 'rxjs';
import {catchError, filter, finalize, switchMap} from 'rxjs/operators';
import {ConfirmData} from '../../shared/confirm-delete-dialog/confirm-data';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {NewsService} from './news.service';

@Component({
    selector: 'td-news',
    imports: [
        MatButton,
        ObButtonDirective,
        RouterLink,
        MatIcon,
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
        DatePipe,
        TranslatePipe,
        MatRow,
        ObInputClearDirective,
        MatLabel,
        MatFormField,
        MatInput,
        MatPaginator,
    ],
    templateUrl: './news.component.html',
    styleUrl: './news.component.scss',
})
export class NewsComponent implements OnInit, AfterViewInit {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    displayedColumns: string[] = ['id', 'title', 'isEnabled', 'created', 'createdBy', 'modified', 'modifiedBy', 'validTo', 'sortIndex', 'action'];
    dataSource: TdMatTableDataSource<NewsItemSummary> = new TdMatTableDataSource();
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    protected readonly isCustomSorted = signal(false);
    protected readonly isFiltered = signal(false);
    private readonly reload = new Subject<void>();
    private readonly destroyRef = inject(DestroyRef);

    constructor(
        private readonly translateService: TranslateService,
        private readonly newsService: NewsService,
        private readonly router: Router,
        private readonly dialog: MatDialog,
        private readonly errorService: ErrorService,
        private readonly notificationService: ObNotificationService
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.newsService.getNewsSummaries()),
                takeUntilDestroyed(this.destroyRef)
            )
            .subscribe(newsItems => {
                this.dataSource.data = newsItems;
                this.dataSource.language = this.translateService.getCurrentLang();
            });
        this.reload.next();
    }

    ngAfterViewInit(): void {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
    }

    onSortChange(event: Sort): void {
        this.isCustomSorted.set(!!event.active && !!event.direction && !(event.active === 'sortIndex' && event.direction === 'asc'));
    }

    update(newsItemSummary: NewsItemSummary) {
        void this.router.navigate(['administration', 'news', newsItemSummary.id]);
    }

    copy(newsItemSummary: NewsItemSummary) {
        void this.router.navigate(['administration', 'news', 'create'], {
            queryParams: {copyFrom: newsItemSummary.id},
        });
    }

    create() {
        void this.router.navigate(['administration', 'news', 'create']);
    }

    delete(news: NewsItemSummary): void {
        this.dialog
            .open<ConfirmDeleteDialogComponent, ConfirmData>(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'news.delete.title',
                    text: 'news.delete.text',
                    textValue: {
                        title: news.title,
                    },
                    confirm: 'news.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(confirmed => !!confirmed),
                switchMap(() => this.newsService.delete(news.id)),
                catchError(error => this.errorService.handleHttpError(error, 'news.notification.deleteError')),
                finalize(() => {
                    this.reload.next();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'news.delete.successTitle',
                    message: 'news.delete.successMessage',
                })
            );
    }

    clearFilter(): void {
        this.dataSource.filter = '';
        this.isFiltered.set(false);

        this.dataSource.paginator?.firstPage();
    }

    applyFilter(event: Event): void {
        const filterValue = (event.target as HTMLInputElement)?.value ?? '';

        this.dataSource.filter = filterValue.trim();
        this.isFiltered.set(filterValue.length > 0);

        this.dataSource.paginator?.firstPage();
    }

    moveUp(news: NewsItemSummary) {
        this.newsService.moveUp(news.id).subscribe(() => this.reload.next());
    }

    moveDown(news: NewsItemSummary) {
        this.newsService.moveDown(news.id).subscribe(() => this.reload.next());
    }
}
