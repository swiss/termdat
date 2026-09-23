import {AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
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
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObInputClearDirective} from '@oblique/oblique';
import {LenochClassificationDetail} from 'api/LenochClassificationDetail';
import {get, uniqWith} from 'lodash';
import {merge, of, Subject} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {authenticatedPageSize} from '../../shared/PaginationMapper';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {LenochClassificationService} from './lenoch-classification.service';
import {sortLenochClassificationDetails} from './LenochClassificationSort';

@Component({
    selector: 'td-lenoch-classification',
    templateUrl: './lenoch-classification.component.html',
    styleUrls: ['./lenoch-classification.component.scss'],
    imports: [
        MatFormField,
        MatLabel,
        MatInput,
        ObInputClearDirective,
        MatIcon,
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
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        MatNoDataRow,
        TranslatePipe,
    ],
})
export class LenochClassificationComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = authenticatedPageSize;

    displayedColumns: string[] = ['id', 'code', 'name', 'entryCount', 'subject', 'domains'];
    dataSource: TdMatTableDataSource<LenochClassificationDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    rowSpans: {[column: string]: number[]} = {};

    private readonly unsubscribe = new Subject<void>();
    private readonly filterSubject = new Subject<void>();

    constructor(
        private readonly lenochClassificationService: LenochClassificationService,
        private readonly translateService: TranslateService
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.lenochClassificationService.getLenochClassificationDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(lenochClassifications => {
                const translatedSubjectNullSubstitute = this.translateService.instant('administration.lenochClassifications.unknownSubject');

                const data = lenochClassifications.map(lc => {
                    // substitute is used to be able to filter for those entries
                    lc.subject ??= translatedSubjectNullSubstitute;

                    return lc;
                });

                data.sort((a, b) => sortLenochClassificationDetails(a, b, this.translateService.getCurrentLang()));

                this.dataSource.data = data;

                this.rowSpans = this.generateRowSpans();

                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return !!(
                data.id.toString().includes(search) ||
                data.code.toLocaleLowerCase().includes(search) ||
                data.entryCount.toString().includes(search) ||
                data.name.toLocaleLowerCase().includes(search) ||
                data.subject?.toLocaleLowerCase().includes(search) ||
                data.domain?.toLocaleLowerCase().includes(search)
            );
        };
    }

    ngAfterViewInit(): void {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        merge(this.paginator.page, this.sort.sortChange, this.filterSubject)
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(() => {
                this.rowSpans = this.generateRowSpans();
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.filterSubject.complete();
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

    private generateRowSpans(): {[column: string]: number[]} {
        const data = this.getVisibleData();

        return {
            id: this.rowSpanDeep(['id'], data),
            subject: this.rowSpanDeep(['id', 'subject'], data),
        };
    }

    private rowSpanDeep(paths: (keyof LenochClassificationDetail)[], data: LenochClassificationDetail[]): number[] {
        if (!paths?.length) {
            return [...data]
                .map(() => 0)
                .fill(0)
                .fill(data.length, 0, 1);
        }

        const copyPaths = [...paths];
        const path = copyPaths.shift() as keyof LenochClassificationDetail;

        const uniq = uniqWith(data, (a: LenochClassificationDetail, b: LenochClassificationDetail) => get(a, path) === get(b, path)).map(
            (item: LenochClassificationDetail) => get(item, path)
        );

        const mapped = uniq.map(uniqItem =>
            this.rowSpanDeep(
                copyPaths,
                data.filter(item => uniqItem === get(item, path))
            )
        );

        return mapped.flat(paths.length) as number[];
    }

    private getVisibleData(): LenochClassificationDetail[] {
        // DataSource.sortData applies sort to provided array, so an array copy should be used, otherwise
        // resetting of sort will not work, since direction is empty string and sortData defaults to asc.
        let data = [...this.dataSource.filteredData];
        data = this.sort ? this.dataSource.sortData(data, this.sort) : data;

        if (!this.paginator) {
            return data;
        }

        const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
        const endIndex = startIndex + this.paginator.pageSize;

        return data.slice(startIndex, endIndex);
    }
}
