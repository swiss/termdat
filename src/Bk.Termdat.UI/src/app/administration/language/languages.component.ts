import {DatePipe} from '@angular/common';
import {AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatButton, MatIconButton} from '@angular/material/button';
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
import {ObButtonDirective, ObInputClearDirective} from '@oblique/oblique';
import {LanguageDetail} from 'api/LanguageDetail';
import {merge, of, Subject} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {LanguageService} from '../../shared/language.service';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';

@Component({
    selector: 'td-languages',
    templateUrl: './languages.component.html',
    styleUrls: ['./languages.component.scss'],
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
export class LanguagesComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 30;

    displayedColumns: string[] = ['id', 'isoCode', 'name', 'isUiLanguage', 'sortIndex', 'created', 'createdBy', 'modified', 'modifiedBy', 'action'];
    dataSource: TdMatTableDataSource<LanguageDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly translateService: TranslateService,
        private readonly languageService: LanguageService,
        private readonly router: Router
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.languageService.getLanguageDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(languages => {
                this.dataSource.data = languages;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filter: string): boolean => {
            const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
            const search = filter?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.isoCode.toLocaleLowerCase().includes(search) ||
                data.name.toLocaleLowerCase().includes(search) ||
                data.sortIndex.toString().includes(search) ||
                data.created?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                data.createdBy?.toLocaleLowerCase().includes(search) ||
                data.modified?.toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(search) ||
                data.modifiedBy?.toLocaleLowerCase().includes(search)
            );
        };
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

    update(language: LanguageDetail): void {
        void this.router.navigate(['administration', 'language', language.id]);
    }
}
