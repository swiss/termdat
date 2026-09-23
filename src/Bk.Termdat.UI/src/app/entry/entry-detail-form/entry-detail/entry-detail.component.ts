import {KeyValuePipe, NgClass} from '@angular/common';
import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {TranslatePipe} from '@ngx-translate/core';
import {EntryDetail} from 'api/EntryDetail';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {SearchService} from '../../../search/search.service';
import {DisplayMode} from '../../../shared/DisplayMode';
import {LanguageService} from '../../../shared/language.service';
import {LanguageDetailComponent} from '../language-detail/language-detail.component';

@Component({
    selector: 'td-entry-detail',
    templateUrl: './entry-detail.component.html',
    styleUrls: ['./entry-detail.component.scss'],
    imports: [NgClass, LanguageDetailComponent, TranslatePipe, KeyValuePipe],
})
export class EntryDetailComponent implements OnInit, OnDestroy {
    @Input() set entry(val: EntryDetail | undefined) {
        this._entry = val;
        this.groupByLanguage();
    }
    @Input() currentDisplay?: DisplayMode;
    details: {[key: string]: EntryLanguageDetail[]} = {};
    detailsCount = 0;

    DisplayMode = DisplayMode;

    private _entry: EntryDetail | undefined;
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly languageService: LanguageService,
        private readonly searchService: SearchService
    ) {}

    ngOnInit(): void {
        this.languageService.languageSummaries$.pipe(takeUntil(this.unsubscribe)).subscribe(() => {
            const languageIds = this.searchService.getVisibleLanguages();
            this.groupByLanguage(languageIds);
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    // avoid alphabetical search of keyvalue
    unsorted(): number {
        return 0;
    }

    private groupByLanguage(languageIds: number[] = []) {
        this.details = {};
        this.detailsCount = 0;

        if (!this._entry) {
            return;
        }

        this._entry?.languageDetails
            ?.filter(ld => !languageIds || languageIds.length === 0 || languageIds.includes(ld.languageId))
            .forEach(detail => {
                const language = this.languageService.getLanguageName(detail.languageId);

                if (!this.details.hasOwnProperty(language)) {
                    this.details[language] = [];
                }

                this.details[language].push(detail);
            });

        this.detailsCount = Object.keys(this.details).length;
    }
}
