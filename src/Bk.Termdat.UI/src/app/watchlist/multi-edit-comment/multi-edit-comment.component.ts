import {Location} from '@angular/common';
import {Component, Input, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObNotificationService} from '@oblique/oblique';
import {EntryCommentUpdate} from 'api/EntryCommentUpdate';
import {MultiEditAction} from 'api/MultiEditAction';
import {EntryService} from '../../entry/entry.service';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-multi-edit-comment',
    templateUrl: './multi-edit-comment.component.html',
    styleUrls: ['./multi-edit-comment.component.scss'],
    imports: [ReactiveFormsModule, MatFormField, MatLabel, MatInput, MatButton, ObButtonDirective, TranslatePipe, MatIcon, RouterLink],
})
export class MultiEditCommentComponent implements OnInit {
    @Input() titleTranslateKey!: string;

    form = new UntypedFormGroup({
        comment: new UntypedFormControl(),
    });

    idsToModify: number[] = [];

    constructor(
        private readonly entryService: EntryService,
        private readonly watchlistService: WatchlistService,
        private readonly notificationService: ObNotificationService,
        private readonly translateService: TranslateService,
        private readonly router: Router,
        private readonly location: Location
    ) {}

    ngOnInit(): void {
        const state = this.location.getState() as {ids: number[]};
        this.idsToModify = state?.ids ?? [];
        if (this.idsToModify.length === 0) {
            void this.router.navigate(['watchlist']);
        }
    }

    add(): void {
        this.callUpdate(MultiEditAction.Add);
    }

    set(): void {
        this.callUpdate(MultiEditAction.Set);
    }

    remove(): void {
        this.callUpdate(MultiEditAction.Remove);
    }

    private callUpdate(action: MultiEditAction) {
        const value: string = this.form.controls.comment.value;

        const update: EntryCommentUpdate = {
            ids: this.idsToModify,
            comment: value,
            action,
        };
        this.entryService.updateMultipleComment(update).subscribe(
            entries => {
                if (entries.lockInfos!.length > 0) {
                    this.watchlistService.displayUpdateInfoWarning(entries.lockInfos!, 'entry.notification.lockedEntriesNotUpdated');
                } else {
                    if (entries.dataWasChanged) {
                        this.notificationService.success(this.translateService.instant('entry.notification.editMultipleSuccess'));
                    } else {
                        this.notificationService.warning(this.translateService.instant('entry.notification.noChanges'));
                    }
                }
            },
            () => this.notificationService.error(this.translateService.instant('entry.notification.editMultipleError'))
        );
    }
}
