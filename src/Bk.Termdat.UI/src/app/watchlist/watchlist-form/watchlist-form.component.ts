import {SelectionModel} from '@angular/cdk/collections';
import {AsyncPipe, NgClass} from '@angular/common';
import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatDialog} from '@angular/material/dialog';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
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
import {Status} from 'api/Status';
import {EMPTY, merge, Observable, of, Subject} from 'rxjs';
import {catchError, debounceTime, filter, map, switchMap, takeUntil} from 'rxjs/operators';
import {ConceptSystemService} from '../../administration/concept-system/concept-system.service';
import {AuthService} from '../../auth/auth.service';
import {EntryService} from '../../entry/entry.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {authenticatedPageSize} from '../../shared/PaginationMapper';
import {TdMatTableDataSource} from '../../shared/td-mat-table-data-source';
import {WatchlistService} from '../watchlist.service';
import {WatchlistExtendedEntry} from '../WatchlistExtendedEntry';

@Component({
    selector: 'td-watchlist-form',
    templateUrl: './watchlist-form.component.html',
    styleUrls: ['./watchlist-form.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        MatTooltip,
        MatMenuTrigger,
        MatMenu,
        MatMenuItem,
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
        MatCheckbox,
        MatCellDef,
        MatCell,
        MatSortHeader,
        RouterLink,
        NgClass,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        MatNoDataRow,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class WatchlistFormComponent implements OnInit, OnDestroy {
    pageSizeOptions: number[] = authenticatedPageSize;
    areAllVisibleSelected = false;
    fieldValueTranslation?: string;

    displayedColumns: string[] = [
        'select',
        'id',
        'entryId',
        'identification',
        'conceptSystemPosition.value',
        'value',
        'statusTranslation',
        'collection.code',
        'glossaries',
        'adminInfos',
    ];
    dataSource = new TdMatTableDataSource<WatchlistExtendedEntry>();

    isAdmin = false;
    isWriter = false;
    isSuperUser = false;
    areAllSelectedDeleted$ = new Observable<boolean>();
    areAllSelectedNotDeleted$ = new Observable<boolean>();
    areAllSuitableForConceptSystemPositionChange$ = new Observable<boolean>();
    collectionsWithConceptSystems: string[] = [];

    @ViewChild(MatPaginator, {static: true}) set paginator(element: MatPaginator) {
        this.dataSource.paginator?.page?.unsubscribe();
        this.dataSource.paginator = element;
        this.dataSource.paginator.page.subscribe(() => {
            this.updateAreAllVisibleSelected();
        });
    }

    @ViewChild(MatSort, {static: true}) set sort(element: MatSort) {
        this.dataSource.sort?.sortChange?.unsubscribe();
        this.dataSource.sort = element;
        this.dataSource.sort.sortChange.subscribe(() => {
            this.dataSource.paginator?.firstPage();
            this.updateAreAllVisibleSelected();
        });
    }

    readonly selection = new SelectionModel<WatchlistExtendedEntry>(true, []);
    private readonly reload = new Subject<void>();
    private readonly filterSubject = new Subject<string>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly watchlistService: WatchlistService,
        private readonly translateService: TranslateService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly notificationService: ObNotificationService,
        private readonly authService: AuthService,
        private readonly dialog: MatDialog,
        private readonly entryService: EntryService,
        private readonly router: Router,
        private readonly conceptSystemService: ConceptSystemService
    ) {}

    ngOnInit(): void {
        this.authService.isAdmin$.subscribe(admin => (this.isAdmin = admin));
        this.authService.isWriter$.subscribe(isWriter => (this.isWriter = isWriter));
        this.authService.isSuperUser$.subscribe(isSuperUser => (this.isSuperUser = isSuperUser));

        this.conceptSystemService.getConceptSystemDetails().subscribe(cs => {
            this.collectionsWithConceptSystems = cs.map(y => y.collection);
        });

        merge(of(true), this.translateService.onLangChange, this.reload)
            .pipe(
                switchMap(() => this.watchlistService.watchlist$),
                switchMap(entryIds => this.watchlistService.getEntries(entryIds)),
                takeUntil(this.unsubscribe)
            )
            .subscribe(watchlistEntries => {
                this.dataSource.data = watchlistEntries.map((entry, i) => {
                    return {
                        ...entry,
                        id: i + 1,
                        statusTranslation: this.getStatusTranslation(entry.status),
                        isValidated: entry.status === Status.Validated,
                        searchPhrase: `EntryId:${entry.entryId}`,
                    };
                });

                this.dataSource.language = this.translateService.getCurrentLang();

                this.pageSizeOptions = authenticatedPageSize;
                if (this.dataSource.data.length > authenticatedPageSize[authenticatedPageSize.length - 1]) {
                    this.pageSizeOptions = [...authenticatedPageSize, this.dataSource.data.length];
                }

                this.fieldValueTranslation = [
                    this.translateService.instant('field.terminus'),
                    this.translateService.instant('field.name'),
                    this.translateService.instant('field.abbreviation'),
                    this.translateService.instant('field.phraseology'),
                ].join(', ');

                this.selection.clear();

                const selectedItems = this.watchlistService.readSelectedItems();
                const allItems = this.getVisibleItems();

                selectedItems.forEach(id => {
                    const item = allItems.find(i => i.entryId === id);
                    if (item !== undefined) {
                        this.selection.select(item);
                    }
                });

                this.updateAreAllVisibleSelected();
            });

        // Explicit definition of filter columns
        this.dataSource.filterPredicate = (data: WatchlistExtendedEntry, filterText: string): boolean => {
            return (
                data.entryId.toString().includes(filterText) ||
                data.identification?.toString().includes(filterText) ||
                data.value?.toUpperCase().includes(filterText) ||
                data.conceptSystemPosition?.value.toUpperCase().includes(filterText) ||
                data.collection?.code.toUpperCase().includes(filterText) ||
                data.statusTranslation.toUpperCase().includes(filterText)
            );
        };

        this.filterSubject.pipe(debounceTime(125), takeUntil(this.unsubscribe)).subscribe(filterText => {
            this.dataSource.filter = filterText.trim().toUpperCase();
            this.dataSource.paginator?.firstPage();
            this.updateAreAllVisibleSelected();
        });

        this.areAllSelectedDeleted$ = this.selection.changed.pipe(map(sel => sel.source.selected.every(s => s.deleted)));
        this.areAllSelectedNotDeleted$ = this.selection.changed.pipe(map(sel => sel.source.selected.every(s => !s.deleted)));
        this.areAllSuitableForConceptSystemPositionChange$ = this.selection.changed.pipe(
            map(
                sel =>
                    sel.source.selected.length > 0 &&
                    sel.source.selected.every(s => !s.deleted && s.collection) &&
                    sel.source.selected.every(s => s.collection.code === sel.source.selected[0].collection.code) &&
                    this.collectionsWithConceptSystems.some(y => y === sel.source.selected[0].collection.code)
            )
        );
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    applyFilter(event: Event) {
        const filterText = (event.target as HTMLInputElement).value;
        this.filterSubject.next(filterText);
    }

    clearFilter(): void {
        this.filterSubject.next('');
    }

    masterToggle(): void {
        if (this.areAllVisibleSelected) {
            this.selection.deselect(...this.getVisibleItems());
        } else {
            this.selection.select(...this.getVisibleItems());
        }

        this.updateAreAllVisibleSelected();

        const ids = this.selection.selected.map(entry => entry.entryId);
        this.watchlistService.updateSelectedItems(ids);
    }

    toggleRow(row: WatchlistExtendedEntry): void {
        this.selection.toggle(row);
        this.updateAreAllVisibleSelected();

        const ids = this.selection.selected.map(entry => entry.entryId);
        this.watchlistService.updateSelectedItems(ids);
    }

    exportPdf(): void {
        const selectedAndSorted = this.getSelectedAndSortedEntryIds();

        if (!selectedAndSorted.length) {
            return;
        }

        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.watchlistService
            .createExportPdf(selectedAndSorted)
            .pipe(
                catchError(() => {
                    this.notificationService.error('watchlist.export.notification.error');
                    return EMPTY;
                })
            )
            .subscribe();
    }

    exportXlsx(): void {
        const selectedAndSorted = this.getSelectedAndSortedEntryIds();

        if (!selectedAndSorted.length) {
            return;
        }

        this.httpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.watchlistService
            .createExportXlsx(selectedAndSorted)
            .pipe()
            .subscribe({
                next: () => {
                    this.notificationService.success({message: 'watchlist.export.notification.success'});
                },
                error: () => {
                    this.notificationService.error({message: 'watchlist.export.notification.error'});
                },
            });
    }

    exportListedEntryIdsToTxt(): void {
        const selectedAndSorted = this.getSelectedAndSortedEntryIds();

        if (!selectedAndSorted.length) {
            return;
        }

        this.watchlistService.createExportTxt(selectedAndSorted);
    }

    softDeleteSelected(): void {
        const selectionCount = this.selection.selected.length;
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: selectionCount > 1 ? 'watchlist.deleteSelectedEntries.confirmTitle' : 'watchlist.deleteSelectedEntry.confirmTitle',
                    text: selectionCount > 1 ? 'watchlist.deleteSelectedEntries.confirmText' : 'watchlist.deleteSelectedEntry.confirmText',
                    textValue: {count: this.selection.selected.length},
                    confirm: 'watchlist.deleteSelectedEntries.confirmButton',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.entryService.deleteMultiple(this.selection.selected.map(entry => entry.entryId)))
            )
            .subscribe({
                next: entries => {
                    if (entries.length) {
                        this.watchlistService.displayUpdateInfoWarning(entries, 'entry.notification.lockedEntriesNotSoftDeleted');
                    } else {
                        this.notificationService.success({
                            title: selectionCount > 1 ? 'watchlist.deleteSelectedEntries.successTitle' : 'watchlist.deleteSelectedEntry.successTitle',
                            message: selectionCount > 1 ? 'watchlist.deleteSelectedEntries.successMessage' : 'watchlist.deleteSelectedEntry.successMessage',
                        });
                        this.reload.next();
                    }
                },
                error: () => this.notificationService.error(this.translateService.instant('entry.error.delete.multiple.failed')),
            });
    }

    hardDeleteSelected(): void {
        const selectionCount = this.selection.selected.length;
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: selectionCount > 1 ? 'watchlist.hardDeleteSelectedEntries.confirmTitle' : 'watchlist.hardDeleteSelectedEntry.confirmTitle',
                    text: selectionCount > 1 ? 'watchlist.hardDeleteSelectedEntries.confirmText' : 'watchlist.hardDeleteSelectedEntry.confirmText',
                    textValue: {count: this.selection.selected.length},
                    confirm: 'watchlist.hardDeleteSelectedEntries.confirmButton',
                },
                minWidth: '500px',
                width: 'auto',
                maxWidth: '90vw',
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() =>
                    this.entryService.deleteMultiple(
                        this.selection.selected.map(entry => entry.entryId),
                        false
                    )
                )
            )
            .subscribe({
                next: entries => {
                    if (entries.length > 0) {
                        this.watchlistService.displayUpdateInfoWarning(
                            entries,
                            'entry.notification.lockedEntriesNotHardDeleted',
                            'entry.notification.notSoftDeletedEntriesNotHardDeleted'
                        );
                    } else {
                        this.notificationService.success({
                            title: selectionCount > 1 ? 'watchlist.hardDeleteSelectedEntries.successTitle' : 'watchlist.hardDeleteSelectedEntry.successTitle',
                            message:
                                selectionCount > 1 ? 'watchlist.hardDeleteSelectedEntries.successMessage' : 'watchlist.hardDeleteSelectedEntry.successMessage',
                        });
                        this.reload.next();
                    }
                },
                error: () => this.notificationService.error(this.translateService.instant('entry.error.delete.multiple.failed')),
            });
    }

    restoreSelected(): void {
        const selectionCount = this.selection.selected.length;
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: selectionCount > 1 ? 'watchlist.restoreSelectedEntries.confirmTitle' : 'watchlist.restoreSelectedEntry.confirmTitle',
                    text: selectionCount > 1 ? 'watchlist.restoreSelectedEntries.confirmText' : 'watchlist.restoreSelectedEntry.confirmText',
                    textValue: {count: this.selection.selected.length},
                    confirm: 'watchlist.restoreSelectedEntries.confirmButton',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.entryService.restoreMultiple(this.selection.selected.map(entry => entry.entryId)))
            )
            .subscribe({
                next: entries => {
                    {
                        if (entries.length > 0) {
                            this.watchlistService.displayUpdateInfoWarning(entries, 'entry.notification.lockedEntriesNotRestore');
                        } else {
                            this.notificationService.success({
                                title: selectionCount > 1 ? 'watchlist.restoreSelectedEntries.successTitle' : 'watchlist.restoreSelectedEntry.successTitle',
                                message:
                                    selectionCount > 1 ? 'watchlist.restoreSelectedEntries.successMessage' : 'watchlist.restoreSelectedEntry.successMessage',
                            });
                        }
                        this.reload.next();
                    }
                },
                error: () => this.notificationService.error(this.translateService.instant('entry.notification.editMultipleError')),
            });
    }

    openHeaderEdit(): void {
        void this.router.navigate(['watchlist', 'updateOptionalChipFields'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    openCommentEdit(): void {
        void this.router.navigate(['watchlist', 'updateComments'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    openConceptSystemEdit(): void {
        void this.router.navigate(['watchlist', 'updateConceptSystem'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId), collectionId: this.selection.selected[0].collection.id},
        });
    }

    removeSelected(): void {
        this.watchlistService.remove(...this.selection.selected.map(entry => entry.entryId));
        this.selection.clear();
    }

    removeAll(): void {
        this.watchlistService.removeAll();
        this.selection.clear();
    }

    updateAreAllVisibleSelected() {
        this.areAllVisibleSelected = this.getVisibleItems().every(i => this.selection.isSelected(i));
    }

    openMultipleEdit(): void {
        void this.router.navigate(['watchlist', 'updateMandatoryHeaderFields'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    openMultipleSearchAndReplace(): void {
        void this.router.navigate(['watchlist', 'multipleSearchAndReplace'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    openSourceEdit(): void {
        void this.router.navigate(['watchlist', 'updateSource'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    openMoveContent(): void {
        void this.router.navigate(['watchlist', 'moveContent'], {
            state: {ids: this.selection.selected.map(entry => entry.entryId)},
        });
    }

    private getVisibleItems(): WatchlistExtendedEntry[] {
        // DataSource.sortData applies sort to provided array, so an array copy should be used, otherwise
        // resetting of sort will not work, since direction is empty string and sortData defaults to asc.
        const data = [...this.dataSource.filteredData.filter(e => !e.locked)];
        const filteredSortedData = this.dataSource.sort ? this.dataSource.sortData(data, this.dataSource.sort) : data;

        const paginator = this.dataSource.paginator!;
        const startIndex = paginator.pageIndex * paginator.pageSize;
        return filteredSortedData.slice(startIndex, startIndex + paginator.pageSize);
    }

    private getSelectedAndSortedEntryIds(): number[] {
        const selectedIds = this.selection.selected.map(item => item.entryId);
        const sorted = this.dataSource.sortData(this.dataSource.filteredData, this.dataSource.sort!).map(item => item.entryId);
        return sorted.filter(id => selectedIds.includes(id));
    }

    private getStatusTranslation(status: number): string {
        return this.translateService.instant(`field.status.${Status[status]}`);
    }
}
