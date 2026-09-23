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
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObInputClearDirective, ObNotificationService} from '@oblique/oblique';
import {ConceptSystemDetail} from 'api/ConceptSystemDetail';
import {EMPTY, merge, Subject} from 'rxjs';
import {catchError, filter, switchMap, takeUntil, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {ConceptSystemService} from './concept-system.service';

@Component({
    selector: 'td-concept-systems',
    templateUrl: './concept-systems.component.html',
    styleUrls: ['./concept-systems.component.scss'],
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
export class ConceptSystemsComponent implements OnInit, AfterViewInit, OnDestroy {
    pageSizeOptions: number[] = [10, 20, 30, 40, 50, 100];
    defaultPageSize = 20;

    displayedColumns: string[] = ['id', 'title', 'collection', 'created', 'createdBy', 'modified', 'modifiedBy', 'action'];
    dataSource: TdMatTableDataSource<ConceptSystemDetail> = new TdMatTableDataSource();

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    private readonly reload = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly router: Router,
        private readonly translateService: TranslateService,
        private readonly conceptSystemService: ConceptSystemService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService,
        private readonly errorService: ErrorService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    ngOnInit(): void {
        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.conceptSystemService.getConceptSystemDetails()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(conceptSystems => {
                this.dataSource.data = conceptSystems;
                this.dataSource.language = this.translateService.getCurrentLang();
            });

        this.dataSource.filterPredicate = (data, filterTerm: string): boolean => {
            const dateFormatOptions = {year: 'numeric', month: '2-digit', day: '2-digit'} as Intl.DateTimeFormatOptions;
            const search = filterTerm?.toLocaleLowerCase() ?? '';

            return (
                data.id.toString().includes(search) ||
                data.title.toLocaleLowerCase().includes(search) ||
                data.collection.toLocaleLowerCase().includes(search) ||
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

    update(conceptSystem: ConceptSystemDetail): void {
        void this.router.navigate(['administration', 'conceptSystem', conceptSystem.id]);
    }

    delete(conceptSystem: ConceptSystemDetail): void {
        if (conceptSystem.entryCount) {
            return;
        }

        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'conceptSystem.delete.title',
                    text: 'conceptSystem.delete.text',
                    textValue: {...conceptSystem},
                    confirm: 'conceptSystem.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                tap(() => this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1)),
                switchMap(() =>
                    this.conceptSystemService.delete(conceptSystem.id).pipe(
                        catchError(error => {
                            this.errorService.handleHttpError(error, 'conceptSystem.notification.deleteError');
                            return EMPTY;
                        })
                    )
                )
            )
            .subscribe(() => {
                this.reload.next();
                this.extendedSearchFilterService.clear();

                this.notificationService.success({
                    title: 'conceptSystem.delete.successTitle',
                    message: 'conceptSystem.delete.successMessage',
                });
            });
    }
}
