import {CdkScrollable} from '@angular/cdk/scrolling';
import {Component, ViewEncapsulation} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {TranslatePipe} from '@ngx-translate/core';
import {UnsavedChangeAction} from './unsaved-change-action';

@Component({
    selector: 'td-unsaved-changes',
    templateUrl: './unsaved-changes.component.html',
    styleUrls: ['./unsaved-changes.component.scss'],
    encapsulation: ViewEncapsulation.None,
    imports: [MatDialogTitle, CdkScrollable, MatDialogContent, MatDialogActions, MatButton, MatDialogClose, TranslatePipe],
})
export class UnsavedChangesComponent {
    constructor(private readonly dialogRef: MatDialogRef<UnsavedChangesComponent, UnsavedChangeAction>) {}

    save(): void {
        this.dialogRef.close(UnsavedChangeAction.SAVE);
    }

    revert(): void {
        this.dialogRef.close(UnsavedChangeAction.DISCARD);
    }
}
