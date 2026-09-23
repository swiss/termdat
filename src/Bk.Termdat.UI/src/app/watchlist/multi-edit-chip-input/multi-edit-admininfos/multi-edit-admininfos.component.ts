import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {EntryChipFieldInputUpdate} from 'api/EntryChipFieldInputUpdate';
import {MultiEditAction} from 'api/MultiEditAction';
import {merge, of, Subject} from 'rxjs';
import {catchError, switchMap, takeUntil} from 'rxjs/operators';
import {EntryService} from '../../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {Chip} from '../../../shared/chiplist-autocomplete/Chip';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipInputComponent} from '../multi-edit-chip-input.component';

@Component({
    selector: 'td-multi-edit-admininfos',
    templateUrl: './multi-edit-admininfos.component.html',
    styleUrls: ['./multi-edit-admininfos.component.scss'],
    imports: [MultiEditChipInputComponent],
})
export class MultiEditAdminInfosComponent implements OnInit, OnDestroy {
    @Input() ids!: number[];

    adminInfoChips: Chip[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly extendedSearchFilterService: ExtendedSearchFiltersService,
        private readonly translateService: TranslateService,
        private readonly watchlistService: WatchlistService,
        private readonly entryService: EntryService,
        private readonly notificationService: ObNotificationService
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.extendedSearchFilterService.getAdminInfos().pipe(catchError(() => of([])))),
                takeUntil(this.unsubscribe)
            )
            .subscribe(result => {
                this.adminInfoChips = result.map(adminInfo => new Chip(adminInfo.code, adminInfo.code, adminInfo.id));
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    add(codes: AdminInfoSummary[]): void {
        this.callUpdate(codes, MultiEditAction.Add);
    }

    set(codes: AdminInfoSummary[]): void {
        this.callUpdate(codes, MultiEditAction.Set);
    }

    remove(): void {
        this.callUpdate([], MultiEditAction.Remove);
    }

    private callUpdate(codes: AdminInfoSummary[], action: MultiEditAction) {
        const update: EntryChipFieldInputUpdate = {
            ids: this.ids,
            codes,
            action,
        };
        this.entryService.updateMultipleAdminInfos(update).subscribe(
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
