import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObInputClearModule, ObNotificationService} from '@oblique/oblique';
import {CollectionDetail} from 'api/CollectionDetail';
import {MockComponent, MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ColumnsMenuComponent} from '../../shared/columns-menu/columns-menu.component';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {ErrorService} from '../../shared/error.service';
import {HyperlinkPipe} from '../../shared/hyperlink.pipe';
import {CollectionService} from './collection.service';
import {CollectionsComponent} from './collections.component';

describe('CollectionsComponent', () => {
    let component: CollectionsComponent;
    let fixture: ComponentFixture<CollectionsComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    let collections: CollectionDetail[] = [];
    const collectionServiceMock = {
        getCollectionDetails: jest.fn(),
        delete: jest.fn(),
        restore: jest.fn(),
    };

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(),
    };

    const notificationServiceMock = {
        success: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        clear: jest.fn(),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MatPaginatorModule,
                MatFormFieldModule,
                MatTableModule,
                MatInputModule,
                MatSortModule,
                MatTooltipModule,
                MatButtonModule,
                MockModule(MatIconModule),
                MockModule(TranslateModule),
                MockModule(ObInputClearModule),
                CollectionsComponent,
                MockPipe(HyperlinkPipe),
                MockDirective(RouterLink),
                MockComponent(ColumnsMenuComponent),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
            ],
        }).compileComponents();

        collections = [
            {
                id: 10000,
                name: 'Collection1',
                code: 'COL1',
                activeApi: true,
                active: true,
                entryCount: 10,
                isObsolete: false,
                deleted: false,
                classification: 'CLASS1',
                exportLindas: false,
            },
            {
                id: 20000,
                name: 'Collection2',
                code: 'COL2',
                activeApi: false,
                active: false,
                entryCount: 101,
                isObsolete: true,
                deleted: false,
                exportLindas: false,
            },
            {
                id: 30000,
                name: 'Collection3',
                code: 'COL3',
                activeApi: false,
                active: false,
                deleted: false,
                entryCount: 200,
                isObsolete: true,
                exportLindas: false,
            },
        ];

        collectionServiceMock.getCollectionDetails.mockReturnValue(of(collections));
        collectionServiceMock.delete.mockReturnValue(of(void 0));
        collectionServiceMock.restore.mockReturnValue(of(collections[0]));
        dialogMock.open.mockReturnValue(dialogRefMock);

        fixture = TestBed.createComponent(CollectionsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set paginator and sort', () => {
        expect(component.paginator).toBeDefined();
        expect(component.dataSource.paginator).toEqual(component.paginator);

        expect(component.sort).toBeDefined();
        expect(component.dataSource.sort).toEqual(component.sort);
    });

    it('should load collections on init', () => {
        expect(collectionServiceMock.getCollectionDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(collections);
    });

    it('should show all columns by default', () => {
        expect(component.displayedColumns).toEqual([
            'code',
            'name',
            'entryCount',
            'active',
            'isObsolete',
            'activeApi',
            'exportLindas',
            'classification',
            'action',
        ]);
    });

    it('should update displayed columns from shared columns menu output', () => {
        component.onDisplayedColumnsChange(['code', 'name', 'entryCount', 'isObsolete', 'activeApi', 'exportLindas', 'classification', 'action']);

        expect(component.displayedColumns).toEqual(['code', 'name', 'entryCount', 'isObsolete', 'activeApi', 'exportLindas', 'classification', 'action']);
    });

    it('should reload collections on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(collectionServiceMock.getCollectionDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(collections);
    });

    it.each([{filter: 'col1'}, {filter: '20000'}, {filter: 'collection1'}, {filter: 'lection1'}, {filter: 'class1'}, {filter: '101'}])(
        'should apply filter for $filter and clear filter',
        async ({filter}) => {
            expect(component.dataSource.filteredData.length).toBe(3);

            component.applyFilter({target: {value: filter} as unknown} as Event);

            fixture.detectChanges();
            await fixture.whenStable();

            expect(component.dataSource.filteredData.length).toBe(1);

            component.clearFilter();

            fixture.detectChanges();
            await fixture.whenStable();

            expect(component.dataSource.filteredData.length).toBe(3);
        }
    );

    it('can apply empty filter', async () => {
        component.applyFilter({target: {value: null} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: undefined} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('can apply filter and clear filter without paginator', async () => {
        // @ts-ignore
        component.paginator = null;
        component.dataSource.paginator = null;

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: 'collection1'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should navigate on update', () => {
        component.update({} as CollectionDetail);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('can open dialog on soft delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.softDelete(collections[0]);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'collection.softDelete.title',
                text: 'collection.softDelete.text',
                textValue: {...collections[0], total: collections[0].entryCount},
                confirm: 'collection.softDelete.confirm',
            },
        });
    });

    it('can soft delete only after dialog is confirmed', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.softDelete({id: 1} as CollectionDetail);

        expect(collectionServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.delete).toHaveBeenCalledWith(1, true);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'collection.softDelete.successTitle',
            message: 'collection.softDelete.successMessage',
        });
    });

    it('can not soft delete when afterClosed returns null or undefined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(null));

        component.softDelete(collections[0]);

        expect(collectionServiceMock.delete).toHaveBeenCalledTimes(0);
    });

    it('can open dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.delete(collections[0]);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'collection.delete.title',
                text: 'collection.delete.text',
                textValue: {...collections[0]},
                confirm: 'collection.delete.confirm',
            },
        });
    });

    it('can delete only after dialog is confirmed', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete({id: 1} as CollectionDetail);

        expect(collectionServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.delete).toHaveBeenCalledWith(1, false);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'collection.delete.successTitle',
            message: 'collection.delete.successMessage',
        });
        expect(errorServiceMock.handleHttpError).not.toHaveBeenCalled();
    });

    it('should handle error on delete', () => {
        collectionServiceMock.delete.mockReturnValue(throwError(() => ({})));
        dialogRefMock.afterClosed.mockReturnValue(of(true));

        component.delete({id: 1} as CollectionDetail);

        expect(collectionServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.delete).toHaveBeenCalledWith(1, false);
        expect(notificationServiceMock.success).not.toHaveBeenCalled();
        expect(errorServiceMock.handleHttpError).toHaveBeenCalledTimes(1);
    });

    it('can not delete when afterClosed returns null or undefined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(null));

        component.delete(collections[0]);

        expect(collectionServiceMock.delete).toHaveBeenCalledTimes(0);
    });

    it('can restore collection', () => {
        const restoreCollection = {id: 10} as CollectionDetail;
        component.restore(restoreCollection);

        expect(collectionServiceMock.restore).toHaveBeenCalledTimes(1);
        expect(collectionServiceMock.restore).toHaveBeenCalledWith(10);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'collection.restore.successTitle',
            message: 'collection.restore.successMessage',
        });
        expect(restoreCollection.id).toEqual(10);
        expect(restoreCollection.deleted).toEqual(false);
    });
});
