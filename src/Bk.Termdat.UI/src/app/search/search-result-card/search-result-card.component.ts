import {AsyncPipe, NgClass} from '@angular/common';
import {Component, Input, OnDestroy} from '@angular/core';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatTooltip} from '@angular/material/tooltip';
import {IsActiveMatchOptions, RouterLinkActive} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {EntryLanguageSummary} from 'api/EntryLanguageSummary';
import {EntrySummary} from 'api/EntrySummary';
import {Status} from 'api/Status';
import {Observable, of, Subject} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {WatchlistService} from '../../watchlist/watchlist.service';

@Component({
    selector: 'td-search-result-card',
    templateUrl: './search-result-card.component.html',
    styleUrls: ['./search-result-card.component.scss'],
    imports: [RouterLinkActive, NgClass, MatCheckbox, MatTooltip, AsyncPipe, TranslatePipe],
})
export class SearchResultCardComponent implements OnDestroy {
    readonly separator = ' — ';

    isValidated = false;
    isMainOffice = false;
    canBeAddedToWatchList = false;
    isWatchlistItem = false;
    isAuthenticated$: Observable<boolean>;
    readonly routerLinkActiveOptions: IsActiveMatchOptions = {
        queryParams: 'ignored',
        fragment: 'ignored',
        paths: 'subset',
        matrixParams: 'subset',
    };

    termLabels: string[] = [];

    @Input() number!: number;

    private _result!: EntrySummary;
    private readonly unsubscribe = new Subject<void>();

    @Input() set result(value: EntrySummary) {
        this._result = value;
        this._result.terms.sort((a, b) => a.sequence - b.sequence);
        this.termLabels = this._result.terms.map(term => this.getLabel(term));
        this.isValidated = this._result.status === Status.Validated;
        this.isMainOffice = this._result.isMainOffice;
        this.canBeAddedToWatchList = this._result.canBeAddedToWatchList;
        this.unsubscribe.next();
        this.subscribeToWatchlist();
    }
    get result(): EntrySummary {
        return this._result;
    }

    constructor(
        private readonly watchlistService: WatchlistService,
        private readonly authService: AuthService
    ) {
        this.isAuthenticated$ = this.authService.isAuthenticated$;
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    toggleWatchlistItem() {
        if (this.isWatchlistItem) {
            this.watchlistService.remove(this.result.id);
        } else {
            this.watchlistService.add(this.result.id);
        }
    }

    private getLabel(term: EntryLanguageSummary) {
        return [term.terminus, term.name, term.abbreviation].filter(x => x).join(this.separator);
    }

    private subscribeToWatchlist() {
        this.isAuthenticated$
            .pipe(
                switchMap(isAuth => (isAuth ? this.watchlistService.isWatchlistItem(this.result.id) : of(false))),
                takeUntil(this.unsubscribe)
            )
            .subscribe(isWatchlistItem => (this.isWatchlistItem = isWatchlistItem));
    }
}
