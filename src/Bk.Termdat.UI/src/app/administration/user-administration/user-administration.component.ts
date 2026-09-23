import {DatePipe} from '@angular/common';
import {AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatChipListbox, MatChipOption} from '@angular/material/chips';
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
import {Router} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObInputClearDirective} from '@oblique/oblique';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {merge, Subject} from 'rxjs';
import {map, switchMap, takeUntil} from 'rxjs/operators';
import {authenticatedPageSize} from '../../shared/PaginationMapper';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {UserAccountService} from './user-account.service';

@Component({
    selector: 'td-user-administration',
    templateUrl: './user-administration.component.html',
    styleUrls: ['./user-administration.component.scss'],
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
        MatChipListbox,
        MatChipOption,
        MatIconButton,
        ObButtonDirective,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        MatNoDataRow,
        DatePipe,
        TranslatePipe,
    ],
})
export class UserAdministrationComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = authenticatedPageSize;
    defaultPageSize = 20;

    displayedColumns: string[] = ['lastName', 'firstName', 'email', 'role', 'lastAction', 'collections', 'glossaries', 'action'];
    dataSource: TdMatTableDataSource<UserAccountDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly userAccountService: UserAccountService,
        private readonly translateService: TranslateService,
        private readonly router: Router
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.userAccountService.getUserAccountDetails()),
                takeUntil(this.unsubscribe),
                map(userAccounts => userAccounts.filter(item => item.hasRole))
            )
            .subscribe(userAccounts => {
                this.dataSource.data = userAccounts;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.lastName.toLocaleLowerCase().includes(search) ||
                data.firstName.toLocaleLowerCase().includes(search) ||
                data.email.toLocaleLowerCase().includes(search) ||
                data.role?.toLocaleLowerCase().includes(search) ||
                this.nullableDateIncludes(search, data.lastAction) ||
                data.collections?.some(c => c.code.toLocaleLowerCase().includes(search)) ||
                data.glossaries?.some(g => g.code.toLocaleLowerCase().includes(search))
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

    clearFilter() {
        this.dataSource.filter = '';

        this.dataSource.paginator?.firstPage();
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement)?.value;
        this.dataSource.filter = filterValue?.trim() ?? '';

        this.dataSource.paginator?.firstPage();
    }

    updateUserAccount(userAccount: UserAccountDetail): void {
        void this.router.navigate(['administration', 'userAdministration', userAccount.id]);
    }

    private nullableDateIncludes(searchTerm: string, date?: Date): boolean {
        const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
        return date !== null || date !== undefined
            ? new Date(date!).toLocaleDateString(this.translateService.getCurrentLang(), dateFormatOptions).includes(searchTerm)
            : false;
    }
}
