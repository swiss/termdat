import {ComponentFixture, fakeAsync, TestBed, tick} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatMenuModule} from '@angular/material/menu';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObInputClearModule, ObNotificationService} from '@oblique/oblique';
import {ConceptSystemDetail} from 'api/ConceptSystemDetail';
import {Status} from 'api/Status';
import {WatchlistEntry} from 'api/WatchlistEntry';
import {MockModule} from 'ng-mocks';
import {BehaviorSubject, of, Subject, throwError} from 'rxjs';
import {ConceptSystemService} from '../../administration/concept-system/concept-system.service';
import {AuthService} from '../../auth/auth.service';
import {EntryService} from '../../entry/entry.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {authenticatedPageSize} from '../../shared/PaginationMapper';
import {WatchlistService} from '../watchlist.service';
import {WatchlistExtendedEntry} from '../WatchlistExtendedEntry';
import {WatchlistFormComponent} from './watchlist-form.component';

/* eslint-disable dot-notation */
describe('WatchlistFormComponent', () => {
    let component: WatchlistFormComponent;
    let fixture: ComponentFixture<WatchlistFormComponent>;

    const onLangChangeSubject = new Subject<void>();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(),
    };

    const watchlistEntryIds = [1, 2, 3];
    const watchlistEntriesSubject = new BehaviorSubject(watchlistEntryIds);
    const watchlistServiceMock = {
        watchlist$: watchlistEntriesSubject.asObservable(),
        getEntries: jest.fn(),
        remove: jest.fn(),
        removeAll: jest.fn(),
        createExportPdf: jest.fn(),
        createExportXlsx: jest.fn(),
        createExportTxt: jest.fn(),
        updateSelectedItems: jest.fn(),
        displayUpdateInfoWarning: jest.fn(),
        readSelectedItems: jest.fn(() => []),
    };

    const conceptSystemServiceMock = {
        getConceptSystemDetails: jest.fn(),
    };

    const httpInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };

    const notificationServiceMock = {
        info: jest.fn(),
        error: jest.fn(),
        success: jest.fn(),
    };

    const isAdminSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);
    const isSuperUserubject = new BehaviorSubject(false);
    const authServiceMock = {
        isAdmin$: isAdminSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
        isSuperUser$: isSuperUserubject.asObservable(),
    };

    const entryServiceMock = {
        deleteMultiple: jest.fn(),
        restoreMultiple: jest.fn(),
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    const watchlistEntries = [
        {
            entryId: 1,
            identification: 1,
            conceptSystemPosition: {
                value: '1. ConceptSystem',
            },
            value: 'First Value',
            status: Status.InProgress,
            deleted: true,
            locked: false,
            collection: {
                code: 'COL01',
            },
            isMainOffice: true,
        },
        {
            entryId: 2,
            identification: 2,
            conceptSystemPosition: {
                value: '2. ConceptSystem',
            },
            value: 'Second Value',
            status: Status.Validated,
            deleted: true,
            locked: false,
            collection: {
                code: 'COL02',
            },
            isMainOffice: true,
        },
        {
            entryId: 3,
            identification: 3,
            conceptSystemPosition: {
                value: '3. ConceptSystem',
            },
            value: 'Third Value',
            status: Status.Validated,
            deleted: false,
            locked: false,
            collection: {
                id: 3,
                code: 'COL03',
            },
            isMainOffice: false,
        },
        {
            entryId: 4,
            identification: 4,
            conceptSystemPosition: {
                value: '4. ConceptSystem',
            },
            value: 'Forth Value',
            status: Status.Validated,
            deleted: false,
            locked: false,
            collection: {
                id: 3,
                code: 'COL03',
            },
            isMainOffice: false,
        },
        {
            entryId: 5,
            identification: 5,
            conceptSystemPosition: {
                value: '5. ConceptSystem',
            },
            value: 'Fifth Value',
            status: Status.Validated,
            deleted: false,
            locked: true,
            collection: {
                code: 'COL01',
            },
            isMainOffice: false,
        },
    ] as WatchlistEntry[];
    const watchlistExtendedEntries = [
        {
            ...watchlistEntries[0],
            id: 1,
            statusTranslation: 'field.status.InProgress',
            isValidated: false,
            searchPhrase: 'EntryId:1',
        },
        {
            ...watchlistEntries[1],
            id: 2,
            statusTranslation: 'field.status.Validated',
            isValidated: true,
            searchPhrase: 'EntryId:2',
        },
        {
            ...watchlistEntries[2],
            id: 3,
            statusTranslation: 'field.status.Validated',
            isValidated: true,
            searchPhrase: 'EntryId:3',
        },
        {
            ...watchlistEntries[3],
            id: 4,
            statusTranslation: 'field.status.Validated',
            isValidated: true,
            searchPhrase: 'EntryId:4',
        },
        {
            ...watchlistEntries[4],
            id: 5,
            statusTranslation: 'field.status.Validated',
            isValidated: true,
            searchPhrase: 'EntryId:5',
        },
    ] as WatchlistExtendedEntry[];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(TranslateModule),
                MockModule(MatFormFieldModule),
                MockModule(MatInputModule),
                MockModule(MatSortModule),
                MockModule(MatCheckboxModule),
                MockModule(MatMenuModule),
                MockModule(MatDialogModule),
                MockModule(MatTooltipModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockModule(ObInputClearModule),
                MockModule(MatPaginatorModule),
                MatTableModule,
                WatchlistFormComponent,
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpInterceptorEventsMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ConceptSystemService, useValue: conceptSystemServiceMock},
            ],
        }).compileComponents();

        const conceptSystems = [
            {
                id: 10000,
                title: 'ConceptSystem1',
                collection: 'COL01',
                createdBy: 'test@test.ch',
                entryCount: 0,
                modifiedBy: 'test2@test.ch',
                modified: new Date('2013-02-22T23:05:05.78'),
                created: new Date('2015-02-22T23:05:05.78'),
            },
            {
                id: 20000,
                title: 'ConceptSystem2',
                collection: 'COL02',
                createdBy: 'test3@test.ch',
                entryCount: 1,
                modifiedBy: 'test4@test.ch',
                modified: new Date('2014-02-22T23:05:05.78'),
                created: new Date('2016-02-22T23:05:05.78'),
            },
            {
                id: 30000,
                title: 'ConceptSystem3',
                collection: 'COL03',
            },
        ] as ConceptSystemDetail[];

        entryServiceMock.deleteMultiple.mockReturnValue(of([]));
        entryServiceMock.restoreMultiple.mockReturnValue(of([]));
        dialogMock.open.mockReturnValue(dialogRefMock);
        isAdminSubject.next(false);
        isWriterSubject.next(false);
        isSuperUserubject.next(false);
        watchlistServiceMock.getEntries.mockReturnValue(of(watchlistEntries));
        translateServiceMock.instant.mockImplementation(input => input);
        conceptSystemServiceMock.getConceptSystemDetails.mockReturnValue(of(conceptSystems));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(WatchlistFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.getEntries).toHaveBeenCalledWith(watchlistEntryIds);
        expect(component.dataSource.data).toEqual(watchlistExtendedEntries);
    });

    it('should apply Filter and clear Filter', () => {
        jest.spyOn(component['filterSubject'], 'next');

        component.applyFilter({target: {value: 'InProgress'} as unknown} as Event);
        expect(component['filterSubject'].next).toHaveBeenCalledTimes(1);
        expect(component['filterSubject'].next).toHaveBeenCalledWith('InProgress');

        component.clearFilter();
        expect(component['filterSubject'].next).toHaveBeenCalledTimes(2);
        expect(component['filterSubject'].next).toHaveBeenCalledWith('');
    });

    it('should subscribe to filter', fakeAsync(() => {
        jest.spyOn(component, 'updateAreAllVisibleSelected');
        jest.spyOn(component.dataSource.paginator!, 'firstPage');

        component['filterSubject'].next('   Second   ');
        tick(125);
        fixture.detectChanges();

        expect(component.dataSource.filter).toBe('SECOND');
        expect(component.dataSource.paginator?.firstPage).toHaveBeenCalled();
        expect(component.updateAreAllVisibleSelected).toHaveBeenCalled();
    }));

    it('should assign sortingDataAccessor with dot notation', () => {
        const item = {
            nested: {
                property: {
                    value: true,
                },
            },
        };

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const val = component.dataSource.sortingDataAccessor(item as any, 'nested.property.value');

        expect(val).toBe(true);
    });

    it.each([
        ['FIRST VALUE', true],
        ['FIRST', true],
        ['IRS', true],
        ['VALU', true],
        ['CONCEPT', true],
        ['COL', true],
        ['01', true],
        ['1', true],
        ['PROGRESS', true],
        ['FALSE', false],
        ['02', false],
        ['GIBBERISH', false],
    ])('should use correct filter predicate', (filter: string, expected: boolean) => {
        const item = {
            ...{
                entryId: 1,
                identification: 1,
                conceptSystemPosition: {
                    value: '1. ConceptSystem',
                },
                value: 'First Value',
                status: Status.InProgress,
                collection: {
                    code: 'COL01',
                },
                isMainOffice: true,
            },
            id: 1,
            statusTranslation: 'field.status.InProgress',
            isValidated: false,
            searchPhrase: 'EntryId:1',
        } as WatchlistExtendedEntry;
        const filterMatches = component.dataSource.filterPredicate(item, filter);
        expect(filterMatches).toBe(expected);
    });

    it('should select/deselect all visible entries and know about selection state', () => {
        component.dataSource.filteredData = watchlistExtendedEntries;
        component.dataSource.paginator!.pageIndex = 0;
        fixture.detectChanges();
        component.selection.clear();
        component.updateAreAllVisibleSelected();

        component.masterToggle();

        expect(component.areAllVisibleSelected).toBeTruthy();

        const ids = component.selection.selected.map(entry => entry.entryId);
        expect(watchlistServiceMock.updateSelectedItems).toHaveBeenCalledWith(ids);

        component.masterToggle();
        expect(component.areAllVisibleSelected).toBeFalsy();
        expect(watchlistServiceMock.updateSelectedItems).toHaveBeenCalledWith([]);

        expect(watchlistServiceMock.updateSelectedItems).toHaveBeenCalledTimes(2);
    });

    it('should navigate to multi header data edit', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);

        component.openHeaderEdit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist', 'updateOptionalChipFields'], {state: {ids: [3, 4]}});
    });

    it('should navigate to multi edit header', () => {
        component.selection.select(watchlistExtendedEntries[2]);

        component.openMultipleEdit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist', 'updateMandatoryHeaderFields'], {state: {ids: [3]}});
    });

    it('should navigate to multi comment edit', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);

        component.openCommentEdit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist', 'updateComments'], {state: {ids: [3, 4]}});
    });

    it('should navigate to multi concept system edit', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);

        component.openConceptSystemEdit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist', 'updateConceptSystem'], {state: {ids: [3, 4], collectionId: 3}});
    });

    it('should navigate to multi source edit', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);

        component.openSourceEdit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist', 'updateSource'], {state: {ids: [3, 4]}});
    });

    it('should remove selected items', () => {
        component.selection.select(watchlistExtendedEntries[1]);
        component.selection.select(watchlistExtendedEntries[2]);
        expect(component.selection.selected.length).toBe(2);
        expect(component.dataSource.data.length).toBe(5);

        component.removeSelected();

        expect(watchlistServiceMock.remove).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.remove).toHaveBeenCalledWith(watchlistExtendedEntries[1].entryId, watchlistExtendedEntries[2].entryId);
        expect(component.selection.isEmpty()).toBeTruthy();
    });

    it('should remove all items', fakeAsync(async () => {
        component.removeAll();

        expect(watchlistServiceMock.removeAll).toHaveBeenCalledTimes(1);
        expect(component.selection.isEmpty()).toBeTruthy();
    }));

    it('should set boolean flags correct for selected and deleted entry', () => {
        component.selection.select(watchlistExtendedEntries[0]);

        component.areAllSelectedDeleted$.subscribe(deleted => expect(deleted).toBeTruthy());
        component.areAllSelectedNotDeleted$.subscribe(notDeleted => expect(notDeleted).toBeFalsy());
    });

    it('should set boolean flags correct for selected and not deleted entry', () => {
        component.selection.select(watchlistExtendedEntries[2]);

        component.areAllSelectedDeleted$.subscribe(deleted => expect(deleted).toBeFalsy());
        component.areAllSelectedNotDeleted$.subscribe(notDeleted => expect(notDeleted).toBeTruthy());
    });

    describe('hard-delete-selected', () => {
        it('should open dialog on hard delete for single', () => {
            component.selection.select(watchlistExtendedEntries[1]);
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.hardDeleteSelected();

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'watchlist.hardDeleteSelectedEntry.confirmTitle',
                    text: 'watchlist.hardDeleteSelectedEntry.confirmText',
                    textValue: {count: 1},
                    confirm: 'watchlist.hardDeleteSelectedEntries.confirmButton',
                },
                minWidth: '500px',
                width: 'auto',
                maxWidth: '90vw',
            });
        });

        it('should open dialog on hard delete for multiple', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            component.selection.select(watchlistExtendedEntries[1]);
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.hardDeleteSelected();

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'watchlist.hardDeleteSelectedEntries.confirmTitle',
                    text: 'watchlist.hardDeleteSelectedEntries.confirmText',
                    textValue: {count: 2},
                    confirm: 'watchlist.hardDeleteSelectedEntries.confirmButton',
                },
                minWidth: '500px',
                width: 'auto',
                maxWidth: '90vw',
            });
        });

        it('should not call service when hard delete dialog aborted', () => {
            component.selection.select(watchlistExtendedEntries[1]);
            dialogRefMock.afterClosed.mockReturnValue(of(false));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(0);
        });

        it('should hard delete selected items when dialog confirmed and show success', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            component.selection.select(watchlistExtendedEntries[1]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId, watchlistExtendedEntries[1].entryId], false);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'watchlist.hardDeleteSelectedEntries.successTitle',
                message: 'watchlist.hardDeleteSelectedEntries.successMessage',
            });
            expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
        });

        it('should hard delete selected item when dialog confirmed and show success', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId], false);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'watchlist.hardDeleteSelectedEntry.successTitle',
                message: 'watchlist.hardDeleteSelectedEntry.successMessage',
            });
            expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
        });

        it('should clear selection after hard delete', () => {
            jest.spyOn(component.selection, 'clear');
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.hardDeleteSelected();

            expect(component.selection.clear).toHaveBeenCalledTimes(1);
        });

        it('should display update warnings', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.deleteMultiple.mockReturnValue(of([{notSoftDeletedEntryId: watchlistExtendedEntries[0].id}]));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId], false);
            expect(watchlistServiceMock.displayUpdateInfoWarning).toHaveBeenCalledTimes(1);
        });

        it('should handle error', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.deleteMultiple.mockReturnValue(throwError(() => ({})));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId], false);
            expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
        });
    });

    describe('soft-delete-selected', () => {
        it('should open dialog on delete single', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.softDeleteSelected();

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'watchlist.deleteSelectedEntry.confirmTitle',
                    text: 'watchlist.deleteSelectedEntry.confirmText',
                    textValue: {count: 1},
                    confirm: 'watchlist.deleteSelectedEntries.confirmButton',
                },
            });
        });

        it('should open dialog on delete multiple', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            component.selection.select(watchlistExtendedEntries[3]);
            dialogRefMock.afterClosed.mockReturnValue(of());
            component.softDeleteSelected();

            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'watchlist.deleteSelectedEntries.confirmTitle',
                    text: 'watchlist.deleteSelectedEntries.confirmText',
                    textValue: {count: 2},
                    confirm: 'watchlist.deleteSelectedEntries.confirmButton',
                },
            });
        });

        it('should not call service when delete dialog aborted', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            dialogRefMock.afterClosed.mockReturnValue(of(false));

            component.softDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(0);
        });

        it('should delete selected item when dialog confirmed and show success', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.softDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[2].entryId]);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'watchlist.deleteSelectedEntry.successTitle',
                message: 'watchlist.deleteSelectedEntry.successMessage',
            });
            expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
        });

        it('should delete selected items when dialog confirmed and show success', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            component.selection.select(watchlistExtendedEntries[3]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.softDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[2].entryId, watchlistExtendedEntries[3].entryId]);
            expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.success).toHaveBeenCalledWith({
                title: 'watchlist.deleteSelectedEntries.successTitle',
                message: 'watchlist.deleteSelectedEntries.successMessage',
            });
            expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
        });

        it('should clear selection after delete', () => {
            jest.spyOn(component.selection, 'clear');
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));

            component.hardDeleteSelected();

            expect(component.selection.clear).toHaveBeenCalledTimes(1);
        });

        it('should display update warnings', () => {
            component.selection.select(watchlistExtendedEntries[0]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.deleteMultiple.mockReturnValue(of([{lockedEntryId: watchlistExtendedEntries[0].id}]));

            component.hardDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId], false);
            expect(watchlistServiceMock.displayUpdateInfoWarning).toHaveBeenCalledTimes(1);
        });

        it('should handle error', () => {
            component.selection.select(watchlistExtendedEntries[2]);
            component.selection.select(watchlistExtendedEntries[3]);
            dialogRefMock.afterClosed.mockReturnValue(of(true));
            entryServiceMock.deleteMultiple.mockReturnValue(throwError(() => ({})));

            component.softDeleteSelected();

            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledTimes(1);
            expect(entryServiceMock.deleteMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[2].entryId, watchlistExtendedEntries[3].entryId]);
            expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
        });
    });

    it('should open dialog on restoring single entry', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.restoreSelected();

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'watchlist.restoreSelectedEntry.confirmTitle',
                text: 'watchlist.restoreSelectedEntry.confirmText',
                textValue: {count: 1},
                confirm: 'watchlist.restoreSelectedEntries.confirmButton',
            },
        });
    });
    it('should open dialog on restoring multiple entries', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.restoreSelected();

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'watchlist.restoreSelectedEntries.confirmTitle',
                text: 'watchlist.restoreSelectedEntries.confirmText',
                textValue: {count: 2},
                confirm: 'watchlist.restoreSelectedEntries.confirmButton',
            },
        });
    });
    it('should not call service when restore dialog aborted', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        dialogRefMock.afterClosed.mockReturnValue(of(false));

        component.restoreSelected();

        expect(entryServiceMock.restoreMultiple).toHaveBeenCalledTimes(0);
    });
    it('should restore selected item when dialog confirmed and show success', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.restoreSelected();

        expect(entryServiceMock.restoreMultiple).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.restoreMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[2].entryId]);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'watchlist.restoreSelectedEntry.successTitle',
            message: 'watchlist.restoreSelectedEntry.successMessage',
        });
        expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
    });
    it('should restore selected items when dialog confirmed and show success', () => {
        component.selection.select(watchlistExtendedEntries[2]);
        component.selection.select(watchlistExtendedEntries[3]);
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.restoreSelected();

        expect(entryServiceMock.restoreMultiple).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.restoreMultiple).toHaveBeenCalledWith([watchlistExtendedEntries[2].entryId, watchlistExtendedEntries[3].entryId]);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'watchlist.restoreSelectedEntries.successTitle',
            message: 'watchlist.restoreSelectedEntries.successMessage',
        });
        expect(watchlistServiceMock.getEntries).toHaveBeenCalledTimes(2);
    });

    it('should clear selection after restoring', () => {
        jest.spyOn(component.selection, 'clear');
        component.selection.select(watchlistExtendedEntries[0]);
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.restoreSelected();

        expect(component.selection.clear).toHaveBeenCalledTimes(1);
    });

    it('should add maximum watchlist entry count to pageSizeOptions when data exceeds pageSizeOptions maximum', () => {
        const generatedCount = authenticatedPageSize[authenticatedPageSize.length - 1] + 5;
        const generatedEntries = [];
        for (let i = 0; i < generatedCount; i++) {
            generatedEntries.push(watchlistEntries[0]);
        }

        watchlistServiceMock.getEntries.mockReturnValue(of(generatedEntries));
        onLangChangeSubject.next();

        expect(component.dataSource.data.length).toEqual(generatedCount);
        expect(component.pageSizeOptions.includes(generatedCount)).toEqual(true);
    });

    it('should remove maximum watchlist entry count to pageSizeOptions when data does not exceed pageSizeOptions maximum', () => {
        const generatedCount = authenticatedPageSize[authenticatedPageSize.length - 1] + 5;
        const generatedEntries = [];
        for (let i = 0; i < generatedCount; i++) {
            generatedEntries.push(watchlistEntries[0]);
        }

        watchlistServiceMock.getEntries.mockReturnValue(of(generatedEntries));
        onLangChangeSubject.next();

        expect(component.dataSource.data.length).toEqual(generatedCount);

        watchlistServiceMock.getEntries.mockReturnValue(of(watchlistEntries));
        onLangChangeSubject.next();

        expect(component.dataSource.data.length).toEqual(5);
        expect(component.pageSizeOptions.includes(generatedCount)).toEqual(false);
    });

    it('should not get export when no selection', () => {
        component.exportPdf();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).not.toHaveBeenCalled();
        expect(watchlistServiceMock.createExportPdf).not.toHaveBeenCalled();
        expect(notificationServiceMock.error).not.toHaveBeenCalled();
        expect(notificationServiceMock.info).not.toHaveBeenCalled();
    });

    it('should get export only when any selection', () => {
        watchlistServiceMock.createExportPdf.mockReturnValue(of({}));
        component.dataSource.filteredData = watchlistExtendedEntries;
        component.selection.select(watchlistExtendedEntries[1]);
        component.selection.select(watchlistExtendedEntries[2]);

        component.exportPdf();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.createExportPdf).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.createExportPdf).toHaveBeenCalledWith([watchlistExtendedEntries[1].id, watchlistExtendedEntries[2].id]);
        expect(notificationServiceMock.error).not.toHaveBeenCalled();
    });

    it('should export listed entry ids to txt via service', () => {
        component.dataSource.filteredData = watchlistExtendedEntries;
        component.selection.select(watchlistExtendedEntries[1]);
        component.selection.select(watchlistExtendedEntries[2]);

        component.exportListedEntryIdsToTxt();

        expect(watchlistServiceMock.createExportTxt).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.createExportTxt).toHaveBeenCalledWith([2, 3]);
    });

    it('shoud show error message on export error', () => {
        watchlistServiceMock.createExportPdf.mockReturnValue(throwError(() => ({})));
        component.dataSource.filteredData = watchlistExtendedEntries;
        component.selection.select(watchlistExtendedEntries[1]);

        component.exportPdf();

        expect(httpInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.createExportPdf).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.info).not.toHaveBeenCalled();
    });

    it('should toggle on toggleRow', () => {
        component.toggleRow(watchlistExtendedEntries[0]);

        expect(watchlistServiceMock.updateSelectedItems).toHaveBeenCalledTimes(1);
        expect(watchlistServiceMock.updateSelectedItems).toHaveBeenCalledWith([watchlistExtendedEntries[0].entryId]);
    });

    it('should set boolean true flag for selected and not deleted entry, which all have the same collection with concept system ', () => {
        component.selection.select(watchlistExtendedEntries[0], watchlistExtendedEntries[4]);

        component.areAllSuitableForConceptSystemPositionChange$.subscribe(result => expect(result).toBeTruthy());
    });

    it('should set boolean false flag for selected and not deleted entry, which dont have the same collection with concept system ', () => {
        component.selection.select(watchlistExtendedEntries[0], watchlistExtendedEntries[1]);

        component.areAllSuitableForConceptSystemPositionChange$.subscribe(result => expect(result).toBeTruthy());
    });

    it('should set boolean false flag for correct for selected and not deleted entry, which all have the same collection without concept system ', () => {
        component.selection.select(watchlistExtendedEntries[2], watchlistExtendedEntries[3]);

        component.areAllSuitableForConceptSystemPositionChange$.subscribe(result => expect(result).toBeTruthy());
    });
});
