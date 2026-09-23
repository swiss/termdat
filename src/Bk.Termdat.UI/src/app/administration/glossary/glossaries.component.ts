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
import {GlossaryDetail} from 'api/GlossaryDetail';
import {merge, Subject} from 'rxjs';
import {filter, finalize, switchMap, takeUntil} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ColumnsMenuComponent} from '../../shared/columns-menu/columns-menu.component';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {GlossaryService} from './glossary.service';

const GLOSSARY_COLUMNS = [
    {key: 'code', label: 'field.code', toggleable: true},
    {key: 'name', label: 'administration.field.name', toggleable: true},
    {key: 'description', label: 'administration.field.description', toggleable: true},
    {key: 'entryCount', label: 'administration.field.entryCount', toggleable: true},
    {key: 'createdBy', label: 'field.createdBy', toggleable: true},
    {key: 'action', label: 'administration.action', toggleable: false},
] as const;

type GlossaryColumn = (typeof GLOSSARY_COLUMNS)[number];
type GlossaryColumnKey = GlossaryColumn['key'];

@Component({
    selector: 'td-glossaries',
    templateUrl: './glossaries.component.html',
    styleUrls: ['./glossaries.component.scss'],
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
export class GlossariesComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    readonly columns = GLOSSARY_COLUMNS;
    displayedColumns: GlossaryColumnKey[] = GLOSSARY_COLUMNS.map(column => column.key);
    dataSource: TdMatTableDataSource<GlossaryDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    private readonly descriptionMaxLength = 200;

    constructor(
        private readonly translateService: TranslateService,
        private readonly glossaryService: GlossaryService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly router: Router,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.glossaryService.getGlossaryDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(glossaries => {
                this.dataSource.data = glossaries;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.code.toLocaleLowerCase().includes(search) ||
                data.entryCount.toString().includes(search) ||
                data.name.toLocaleLowerCase().includes(search) ||
                data.description?.toLocaleLowerCase().includes(search) ||
                data.createdBy?.toLocaleLowerCase().includes(search)
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
        this.displayedColumns = displayedColumns as GlossaryColumnKey[];
    }

    truncateDescription(description?: string): string {
        if (!description || description.length <= this.descriptionMaxLength) {
            return description ?? '';
        }

        return `${description.slice(0, this.descriptionMaxLength)}...`;
    }

    update(glossaryDetail: GlossaryDetail) {
        void this.router.navigate(['administration', 'glossary', glossaryDetail.id]);
    }

    delete(glossaryDetail: GlossaryDetail) {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'glossary.delete.title',
                    text: 'glossary.delete.text',
                    textValue: {...glossaryDetail, total: glossaryDetail.entryCount},
                    confirm: 'glossary.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.glossaryService.delete(glossaryDetail.id)),
                finalize(() => {
                    this.reload.next();
                    this.extendedSearchFilterService.clear();
                })
            )
            .subscribe(() =>
                this.notificationService.success({
                    title: 'glossary.delete.successTitle',
                    message: 'glossary.delete.successMessage',
                })
            );
    }
}
