import {HttpErrorResponse} from '@angular/common/http';
import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {FormBuilder, FormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, ParamMap, Router} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {of, Subject, throwError} from 'rxjs';
import {Observable} from 'rxjs/internal/Observable';
import {catchError, distinctUntilChanged, filter, map, switchMap, takeUntil, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {FocusRemoverDirective} from '../../shared/focus-remover.directive';
import {CanComponentDeactivate} from '../../shared/unsaved-changes/CanComponentDeactivate';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {CanLockedComponentDeactivate} from '../entry-locked/CanLockedComponentDeactivate';
import {EntryLockedService} from '../entry-locked/entry-locked.service';
import {EntryTemplateCreateDialogComponent} from '../entry-template-create-dialog/entry-template-create-dialog.component';
import {EntryService} from '../entry.service';

@Component({
    selector: 'td-entry-update',
    templateUrl: './entry-update.component.html',
    styleUrls: ['./entry-update.component.scss'],
    imports: [MatButton, ObButtonDirective, FocusRemoverDirective, EntryFormComponent, TranslatePipe],
})
export class EntryUpdateComponent implements OnInit, OnDestroy, CanComponentDeactivate, CanLockedComponentDeactivate {
    form: FormGroup;
    entry?: EntryModificationDetail;

    @ViewChild(EntryFormComponent) entryFormComponent!: EntryFormComponent;

    private static readonly ID = 'entry-update';
    private readonly unsubscribe = new Subject<void>();

    private get locked() {
        return !!this.entry?.locked;
    }

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly entryService: EntryService,
        private readonly entryTemplateService: EntryTemplateService,
        private readonly formBuilder: FormBuilder,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly unsavedChangesService: UnsavedChangesService,
        private readonly entryLockedService: EntryLockedService,
        private readonly dialog: MatDialog,
        private readonly router: Router,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService
    ) {
        this.form = this.formBuilder.group({});
        this.unsavedChangesService.watch(EntryUpdateComponent.ID, this, this.form);
        this.entryLockedService.watch(EntryUpdateComponent.ID, this);
    }

    ngOnInit(): void {
        this.activatedRoute.paramMap
            .pipe(
                switchMap((params: ParamMap) => {
                    const value = params.get('id') ?? '0';
                    const id = parseInt(value, 10);
                    return of(isNaN(id) ? 0 : id);
                }),
                distinctUntilChanged(),
                filter((id: number) => id > 0),
                switchMap((id: number) => {
                    return this.entryService
                        .getModification(id)
                        .pipe(catchError((error: HttpErrorResponse) => this.errorService.handleHttpError(error, 'entry.notification.editError')));
                }),
                switchMap((entry: EntryModificationDetail) => {
                    return this.entryService.lock(entry.id!).pipe(
                        map(result => {
                            entry.locked = new Date();
                            entry.concurrencyToken = result.concurrencyToken;

                            return entry;
                        })
                    );
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe(entry => {
                this.entry = entry;
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();

        this.unsavedChangesService.unwatch(EntryUpdateComponent.ID);
        this.entryLockedService.unwatch(EntryUpdateComponent.ID);
    }

    save(): void {
        this.saveInternal().subscribe();
    }

    createEntryTemplate(): void {
        const applyEntryTemplate = this.entryFormComponent.getFormEntry();

        if (applyEntryTemplate.subjectIds?.length === 1 && applyEntryTemplate.subjectIds[0] === null) {
            applyEntryTemplate.subjectIds = [];
        }

        this.dialog
            .open(EntryTemplateCreateDialogComponent)
            .afterClosed()
            .pipe(
                filter(result => result),
                switchMap(result => this.entryTemplateService.createEntryTemplate(result, applyEntryTemplate))
            )
            .subscribe({
                next: () => {
                    this.notificationService.success('entry.template.create.success');
                },
                error: error => this.errorService.handleHttpError(error, 'entry.template.create.error'),
            });
    }

    close() {
        void this.router.navigate(['/search/entry', this.entry!.id], {queryParams: this.activatedRoute.snapshot?.queryParams});
    }

    canUnload(): boolean {
        return !this.form.dirty;
    }

    canDeactivate(): Observable<boolean> {
        return this.form.dirty ? this.unsavedChangesService.createDeactivateConfirmDialog(() => this.saveInternal()) : of(true);
    }

    canLockedDeactivate(): Observable<boolean> {
        return this.locked ? this.unlock() : of(true);
    }

    canLockedUnload(): boolean {
        return !this.locked;
    }

    private unlock(): Observable<boolean> {
        return this.entryService.unlock(this.entry!.id!).pipe(
            map(() => true),
            catchError(() => of(false))
        );
    }

    private saveInternal(): Observable<boolean> {
        return this.saveEntry().pipe(
            switchMap(updatedEntry => {
                this.entry = updatedEntry;
                this.form.markAsPristine();

                return of(true);
            }),
            catchError(() => {
                return of(false);
            })
        );
    }

    private saveEntry(): Observable<EntryModificationDetail> {
        this.form.markAllAsTouched();

        if (!this.form.valid) {
            const validationNotifications = this.entryFormComponent.getValidationNotifications();
            if (validationNotifications.length) {
                this.notificationService.warning({message: validationNotifications.pop()});
            }

            return throwError(() => this.form.errors);
        }

        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);

        const entry = this.entryFormComponent.getFormEntry();

        return this.entryService.update(entry).pipe(
            tap(() => {
                this.notificationService.success('entry.notification.editSuccess');
                this.extendedSearchFilterService.reloadExtendedSearchData$.next();
            }),
            catchError(error => {
                this.errorService.handleHttpError(error, 'entry.notification.editError');
                return throwError(() => error);
            })
        );
    }
}
