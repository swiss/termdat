import {SimpleChange} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatIcon} from '@angular/material/icon';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {MockComponent, MockPipe} from 'ng-mocks';
import {ColumnsMenuComponent, TableColumn} from './columns-menu.component';

describe('ColumnsMenuComponent', () => {
    let component: ColumnsMenuComponent;
    let fixture: ComponentFixture<ColumnsMenuComponent>;

    const mockColumns: TableColumn[] = [
        {key: 'id', label: 'ID', toggleable: false},
        {key: 'name', label: 'Name', toggleable: true},
        {key: 'email', label: 'Email', toggleable: true},
        {key: 'status', label: 'Status', toggleable: true},
    ];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ColumnsMenuComponent,
                MatButton,
                MatMenuTrigger,
                MockComponent(MatIcon),
                MatMenu,
                MatMenuItem,
                MatCheckbox,
                MatTooltip,
                ObButtonDirective,
                MockPipe(TranslatePipe),
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(ColumnsMenuComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Toggleable Columns Getter', () => {
        beforeEach(() => {
            component.columns = mockColumns;
        });

        it('should return only toggleable columns', () => {
            const toggleable = component.toggleableColumns;
            expect(toggleable).toHaveLength(3);
            expect(toggleable).toEqual([
                {key: 'name', label: 'Name', toggleable: true},
                {key: 'email', label: 'Email', toggleable: true},
                {key: 'status', label: 'Status', toggleable: true},
            ]);
        });

        it('should exclude non-toggleable columns', () => {
            const toggleable = component.toggleableColumns;
            expect(toggleable.some(col => col.key === 'id')).toBeFalsy();
        });

        it('should return empty array when no columns are toggleable', () => {
            component.columns = [
                {key: 'id', label: 'ID', toggleable: false},
                {key: 'timestamp', label: 'Timestamp', toggleable: false},
            ];
            expect(component.toggleableColumns).toHaveLength(0);
        });
    });

    describe('Column Visibility', () => {
        beforeEach(() => {
            component.columns = mockColumns;
            fixture.detectChanges();
        });

        it('should show all columns by default', () => {
            component.toggleableColumns.forEach(column => {
                expect(component.isColumnVisible(column.key)).toBeTruthy();
            });
        });

        it('should hide a column when setColumnVisible is called with false', () => {
            component.setColumnVisible('name', false);
            expect(component.isColumnVisible('name')).toBeFalsy();
        });

        it('should show a hidden column when setColumnVisible is called with true', () => {
            component.setColumnVisible('name', false);
            expect(component.isColumnVisible('name')).toBeFalsy();

            component.setColumnVisible('name', true);
            expect(component.isColumnVisible('name')).toBeTruthy();
        });

        it('should not toggle non-toggleable columns', () => {
            component.setColumnVisible('id', false);
            expect(component.isColumnVisible('id')).toBeTruthy();
        });

        it('should not toggle columns that do not exist', () => {
            component.setColumnVisible('nonexistent', false);
            expect(component.displayedColumns).toEqual(['id', 'name', 'email', 'status']);
        });

        it('should handle toggling multiple columns independently', () => {
            component.setColumnVisible('name', false);
            component.setColumnVisible('email', false);

            expect(component.isColumnVisible('name')).toBeFalsy();
            expect(component.isColumnVisible('email')).toBeFalsy();
            expect(component.isColumnVisible('status')).toBeTruthy();
        });
    });

    describe('All Columns Visible Getter', () => {
        beforeEach(() => {
            component.columns = mockColumns;
            fixture.detectChanges();
        });

        it('should return true when all toggleable columns are visible', () => {
            expect(component.allColumnsVisible).toBeTruthy();
        });

        it('should return false when at least one toggleable column is hidden', () => {
            component.setColumnVisible('name', false);
            expect(component.allColumnsVisible).toBeFalsy();
        });

        it('should return true after hiding and then showing all columns again', () => {
            component.setColumnVisible('name', false);
            component.setColumnVisible('email', false);
            expect(component.allColumnsVisible).toBeFalsy();

            component.setColumnVisible('name', true);
            component.setColumnVisible('email', true);
            expect(component.allColumnsVisible).toBeTruthy();
        });

        it('should ignore non-toggleable columns when checking visibility', () => {
            component.setColumnVisible('name', false);
            expect(component.allColumnsVisible).toBeFalsy();
        });
    });

    describe('Set All Columns Visible', () => {
        beforeEach(() => {
            component.columns = mockColumns;
            fixture.detectChanges();
        });

        it('should show all toggleable columns when called with true', () => {
            component.setColumnVisible('name', false);
            component.setColumnVisible('email', false);

            component.setAllColumnsVisible(true);

            expect(component.isColumnVisible('name')).toBeTruthy();
            expect(component.isColumnVisible('email')).toBeTruthy();
            expect(component.isColumnVisible('status')).toBeTruthy();
        });

        it('should hide all toggleable columns when called with false', () => {
            component.setAllColumnsVisible(false);

            expect(component.isColumnVisible('name')).toBeFalsy();
            expect(component.isColumnVisible('email')).toBeFalsy();
            expect(component.isColumnVisible('status')).toBeFalsy();
        });

        it('should not affect non-toggleable columns', () => {
            component.setAllColumnsVisible(false);
            expect(component.isColumnVisible('id')).toBeTruthy();
        });

        it('should update allColumnsVisible getter accordingly', () => {
            expect(component.allColumnsVisible).toBeTruthy();

            component.setAllColumnsVisible(false);
            expect(component.allColumnsVisible).toBeFalsy();

            component.setAllColumnsVisible(true);
            expect(component.allColumnsVisible).toBeTruthy();
        });
    });

    describe('Displayed Columns Getter', () => {
        beforeEach(() => {
            component.columns = mockColumns;
            fixture.detectChanges();
        });

        it('should include all columns by default', () => {
            expect(component.displayedColumns).toEqual(['id', 'name', 'email', 'status']);
        });

        it('should exclude hidden toggleable columns', () => {
            component.setColumnVisible('name', false);
            component.setColumnVisible('email', false);
            expect(component.displayedColumns).toEqual(['id', 'status']);
        });

        it('should always include non-toggleable columns', () => {
            component.setAllColumnsVisible(false);
            expect(component.displayedColumns).toContain('id');
        });

        it('should maintain column order', () => {
            component.setColumnVisible('email', false);
            expect(component.displayedColumns).toEqual(['id', 'name', 'status']);
        });

        it('should reflect changes after visibility updates', () => {
            expect(component.displayedColumns).toEqual(['id', 'name', 'email', 'status']);

            component.setColumnVisible('name', false);
            expect(component.displayedColumns).toEqual(['id', 'email', 'status']);

            component.setColumnVisible('name', true);
            expect(component.displayedColumns).toEqual(['id', 'name', 'email', 'status']);
        });
    });

    describe('Event Emission', () => {
        beforeEach(() => {
            component.columns = mockColumns;
            fixture.detectChanges();
        });

        it('should emit displayedColumnsChange when a column visibility changes', () => {
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.setColumnVisible('name', false);

            expect(component.displayedColumnsChange.emit).toHaveBeenCalledWith(['id', 'email', 'status']);
        });

        it('should emit displayedColumnsChange when all columns visibility changes', () => {
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.setAllColumnsVisible(false);

            expect(component.displayedColumnsChange.emit).toHaveBeenCalledWith(['id']);
        });

        it('should emit with all columns when showing all', () => {
            component.setAllColumnsVisible(false);
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.setAllColumnsVisible(true);

            expect(component.displayedColumnsChange.emit).toHaveBeenCalledWith(['id', 'name', 'email', 'status']);
        });

        it('should emit multiple times for multiple changes', () => {
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.setColumnVisible('name', false);
            component.setColumnVisible('email', false);
            component.setColumnVisible('status', false);

            expect(component.displayedColumnsChange.emit).toHaveBeenCalledTimes(3);
        });
    });

    describe('ngOnChanges', () => {
        it('should update hiddenColumns when columns input changes', () => {
            component.columns = mockColumns;
            component.setColumnVisible('name', false);

            const newColumns = [
                {key: 'id', label: 'ID', toggleable: false},
                {key: 'name', label: 'Name', toggleable: true},
            ];
            component.columns = newColumns;
            component.ngOnChanges({columns: new SimpleChange(mockColumns, newColumns, true)});

            expect(component.isColumnVisible('email')).toBeTruthy();
        });

        it('should emit displayedColumns after columns change', () => {
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.columns = mockColumns;
            component.ngOnChanges({columns: new SimpleChange(undefined, mockColumns, true)});

            expect(component.displayedColumnsChange.emit).toHaveBeenCalled();
        });

        it('should remove hidden state for columns that no longer exist', () => {
            component.columns = mockColumns;
            component.setColumnVisible('email', false);
            expect(component.isColumnVisible('email')).toBeFalsy();

            const newColumns = [
                {key: 'id', label: 'ID', toggleable: false},
                {key: 'name', label: 'Name', toggleable: true},
            ];
            component.columns = newColumns;
            component.ngOnChanges({columns: new SimpleChange(mockColumns, newColumns, true)});

            // email should not be tracked anymore, so we shouldn't be able to hide it
            expect(component.displayedColumns).not.toContain('email');
        });

        it('should not emit if columns input does not change', () => {
            component.columns = mockColumns;
            fixture.detectChanges();
            jest.spyOn(component.displayedColumnsChange, 'emit');

            component.ngOnChanges({});

            expect(component.displayedColumnsChange.emit).not.toHaveBeenCalled();
        });

        it('should handle empty columns array', () => {
            component.columns = mockColumns;
            component.setColumnVisible('name', false);

            component.columns = [];
            component.ngOnChanges({columns: new SimpleChange(mockColumns, [], true)});

            expect(component.displayedColumns).toEqual([]);
            expect(component.toggleableColumns).toEqual([]);
        });
    });
});
