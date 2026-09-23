import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {EntryReferenceSummary} from 'api/EntryReferenceSummary';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {LanguageService} from '../../../shared/language.service';

@Component({
    selector: 'td-referenced-by-list',
    templateUrl: './referenced-by-list.component.html',
    styleUrls: ['./referenced-by-list.component.scss'],
    imports: [RouterLink, TranslatePipe],
})
export class ReferencedByListComponent implements OnInit, OnDestroy {
    displayReferences: EntryReferenceSummary[] = [];

    private _references: EntryReferenceDetail[] = [];
    private readonly unsubscribe = new Subject<void>();

    @Input() set references(value: EntryReferenceDetail[]) {
        this._references = value;
        this.displayReferences = this.createDisplayReferences(value);
    }

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        private readonly translateService: TranslateService,
        private readonly languageService: LanguageService
    ) {}

    ngOnInit(): void {
        this.translateService.onLangChange
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(() => (this.displayReferences = this.createDisplayReferences(this._references)));
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    private createDisplayReferences(references: EntryReferenceDetail[]): EntryReferenceSummary[] {
        if (!references?.length) {
            return [];
        }

        const currentLangId = this.languageService.getCurrentLanguageId();

        const referenceMap: {[key: number]: EntryReferenceSummary} = {};
        references.forEach(reference => {
            const languageHit = reference.languages.find(lang => lang.languageId === currentLangId);

            if (languageHit) {
                referenceMap[reference.entryId] = {
                    type: reference.type,
                    entryId: reference.entryId,
                    text: languageHit.text,
                } as EntryReferenceSummary;
            }
        });

        const mappedReferences = Object.values(referenceMap);

        mappedReferences.sort((a, b) => {
            return a.text.localeCompare(b.text, this.translateService.getCurrentLang(), {sensitivity: 'accent'});
        });

        return mappedReferences;
    }
}
