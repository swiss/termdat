import {Component, Input, OnChanges} from '@angular/core';
import {
    MatCell,
    MatCellDef,
    MatColumnDef,
    MatHeaderCell,
    MatHeaderCellDef,
    MatHeaderRow,
    MatHeaderRowDef,
    MatRow,
    MatRowDef,
    MatTable,
} from '@angular/material/table';
import {TranslatePipe} from '@ngx-translate/core';
import {ObAlertComponent} from '@oblique/oblique';
import {TdMatTableDataSource} from '../../../shared/td-mat-table-data-source';

@Component({
    selector: 'td-file-import-error',
    templateUrl: './file-import-error.component.html',
    styleUrls: ['./file-import-error.component.scss'],
    imports: [
        ObAlertComponent,
        MatTable,
        MatColumnDef,
        MatHeaderCellDef,
        MatHeaderCell,
        MatCellDef,
        MatCell,
        MatHeaderRowDef,
        MatHeaderRow,
        MatRowDef,
        MatRow,
        TranslatePipe,
    ],
})
export class FileImportErrorComponent implements OnChanges {
    @Input() errorList!: string[];

    displayedColumns: string[] = ['errorDetail'];
    dataSource: TdMatTableDataSource<string> = new TdMatTableDataSource();

    ngOnChanges(): void {
        this.dataSource.data = this.errorList;
    }
}
