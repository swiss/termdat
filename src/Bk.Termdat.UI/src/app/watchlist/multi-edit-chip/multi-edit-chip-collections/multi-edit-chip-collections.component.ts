import {Component, Input, OnInit} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {EntryChipFieldUpdate} from 'api/EntryChipFieldUpdate';
import {MultiEditAction} from 'api/MultiEditAction';
import {merge, of, Subject} from 'rxjs';
import {catchError, switchMap, takeUntil} from 'rxjs/operators';
import {CollectionService} from '../../../administration/collection/collection.service';
import {EntryService} from '../../../entry/entry.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {toCodeNameLabel} from '../../../shared/LabelFormat';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipComponent} from '../multi-edit-chip.component';

@Component({
    selector: 'td-multi-edit-chip-collections',
    templateUrl: './multi-edit-chip-collections.component.html',
    styleUrls: ['./multi-edit-chip-collections.component.scss'],
    imports: [MultiEditChipComponent],
})
export class MultiEditChipCollectionsComponent implements OnInit {
    @Input() ids!: number[];

    collectionChips: Chip[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly collectionService: CollectionService,
        private readonly translateService: TranslateService,
        private readonly watchlistService: WatchlistService,
        private readonly entryService: EntryService,
        private readonly notificationService: ObNotificationService
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.collectionService.getModificationCollections().pipe(catchError(() => of([])))),
                takeUntil(this.unsubscribe)
            )
            .subscribe(result => {
                this.collectionChips = result.map(collection => new Chip(collection.code, toCodeNameLabel(collection), collection.id));
            });
    }

    add(codes: string[]) {
        this.callUpdate(
            codes.map(y => String(y)),
            MultiEditAction.Add
        );
    }

    set(codes: string[]) {
        this.callUpdate(
            codes.map(y => String(y)),
            MultiEditAction.Set
        );
    }

    remove() {
        this.callUpdate([], MultiEditAction.Remove);
    }

    private callUpdate(codes: string[], action: MultiEditAction): void {
        const update: EntryChipFieldUpdate = {
            ids: this.ids,
            codes,
            action,
        };
        this.entryService.updateMultipleRelatedCollections(update).subscribe(
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
