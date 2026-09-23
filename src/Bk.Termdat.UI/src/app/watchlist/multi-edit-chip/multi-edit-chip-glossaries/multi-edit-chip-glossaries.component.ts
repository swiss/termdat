import {Component, Input, OnInit} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {EntryChipFieldUpdate} from 'api/EntryChipFieldUpdate';
import {MultiEditAction} from 'api/MultiEditAction';
import {merge, of, Subject} from 'rxjs';
import {catchError, switchMap, takeUntil} from 'rxjs/operators';
import {GlossaryService} from '../../../administration/glossary/glossary.service';
import {EntryService} from '../../../entry/entry.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipComponent} from '../multi-edit-chip.component';

@Component({
    selector: 'td-multi-edit-chip-glossaries',
    templateUrl: './multi-edit-chip-glossaries.component.html',
    styleUrls: ['./multi-edit-chip-glossaries.component.scss'],
    imports: [MultiEditChipComponent],
})
export class MultiEditChipGlossariesComponent implements OnInit {
    @Input() ids!: number[];

    glossaryChips: Chip[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly glossaryService: GlossaryService,
        private readonly translateService: TranslateService,
        private readonly watchlistService: WatchlistService,
        private readonly entryService: EntryService,
        private readonly notificationService: ObNotificationService
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.glossaryService.getModificationGlossaries().pipe(catchError(() => of([])))),
                takeUntil(this.unsubscribe)
            )
            .subscribe(result => {
                this.glossaryChips = result.map(Chip.fromOption);
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
        this.entryService.updateMultipleGlossaries(update).subscribe(
            updatedEntries => {
                if (updatedEntries.lockInfos!.length > 0) {
                    this.watchlistService.displayUpdateInfoWarning(updatedEntries.lockInfos!, 'entry.notification.lockedEntriesNotUpdated');
                } else {
                    if (updatedEntries.dataWasChanged) {
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
