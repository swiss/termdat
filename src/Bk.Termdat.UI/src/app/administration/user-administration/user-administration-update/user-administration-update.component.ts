import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {KeyValuePipe} from '@angular/common';
import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {UserAccountModification} from 'api/UserAccountModification';
import {forkJoin, merge, of, Subject} from 'rxjs';
import {catchError, mergeMap, switchMap, takeUntil, tap} from 'rxjs/operators';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteComponent} from '../../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../../shared/error.service';
import {UserAccountService} from '../user-account.service';

@Component({
    selector: 'td-user-administration-update',
    templateUrl: './user-administration-update.component.html',
    styleUrls: ['./user-administration-update.component.scss'],
    imports: [
        MatButton,
        ObButtonDirective,
        MatIcon,
        RouterLink,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        ChiplistAutocompleteComponent,
        MatError,
        KeyValuePipe,
        TranslatePipe,
        ObUnsavedChangesDirective,
        CdkTextareaAutosize,
    ],
})
export class UserAdministrationUpdateComponent implements OnInit, OnDestroy {
    public userAccountDetail!: UserAccountDetail;
    form: UntypedFormGroup;

    @Input() titleTranslateKey!: string;
    glossaryChips: Chip[] = [];
    collectionChips: Chip[] = [];
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly userAccountService: UserAccountService,
        private readonly errorService: ErrorService,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents,
        private readonly translateService: TranslateService,
        private readonly router: Router,
        private readonly route: ActivatedRoute,
        private readonly notificationService: ObNotificationService
    ) {
        this.form = this.formBuilder.group({
            id: {value: '', disabled: true},
            firstName: {value: '', disabled: true},
            lastName: {value: '', disabled: true},
            email: {value: '', disabled: true},
            role: {value: '', disabled: true},
            lastAction: {value: '', disabled: true},
            collections: {value: [], disabled: false},
            glossaries: {value: [], disabled: false},
            information: this.formBuilder.control(null, [Validators.maxLength(1000)]),
        });
    }

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() =>
                    forkJoin({
                        collections: this.extendedSearchFilterService.getCollections().pipe(catchError(() => of([]))),
                        glossaries: this.extendedSearchFilterService.getGlossaries().pipe(catchError(() => of([]))),
                    })
                ),
                tap(({collections, glossaries}) => {
                    this.collectionChips = collections.map(Chip.fromOption);
                    this.glossaryChips = glossaries.map(Chip.fromOption);
                }),
                mergeMap(() =>
                    this.route.paramMap.pipe(
                        switchMap(params => {
                            const idParam = params.get('id');
                            if (!idParam) {
                                this.back();
                            }
                            const userAccountId = +idParam!;
                            this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
                            return this.userAccountService.get(userAccountId).pipe(
                                tap({
                                    next: data => {
                                        this.userAccountDetail = data;

                                        this.form.reset(data);
                                        if (!this.userAccountDetail.isWriter) {
                                            this.form.controls.glossaries.disable();
                                        } else {
                                            this.form.controls.glossaries.enable();
                                        }
                                        this.form.controls.collections.patchValue(data.collections.map(y => y.id));
                                        this.form.controls.glossaries.patchValue(data.glossaries.map(y => y.id));
                                    },
                                    error: error => {
                                        this.errorService.handleHttpError(error, 'administration.userAdministration.notification.saveError');
                                        this.back();
                                    },
                                })
                            );
                        })
                    )
                ),
                takeUntil(this.unsubscribe)
            )
            .subscribe();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    save(): void {
        this.form.markAllAsTouched();

        if (this.form.invalid) {
            return;
        }
        this.httpInterceptorEvents.deactivateNotificationOnNextAPICalls(1);
        this.userAccountService.update(this.getFormEntry()).subscribe(
            () => {
                this.notificationService.success('entry.notification.editMultipleSuccess');
                this.back();
            },
            error => this.errorService.handleHttpError(error, 'entry.notification.editMultipleError')
        );
    }

    getFormEntry(): UserAccountModification {
        const ctrls = this.form.controls;
        return {
            id: ctrls.id.value,
            information: ctrls.information.value,
            authorizedCollectionIds: ctrls.collections.value,
            authorizedGlossaryIds: ctrls.glossaries.value,
        };
    }

    private back(): void {
        void this.router.navigate(['administration', 'userAdministration']);
    }
}
