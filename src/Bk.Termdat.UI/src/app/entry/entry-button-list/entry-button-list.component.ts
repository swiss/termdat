import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObNotificationService} from '@oblique/oblique';
import {EntryDetail} from 'api/EntryDetail';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {combineLatest, Subject} from 'rxjs';
import {filter, switchMap} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {ConfirmDeleteDialogComponent} from '../../shared/confirm-delete-dialog/confirm-delete-dialog.component';
import {FocusRemoverDirective} from '../../shared/focus-remover.directive';
import {EntryService} from '../entry.service';

@Component({
    selector: 'td-entry-button-list',
    templateUrl: './entry-button-list.component.html',
    styleUrls: ['./entry-button-list.component.scss'],
    imports: [MatButton, ObButtonDirective, FocusRemoverDirective, RouterLink, TranslatePipe],
})
export class EntryButtonListComponent implements OnInit, OnDestroy {
    canCreateAndDeleteAndUpdate = false;
    isAdmin = false;
    canCreate = false;
    @Input() currentEntry: EntryDetail | undefined;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly authService: AuthService,
        private readonly entryService: EntryService,
        private readonly dialog: MatDialog,
        private readonly notificationService: ObNotificationService,
        private readonly translateService: TranslateService
    ) {}

    ngOnInit(): void {
        this.authService.roles$.subscribe(roles => {
            this.canCreateAndDeleteAndUpdate = roles.length > 0;
        });

        combineLatest([this.authService.isAdmin$, this.authService.isSuperUser$, this.authService.isWriter$, this.authService.userAccountDetail$]).subscribe(
            ([isAdmin, isSuperUser, isWriter, userAccount]) => {
                this.isAdmin = isAdmin;
                this.setCanCreate(userAccount, isWriter, isSuperUser);
            }
        );
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    softDeleteEntry(id: number) {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'entry.delete.title',
                    text: 'entry.delete.text',
                    textValue: {id},
                    confirm: 'entry.delete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.entryService.delete(id))
            )
            .subscribe(
                infos => {
                    if (infos.length) {
                        if (infos[0].lockedEntryId) {
                            this.notificationService.warning({
                                message: this.translateService.instant('entry.error.delete.failedLocked', {email: infos[0].lockedBy}),
                                timeout: 15000,
                            });
                        }
                    } else {
                        this.notificationService.success({
                            title: 'search.result.deleted.successTitle',
                            message: 'search.result.deleted.successMessage',
                        });

                        if (this.isAdmin && this.currentEntry) {
                            this.currentEntry.deleted = true;
                        }
                    }
                },
                () => this.notificationService.error(this.translateService.instant('entry.error.delete.failed'))
            );
    }

    hardDeleteEntry(id: number) {
        this.dialog
            .open(ConfirmDeleteDialogComponent, {
                data: {
                    title: 'entry.hardDelete.title',
                    text: 'entry.hardDelete.text',
                    textValue: {id},
                    confirm: 'entry.hardDelete.confirm',
                },
            })
            .afterClosed()
            .pipe(
                filter(data => !!data),
                switchMap(() => this.entryService.delete(id, false))
            )
            .subscribe(
                infos => {
                    if (infos.length) {
                        if (infos[0].lockedEntryId) {
                            this.notificationService.warning({
                                message: this.translateService.instant('entry.error.delete.failedLocked', {email: infos[0].lockedBy}),
                                timeout: 15000,
                            });
                        } else if (infos[0].notSoftDeletedEntryId) {
                            this.notificationService.warning({
                                message: this.translateService.instant('entry.error.delete.failedNotSoftDeleted'),
                                timeout: 15000,
                            });
                        }
                    } else {
                        this.notificationService.success({
                            title: 'search.result.deleted.successTitle',
                            message: 'entry.hardDelete.successMessage',
                        });
                    }
                },
                () => this.notificationService.error(this.translateService.instant('entry.error.delete.failed'))
            );
    }

    restoreEntry(id: number): void {
        this.entryService.restore(id).subscribe(restoredEntry => {
            this.notificationService.success({
                title: 'entry.restore.successTitle',
                message: 'entry.restore.successMessage',
            });

            if (this.currentEntry) {
                this.currentEntry.deleted = restoredEntry.deleted;
                this.currentEntry.modified = restoredEntry.modified;
            }
        });
    }

    unlock(id: number) {
        this.entryService.unlock(id).subscribe(() => {
            this.notificationService.success({
                title: 'entry.unlock.successTitle',
                message: 'entry.unlock.successMessage',
            });

            this.currentEntry!.locked = undefined;
        });
    }

    setCanCreate(userAccount: UserAccountDetail, isWriter: boolean, isSuperUser: boolean): void {
        this.canCreate = (userAccount.collections.length > 0 && isWriter) || isSuperUser || this.isAdmin;
    }
}
