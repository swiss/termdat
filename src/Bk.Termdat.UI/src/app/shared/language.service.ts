import {HttpClient, HttpParams} from '@angular/common/http';
import {Injectable, OnDestroy} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {LanguageDetail} from 'api/LanguageDetail';
import {LanguageModification} from 'api/LanguageModification';
import {LanguageSummary} from 'api/LanguageSummary';
import {BehaviorSubject, merge, Observable, of, Subject} from 'rxjs';
import {distinctUntilChanged, filter, map, switchMap, take, takeUntil, tap} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class LanguageService implements OnDestroy {
    readonly languageSummaries$: Observable<LanguageSummary[]>;

    private currentLanguageSummaries: LanguageSummary[] = [];

    private readonly languageSubject = new BehaviorSubject<LanguageSummary[]>([]);
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly httpClient: HttpClient,
        private readonly translateService: TranslateService,
        private readonly httpApiInterceptorEvents: ObHttpApiInterceptorEvents
    ) {
        this.languageSummaries$ = this.languageSubject.asObservable();
    }

    init() {
        merge(of(this.translateService.getCurrentLang()), this.translateService.onLangChange.pipe(map(data => data.lang)))
            .pipe(
                filter((lang): lang is string => !!lang),
                distinctUntilChanged(),
                switchMap(lang => this.getLanguage(lang)),
                tap(translations => {
                    this.currentLanguageSummaries = translations;
                    this.languageSubject.next(this.currentLanguageSummaries);
                }),
                takeUntil(this.unsubscribe)
            )
            .subscribe();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    get(id: number): Observable<LanguageModification> {
        return this.httpClient.get<LanguageModification>(`/api/languages/${id}`);
    }

    getLanguage(isoCode: string): Observable<LanguageSummary[]> {
        const params = new HttpParams().set('isoCode', isoCode);

        this.httpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);

        return this.httpClient.get<LanguageSummary[]>('/api/languages/get-by-iso-code', {params});
    }

    getLanguageDetails(): Observable<LanguageDetail[]> {
        return this.httpClient.get<LanguageDetail[]>('/api/languages/details').pipe(
            map(data => {
                return data.map(LanguageService.createDates);
            })
        );
    }

    nextSort(): Observable<number> {
        return this.httpClient.get<number>('/api/languages/next-sort').pipe(
            map(data => {
                return data;
            })
        );
    }

    create(data: LanguageModification): Observable<LanguageDetail> {
        return this.httpClient.post<LanguageDetail>('/api/languages', data).pipe(map(LanguageService.createDates));
    }

    update(data: LanguageModification): Observable<LanguageDetail> {
        return this.httpClient.put<LanguageDetail>(`/api/languages/${data.id}`, data).pipe(map(LanguageService.createDates));
    }

    getLanguageName(languageId: number): string {
        return this.currentLanguageSummaries.find(t => t.id === languageId)?.name || languageId.toString();
    }

    getCurrentLanguageSummaries(): Observable<LanguageSummary[]> {
        const currentLang = this.translateService.getCurrentLang();

        return this.getLanguage(currentLang);
    }

    updateLanguageSummaries(): void {
        const currentLang = this.translateService.getCurrentLang();

        this.getLanguage(currentLang)
            .pipe(take(1))
            .subscribe(translations => {
                this.currentLanguageSummaries = translations;
                this.languageSubject.next(this.currentLanguageSummaries);
            });
    }

    isLangActive(lang: string): boolean {
        return this.translateService.getCurrentLang() === lang;
    }

    changeLang(lang: string): void {
        this.translateService.use(lang);
    }

    getCurrentLanguageId(): number {
        const currentLang = this.currentLanguageSummaries.find(lang => lang.isoCode.toUpperCase() === this.translateService.getCurrentLang().toUpperCase());
        return currentLang ? currentLang.id : 0;
    }

    private static createDates(data: LanguageDetail) {
        data.created = new Date(data.created);
        data.modified = new Date(data.modified);

        return data;
    }
}
