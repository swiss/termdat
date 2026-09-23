import {Injectable, OnDestroy} from '@angular/core';
import {MatPaginatorIntl} from '@angular/material/paginator';
import {TranslateService} from '@ngx-translate/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';

@Injectable()
export class TermdatPaginatorIntl extends MatPaginatorIntl implements OnDestroy {
    private ofLabel?: string;
    private totalLabel?: string;

    private readonly unsubscribe = new Subject<void>();

    constructor(private readonly translateService: TranslateService) {
        super();

        this.nextPageLabel = '';
        this.previousPageLabel = '';
        this.lastPageLabel = '';
        this.firstPageLabel = '';

        this.getTranslationChanges();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    getTranslationChanges(): void {
        this.translateService.stream(['pagination.entriesPerPage', 'pagination.of', 'pagination.total']).subscribe(translation => {
            this.itemsPerPageLabel = `${translation['pagination.entriesPerPage']}`;
            this.ofLabel = translation['pagination.of'];
            this.totalLabel = translation['pagination.total'];
            this.changes.next();
        }, takeUntil(this.unsubscribe));
    }

    getRangeLabel = (pageIndex: number, pageSize: number, length: number): string => {
        length = Math.max(length, 0);

        if (length === 0) {
            return `0 - 0 ${this.ofLabel} ${length} ${this.totalLabel}`;
        }

        const startIndex = pageIndex * pageSize;
        const endIndex = startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;
        return `${startIndex + 1} - ${endIndex} ${this.ofLabel} ${length} ${this.totalLabel}`;
    };
}
