import {Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewEncapsulation} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatIcon} from '@angular/material/icon';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';

export interface TableColumn {
    key: string;
    label: string;
    toggleable: boolean;
}

@Component({
    selector: 'td-columns-menu',
    templateUrl: './columns-menu.component.html',
    styleUrls: ['./columns-menu.component.scss'],
    imports: [MatButton, ObButtonDirective, MatMenuTrigger, MatIcon, TranslatePipe, MatMenu, MatMenuItem, MatCheckbox, MatTooltip],
    encapsulation: ViewEncapsulation.None,
})
export class ColumnsMenuComponent implements OnChanges {
    @Input({required: true}) columns: readonly TableColumn[] = [];
    @Input({required: true}) menuLabel = '';

    @Output() readonly displayedColumnsChange = new EventEmitter<string[]>();

    private readonly hiddenColumns: Partial<Record<string, boolean>> = {};

    get toggleableColumns(): TableColumn[] {
        return this.columns.filter(column => column.toggleable);
    }

    get allColumnsVisible(): boolean {
        return this.toggleableColumns.every(column => this.isColumnVisible(column.key));
    }

    get displayedColumns(): string[] {
        return this.columns.filter(column => !column.toggleable || this.isColumnVisible(column.key)).map(column => column.key);
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (!changes.columns) {
            return;
        }

        const validColumnKeys = new Set(this.columns.map(column => column.key));

        Object.keys(this.hiddenColumns)
            .filter(key => !validColumnKeys.has(key))
            .forEach(key => delete this.hiddenColumns[key]);

        this.emitDisplayedColumns();
    }

    setAllColumnsVisible(visible: boolean): void {
        this.toggleableColumns.forEach(column => this.setColumnVisible(column.key, visible));
    }

    setColumnVisible(column: string, visible: boolean): void {
        const tableColumn = this.columns.find(({key}) => key === column);

        if (!tableColumn?.toggleable) {
            return;
        }

        this.hiddenColumns[tableColumn.key] = !visible;
        this.emitDisplayedColumns();
    }

    isColumnVisible(column: string): boolean {
        return !this.hiddenColumns[column];
    }

    private emitDisplayedColumns(): void {
        this.displayedColumnsChange.emit(this.displayedColumns);
    }
}
