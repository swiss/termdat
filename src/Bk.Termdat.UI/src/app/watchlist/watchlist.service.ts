import {HttpClient} from '@angular/common/http';
import {Inject, Injectable, OnDestroy} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {EntryMultipleUpdateInfo} from 'api/EntryMultipleUpdateInfo';
import {EntrySummary} from 'api/EntrySummary';
import {WatchlistEntry} from 'api/WatchlistEntry';
import {BehaviorSubject, Observable} from 'rxjs';
import {distinct, map, tap} from 'rxjs/operators';
import {downloadFile, downloadTxtFile} from '../shared/download.helper';
import {LOCAL_STORAGE} from '../shared/injection.tokens';

@Injectable({
    providedIn: 'root',
})
export class WatchlistService implements OnDestroy {
    watchlist$: Observable<number[]>;
    private readonly watchlistSubject: BehaviorSubject<number[]>;

    private readonly watchlistLocalStorageName = 'Watchlist';
    private readonly selectedWatchlistItemsLocalStorageName = 'SelectedWatchlistItems';
    private readonly separator = ',';

    constructor(
        @Inject(LOCAL_STORAGE) private readonly localStorage: Storage,
        private readonly httpClient: HttpClient,
        private readonly notificationService: ObNotificationService,
        private readonly translateService: TranslateService
    ) {
        this.watchlistSubject = new BehaviorSubject<number[]>(this.readWatchlist());
        this.watchlist$ = this.watchlistSubject.asObservable().pipe(distinct());
    }

    ngOnDestroy(): void {
        this.watchlistSubject.complete();
    }

    isWatchlistItem(entryId: number): Observable<boolean> {
        return this.watchlist$.pipe(map(watchlist => watchlist.includes(entryId)));
    }

    add(...entryIds: number[]) {
        this.updateWatchlist([...this.readWatchlist(), ...entryIds]);
    }

    remove(...entryIds: number[]) {
        this.updateWatchlist(this.readWatchlist().filter(id => !entryIds.includes(id)));
        this.localStorage.removeItem(this.selectedWatchlistItemsLocalStorageName);
    }

    removeAll() {
        this.updateWatchlist([]);
        this.localStorage.removeItem(this.selectedWatchlistItemsLocalStorageName);
    }

    getEntries(entryIds: number[]): Observable<WatchlistEntry[]> {
        return this.httpClient.post<WatchlistEntry[]>('/api/watchlist', {entryIds});
    }

    createExportPdf(entryIds: EntrySummary['id'][]): Observable<void> {
        return this.httpClient.post('/api/watchlist/export:pdf', {entryIds}, {observe: 'response', responseType: 'blob'}).pipe(
            tap(response => downloadFile(response, 'Export.pdf')),
            map(() => void 0)
        );
    }

    createExportXlsx(entryIds: EntrySummary['id'][]): Observable<void> {
        return this.httpClient.post('/api/watchlist/export:xlsx', {entryIds}, {observe: 'response', responseType: 'blob'}).pipe(
            tap(response => downloadFile(response, 'Export.xlsx')),
            map(() => void 0)
        );
    }

    createExportTxt(entryIds: number[]): void {
        const data = entryIds.join(',');

        const datePrefix = new Date().toISOString().split('T')[0].replace(/-/g, '');

        downloadTxtFile(data, this.translateService.instant('watchList.exportEntryIdsTxtFilename', {date: datePrefix}));
    }

    displayUpdateInfoWarning(infos: EntryMultipleUpdateInfo[], lockedMessage: string, notSoftDeletedMessage?: string): void {
        const lockedEntryInfos = infos.filter(item => !!item.lockedEntryId);
        const notSoftDeletedInfos = infos.filter(item => !!item.notSoftDeletedEntryId);

        if (lockedEntryInfos.length) {
            const lockedMessageText = this.translateService.instant(lockedMessage);
            let lockedEntriesText = '';
            infos.forEach(entry => (lockedEntriesText = lockedEntriesText += `\n${entry.lockedEntryId}: ${entry.lockedBy};`));
            this.notificationService.warning({
                message: `${lockedMessageText}\n${lockedEntriesText}`,
                timeout: 15000,
            });
        }

        if (notSoftDeletedInfos.length && notSoftDeletedMessage) {
            const notSoftDeletedMessageText = this.translateService.instant(notSoftDeletedMessage);
            let notSoftDeletedText = '';
            infos.forEach(entry => (notSoftDeletedText = notSoftDeletedText += `\n${entry.notSoftDeletedEntryId};`));
            this.notificationService.warning({
                message: `${notSoftDeletedMessageText}\n${notSoftDeletedText}`,
                timeout: 15000,
            });
        }
    }

    public updateSelectedItems(entryIds: number[]): void {
        this.localStorage.setItem(this.selectedWatchlistItemsLocalStorageName, entryIds.join(this.separator));
    }

    public readSelectedItems(): number[] {
        return (
            this.localStorage
                .getItem(this.selectedWatchlistItemsLocalStorageName)
                ?.split(this.separator)
                ?.map(s => parseInt(s, 10))
                ?.filter(n => !isNaN(n)) ?? []
        );
    }

    private updateWatchlist(entryIds: number[]) {
        this.localStorage.setItem(this.watchlistLocalStorageName, entryIds.join(this.separator));
        this.watchlistSubject.next(entryIds);
    }

    private readWatchlist(): number[] {
        return (
            this.localStorage
                .getItem(this.watchlistLocalStorageName)
                ?.split(this.separator)
                ?.map(s => parseInt(s, 10))
                ?.filter(n => !isNaN(n)) ?? []
        );
    }
}
