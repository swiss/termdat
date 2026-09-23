import {Location} from '@angular/common';
import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {ActivatedRoute, ParamMap, Router} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObAlertComponent, ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {of, Subject, throwError} from 'rxjs';
import {Observable} from 'rxjs/internal/Observable';
import {catchError, distinctUntilChanged, filter, switchMap, takeUntil, tap} from 'rxjs/operators';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {ErrorService} from '../../shared/error.service';
import {FocusRemoverDirective} from '../../shared/focus-remover.directive';
import {CanComponentDeactivate} from '../../shared/unsaved-changes/CanComponentDeactivate';
import {UnsavedChangesService} from '../../shared/unsaved-changes/unsaved-changes.service';
import {EntryFormComponent} from '../entry-form/entry-form.component';
import {EntryService} from '../entry.service';

@Component({
    selector: 'td-entry-template',
    templateUrl: './entry-template.component.html',
    styleUrls: ['./entry-template.component.scss'],
    imports: [MatButton, ObButtonDirective, FocusRemoverDirective, ObAlertComponent, EntryFormComponent, TranslatePipe],
})
export class EntryTemplateComponent implements OnInit, OnDestroy, CanComponentDeactivate {
    form: UntypedFormGroup;
    entry?: EntryModificationDetail;
    templateId = 0;

    @ViewChild(EntryFormComponent) entryFormComponent!: EntryFormComponent;

    private static readonly ID = 'entry-template';
    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly entryService: EntryService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly notificationService: ObNotificationService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly errorService: ErrorService,
        private readonly unsavedChangesService: UnsavedChangesService,
        private readonly router: Router,
        private readonly entryTemplateService: EntryTemplateService,
        private readonly location: Location
    ) {
        this.form = this.formBuilder.group({});
        this.unsavedChangesService.watch(EntryTemplateComponent.ID, this, this.form);
    }

    ngOnInit(): void {
        this.activatedRoute.paramMap
            .pipe(
                switchMap((params: ParamMap) => {
                    const value = params.get('id') ?? '0';
                    const id = parseInt(value, 10);
                    this.templateId = isNaN(id) ? 0 : id;
                    return of(this.templateId);
                }),
                distinctUntilChanged(),
                filter((id: number) => id > 0),
                switchMap((id: number) => this.entryTemplateService.getTemplateById(id)),
                takeUntil(this.unsubscribe)
            )
            .subscribe(entry => {
                this.setEntry(entry);
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();

        this.unsavedChangesService.unwatch(EntryTemplateComponent.ID);
    }

    save(): void {
        this.saveInternal().subscribe();
    }

    canUnload(): boolean {
        return !this.form.dirty;
    }

    back(): void {
        this.location.back();
    }

    canDeactivate(): Observable<boolean> {
        return this.form.dirty ? this.unsavedChangesService.createDeactivateConfirmDialog(() => this.saveInternal()) : of(true);
    }

    private saveInternal(): Observable<boolean> {
        return this.saveEntry().pipe(
            switchMap(savedEntry => {
                this.form.markAsPristine();
                void this.router.navigate(['/search/entry', savedEntry.id, 'update'], {
                    queryParams: this.router.currentNavigation()?.extras.queryParams ?? {},
                });

                return of(true);
            }),
            catchError(() => of(false))
        );
    }

    private setEntry(entry: EntryModificationDetail) {
        this.entry = {
            ...entry,
            id: undefined,
            concurrencyToken: undefined,
            lenochClassifications: [],
            languageDetails:
                entry.languageDetails?.map((details: EntryLanguageDetail) => {
                    details.id = undefined;
                    return details;
                }) ?? [],
            references:
                entry.references?.map((refs: EntryReferenceDetail) => {
                    refs.id = undefined;
                    return refs;
                }) ?? [],
        };
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
            }),
            catchError(error => {
                this.errorService.handleHttpError(error, 'entry.notification.createError');

                return throwError(() => error);
            })
        );
    }
}
