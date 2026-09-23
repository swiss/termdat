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
import {CollectionDetail} from 'api/CollectionDetail';
import {EMPTY, merge, Subject} from 'rxjs';
import {catchError, filter, finalize, switchMap, takeUntil} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ColumnsMenuComponent} from '../../shared/columns-menu/columns-menu.component';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {authenticatedPageSize} from '../../shared/PaginationMapper';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {CollectionService} from './collection.service';

const COLLECTION_COLUMNS = [
    {key: 'code', label: 'field.code', toggleable: true},
    {key: 'name', label: 'administration.field.name', toggleable: true},
    {key: 'entryCount', label: 'administration.field.entryCount', toggleable: true},
    {key: 'active', label: 'administration.collection.field.active', toggleable: true},
    {key: 'isObsolete', label: 'administration.collection.field.isObsolete', toggleable: true},
    {key: 'activeApi', label: 'administration.collection.field.activeApi', toggleable: true},
    {key: 'exportLindas', label: 'administration.collection.field.exportLindasShort', toggleable: true},
    {key: 'classification', label: 'administration.collection.field.classification', toggleable: true},
    {key: 'action', label: 'administration.action', toggleable: false},
] as const;

type CollectionColumn = (typeof COLLECTION_COLUMNS)[number];
type CollectionColumnKey = CollectionColumn['key'];

@Component({
    selector: 'td-collections',
    templateUrl: './collections.component.html',
    styleUrls: ['./collections.component.scss'],
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
        ColumnsMenuComponent,
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
        TranslatePipe,
    ],
})
export class CollectionsComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = authenticatedPageSize;

    readonly columns = COLLECTION_COLUMNS;
    displayedColumns: CollectionColumnKey[] = COLLECTION_COLUMNS.map(column => column.key);
    dataSource: TdMatTableDataSource<CollectionDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly translateService: TranslateService,
        private readonly collectionService: CollectionService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly router: Router,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.collectionService.getCollectionDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(collections => {
                this.dataSource.data = collections;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.code.toLocaleLowerCase().includes(search) ||
                data.entryCount.toString().includes(search) ||
                data.name.toLocaleLowerCase().includes(search) ||
                (data.classification?.toLocaleLowerCase().includes(search) ?? false)
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

    onDisplayedColumnsChange(displayedColumns: string[]): void {
        this.displayedColumns = displayedColumns as CollectionColumnKey[];
    }

    update(collectionDetail: CollectionDetail): void {
        void this.router.navigate(['administration', 'collection', collectionDetail.id]);
    }

    softDelete(collection: CollectionDetail): void {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'collection.softDelete.title',
                    text: 'collection.softDelete.text',
                    textValue: {...collection, total: collection.entryCount},
                    confirm: 'collection.softDelete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.collectionService.delete(collection.id, true)),
                finalize(() => {
                    this.reload.next();
                    this.extendedSearchFilterService.clear();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'collection.softDelete.successTitle',
                    message: 'collection.softDelete.successMessage',
                })
            );
    }

    delete(collection: CollectionDetail): void {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'collection.delete.title',
                    text: 'collection.delete.text',
                    textValue: {...collection},
                    confirm: 'collection.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.collectionService.delete(collection.id, false)),
                catchError(error => {
                    this.errorService.handleHttpError(error, 'collection.delete.errorMessage');
                    return EMPTY;
                }),
                finalize(() => {
                    this.reload.next();
                    this.extendedSearchFilterService.clear();
                })
            )
            .subscribe({
                next: () => {
                    this.notificationService.success({
                        title: 'collection.delete.successTitle',
                        message: 'collection.delete.successMessage',
                    });
                },
            });
    }

    restore(collection: CollectionDetail) {
        this.collectionService.restore(collection.id).subscribe(() => {
            collection.deleted = false;

            this.extendedSearchFilterService.clear();

            this.notificationService.success({
                title: 'collection.restore.successTitle',
                message: 'collection.restore.successMessage',
            });
        });
    }
}
