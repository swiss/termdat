import {Location} from '@angular/common';
import {Component, OnDestroy, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {Router, RouterLink, RouterModule} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonModule, ObNotificationService} from '@oblique/oblique';
import {MultiEditAction} from 'api/MultiEditAction';
import {map, merge, of, Subject, switchMap, takeUntil} from 'rxjs';
import {EntryConceptSystemPositionUpdate} from '../../../api/EntryConceptSystemPositionUpdate';
import {EntryService} from '../../entry/entry.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {Option as AutocompleteOption} from '../../shared/autocomplete/Option';
import {WatchlistService} from '../watchlist.service';

@Component({
    selector: 'td-multi-edit-concept-system',
    imports: [
        TranslatePipe,
        MatButtonModule,
        ObButtonModule,
        MatLabel,
        MatFormFieldModule,
        ReactiveFormsModule,
        RouterModule,
        MatInputModule,
        MatIcon,
        RouterLink,
        AutocompleteComponent,
    ],
    templateUrl: './multi-edit-concept-system.component.html',
    styleUrl: './multi-edit-concept-system.component.scss',
})
export class MultiEditConceptSystemComponent implements OnInit, OnDestroy {
    form = new UntypedFormGroup({
        conceptSystemPosition: new UntypedFormControl(),
    });

    idsToModify: number[] = [];
    collectionId?: number;
    conceptSystemPositions: AutocompleteOption[] = [];
    private readonly unsubscribe = new Subject<void>();
    constructor(
        private readonly entryService: EntryService,
        private readonly watchlistService: WatchlistService,
        private readonly notificationService: ObNotificationService,
        private readonly translateService: TranslateService,
        private readonly router: Router,
        private readonly location: Location
    ) {}

    ngOnInit(): void {
        const state = this.location.getState() as {ids: number[]; collectionId: number};
        this.idsToModify = state?.ids ?? [];
        this.collectionId = state?.collectionId;
        if (this.idsToModify.length === 0 || !this.collectionId) {
            void this.router.navigate(['watchlist']);
        }

        merge(of(true), this.translateService.onLangChange)
            .pipe(
                takeUntil(this.unsubscribe),
                switchMap(() =>
                    this.entryService.getConceptSystemPositions(this.collectionId!).pipe(
                        map(response =>
                            response.map(item => ({
                                id: item.id,
                                code: '',
                                name: item.value,
                            }))
                        )
                    )
                )
            )
            .subscribe(conceptSystemPositions => {
                this.conceptSystemPositions = conceptSystemPositions;
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    set(): void {
        this.callUpdate(MultiEditAction.Set);
    }

    remove(): void {
        this.callUpdate(MultiEditAction.Remove);
    }

    private callUpdate(action: MultiEditAction) {
        const value: number = this.form.controls.conceptSystemPosition.value;

        const update: EntryConceptSystemPositionUpdate = {
            ids: this.idsToModify,
            conceptSystemPositionId: value,
            action,
        };
        this.entryService.updateMultipleConceptSystemPosition(update).subscribe(
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
