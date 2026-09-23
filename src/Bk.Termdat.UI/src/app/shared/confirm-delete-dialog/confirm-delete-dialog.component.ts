import {CdkScrollable} from '@angular/cdk/scrolling';
import {Component, Inject} from '@angular/core';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {MatIcon} from '@angular/material/icon';
import {TranslatePipe} from '@ngx-translate/core';
import {ObAlertComponent} from '@oblique/oblique';
import {ConfirmData} from './confirm-data';

@Component({
    selector: 'td-confirm-delete-dialog',
    templateUrl: './confirm-delete-dialog.component.html',
    styleUrls: ['./confirm-delete-dialog.component.scss'],
    imports: [
        MatDialogTitle,
        MatIconButton,
        MatDialogClose,
        MatIcon,
        CdkScrollable,
        MatDialogContent,
        ObAlertComponent,
        MatDialogActions,
        MatButton,
        TranslatePipe,
    ],
})
export class ConfirmDeleteDialogComponent {
    constructor(
        private readonly dialogRef: MatDialogRef<ConfirmDeleteDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ConfirmData
    ) {}

    confirmDialog(): void {
        this.dialogRef.close(true);
    }
}
