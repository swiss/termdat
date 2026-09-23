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
import {GlossaryDetail} from 'api/GlossaryDetail';
import {MockComponent, MockDirective, MockModule} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {ColumnsMenuComponent} from '../../shared/columns-menu/columns-menu.component';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {GlossariesComponent} from './glossaries.component';
import {GlossaryService} from './glossary.service';

describe('GlossariesComponent', () => {
    let component: GlossariesComponent;
    let fixture: ComponentFixture<GlossariesComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    let glossaries: GlossaryDetail[] = [];
    const glossaryServiceMock = {
        getGlossaryDetails: jest.fn(),
        delete: jest.fn(),
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
                GlossariesComponent,
                MockDirective(RouterLink),
                MockComponent(ColumnsMenuComponent),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
            ],
        }).compileComponents();

        glossaries = [
            {
                id: 10000,
                name: 'Glossary1',
                code: 'Glo1',
                description: 'description1',
                entryCount: 10,
                createdBy: 'test@test.ch',
            },
            {
                id: 20000,
                name: 'Glossary2',
                code: 'Glo2',
                description: 'description2',
                entryCount: 101,
                createdBy: 'test3@test.ch',
            },
            {
                id: 30000,
                name: 'Glossary3',
                code: 'Glo3',
                entryCount: 200,
            } as GlossaryDetail,
        ];
        glossaryServiceMock.getGlossaryDetails.mockReturnValue(of(glossaries));
        glossaryServiceMock.delete.mockReturnValue(of(glossaries[0]));
        dialogMock.open.mockReturnValue(dialogRefMock);
    });

    beforeEach(async () => {
        fixture = TestBed.createComponent(GlossariesComponent);
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

    it('should load glossaries on init', () => {
        expect(glossaryServiceMock.getGlossaryDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(glossaries);
    });

    it('should show all columns by default', () => {
        expect(component.displayedColumns).toEqual(['code', 'name', 'description', 'entryCount', 'createdBy', 'action']);
    });

    it('should update displayed columns from shared columns menu output', () => {
        component.onDisplayedColumnsChange(['code', 'name', 'description', 'entryCount', 'createdBy', 'action']);

        expect(component.displayedColumns).toEqual(['code', 'name', 'description', 'entryCount', 'createdBy', 'action']);
    });

    it('should reload glossaries on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(glossaryServiceMock.getGlossaryDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(glossaries);
    });

    it.each([
        {filter: 'glo1'},
        {filter: '20000'},
        {filter: 'glossary1'},
        {filter: 'ssary1'},
        {filter: 'descrIPTIOn1'},
        {filter: '101'},
        {filter: 'test@test.ch'},
    ])('should apply filter for $filter and clear filter', async ({filter}) => {
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(3);
    });

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

        component.applyFilter({target: {value: 'glossary1'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should navigate on update', () => {
        component.update({} as GlossaryDetail);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('can open dialog on delete', () => {
        dialogRefMock.afterClosed.mockReturnValue(of());
        component.delete(glossaries[0]);

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(ConfirmDeleteDialogComponent, {
            data: {
                title: 'glossary.delete.title',
                text: 'glossary.delete.text',
                textValue: {...glossaries[0], total: glossaries[0].entryCount},
                confirm: 'glossary.delete.confirm',
            },
        });
    });

    it('can delete only after dialog is confirmed', () => {
        const deleteDetail = {id: 1} as GlossaryDetail;
        dialogRefMock.afterClosed.mockReturnValue(of(deleteDetail));

        component.delete(deleteDetail);

        expect(glossaryServiceMock.delete).toHaveBeenCalledTimes(1);
        expect(extendedSearchFiltersServiceMock.clear).toHaveBeenCalledTimes(1);
        expect(glossaryServiceMock.delete).toHaveBeenCalledWith(1);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
        expect(notificationServiceMock.success).toHaveBeenCalledWith({
            title: 'glossary.delete.successTitle',
            message: 'glossary.delete.successMessage',
        });
        expect(deleteDetail.id).toEqual(1);
    });

    it('can not delete when afterClosed returns null or undefined', () => {
        dialogRefMock.afterClosed.mockReturnValue(of(null));

        component.delete(glossaries[0]);

        expect(glossaryServiceMock.delete).toHaveBeenCalledTimes(0);
    });
});
