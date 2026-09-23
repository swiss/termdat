import {CdkScrollable} from '@angular/cdk/scrolling';
import {Component, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {MatFormField} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatSelect} from '@angular/material/select';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {EntryTemplateSummary} from 'api/EntryTemplateSummary';
import {merge, Subject, switchMap, takeUntil} from 'rxjs';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';

@Component({
    selector: 'td-entry-template-selection-dialog',
    templateUrl: './entry-template-selection-dialog.component.html',
    styleUrls: ['./entry-template-selection-dialog.component.scss'],
    imports: [
        MatDialogTitle,
        MatIconButton,
        MatDialogClose,
        MatIcon,
        CdkScrollable,
        MatDialogContent,
        ReactiveFormsModule,
        MatFormField,
        MatSelect,
        MatOption,
        MatDialogActions,
        MatButton,
        TranslatePipe,
    ],
})
export class EntryTemplateSelectionDialogComponent implements OnInit {
    templateSelectionForm!: UntypedFormGroup;
    entryTemplates!: EntryTemplateSummary[];

    private readonly unsubscribe = new Subject<void>();
    private readonly reload = new Subject<void>();

    constructor(
        private readonly translateService: TranslateService,
        private readonly entryTemplateService: EntryTemplateService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly dialogRef: MatDialogRef<EntryTemplateSelectionDialogComponent>
    ) {}

    ngOnInit(): void {
        this.templateSelectionForm = this.formBuilder.group({
            entryTemplateId: null,
        });

        merge(this.reload, this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.entryTemplateService.getEntryTemplates()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(entryTemplates => {
                this.entryTemplates = entryTemplates;
            });

        this.reload.next();
    }

    selectTemplate(): void {
        const templateId = this.templateSelectionForm.controls.entryTemplateId?.value as number;

        if (templateId !== null) {
            this.dialogRef.close(templateId);
        }
    }
}
