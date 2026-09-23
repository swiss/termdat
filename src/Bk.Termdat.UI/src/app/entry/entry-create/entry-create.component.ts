import {Location} from '@angular/common';
import {Component, OnDestroy, ViewChild} from '@angular/core';
import {UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {Router} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {OfficeSummary} from 'api/OfficeSummary';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {of, throwError} from 'rxjs';
import {Observable} from 'rxjs/internal/Observable';
import {catchError, filter, switchMap, tap} from 'rxjs/operators';
import {ConfigsService} from '../../config.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {FocusRemoverDirective} from '../../shared/focus-remover.directive';
import {CanComponentDeactivate} from '../../shared/unsaved-changes/CanComponentDeactivate';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {EntryTemplateCreateDialogComponent} from '../entry-template-create-dialog/entry-template-create-dialog.component';
import {EntryTemplateSelectionDialogComponent} from '../entry-template-selection-dialog/entry-template-selection-dialog.component';
import {EntryService} from '../entry.service';

@Component({
    selector: 'td-create',
    templateUrl: './entry-create.component.html',
    styleUrls: ['./entry-create.component.scss'],
    imports: [MatButton, ObButtonDirective, FocusRemoverDirective, EntryFormComponent, TranslatePipe],
})
export class EntryCreateComponent implements OnDestroy, CanComponentDeactivate {
    form: UntypedFormGroup;
    entryDefault: EntryModificationDetail | undefined;

    @ViewChild(EntryFormComponent) entryFormComponent!: EntryFormComponent;

    private static readonly ID = 'entry-create';

    constructor(
        private readonly entryService: EntryService,
        private readonly entryTemplateService: EntryTemplateService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly unsavedChangesService: UnsavedChangesService,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly dialog: MatDialog,
        private readonly router: Router,
        private readonly location: Location,
        private readonly configsService: ConfigsService
    ) {
        this.form = this.formBuilder.group({});
        this.unsavedChangesService.watch(EntryCreateComponent.ID, this, this.form);
        this.setDefaultEntry();
    }

    ngOnDestroy(): void {
        this.unsavedChangesService.unwatch(EntryCreateComponent.ID);
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

    applyEntryTemplate(): void {
        this.dialog
            .open(EntryTemplateSelectionDialogComponent)
            .afterClosed()
            .pipe(
                filter(templateId => templateId),
                switchMap(templateId =>
                    this.router.navigate(['/search/entry', templateId, 'template'], {queryParams: this.router.currentNavigation()?.extras.queryParams})
                )
            )
            .subscribe();
    }

    back(): void {
        this.location.back();
    }

    canDeactivate(): Observable<boolean> {
        return this.form.dirty ? this.unsavedChangesService.createDeactivateConfirmDialog(() => this.saveInternal()) : of(true);
    }

    canUnload(): boolean {
        return !this.form.dirty;
    }

    private saveInternal(): Observable<boolean> {
        return this.saveEntry().pipe(
            switchMap(savedEntry => {
                this.form.markAsPristine();
                void this.router.navigate(['/search/entry', savedEntry.id, 'update'], {
                    queryParams: this.router.currentNavigation()?.extras.queryParams,
                });

                return of(true);
            }),
            catchError(() => of(false))
        );
    }

    private setDefaultEntry() {
        this.entryDefault = {
            status: Status.InProgress,
            reliability: Reliability.Tentative,
            lenochClassifications: [],
            glossaries: [],
            collection: {id: this.configsService.frontendConfig.defaultCollectionId} as CollectionSummary,
            office: {id: this.configsService.frontendConfig.defaultOfficeId} as OfficeSummary,
            relatedCollections: [],
        };
        this.form.markAsPristine();
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
        return this.entryService.create(entry).pipe(
            tap(() => {
                this.notificationService.success({
                    message: 'entry.notification.createSuccess',
                });
                this.extendedSearchFilterService.reloadExtendedSearchData$.next();
            }),
            catchError(error => {
                this.errorService.handleHttpError(error, 'entry.notification.createError');

                return throwError(() => error);
            })
        );
    }
}
