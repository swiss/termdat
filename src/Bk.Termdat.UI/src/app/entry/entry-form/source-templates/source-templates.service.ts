import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {SourceTemplateModification} from 'api/SourceTemplateModification';
import {SourceTemplateSummary} from 'api/SourceTemplateSummary';
import {Observable} from 'rxjs';
import {shareReplay, tap} from 'rxjs/operators';
import {SourceTemplateDefaults} from './SourceTemplateDefaults';

@Injectable({
    providedIn: 'root',
})
export class SourceTemplatesService {
    private sourceTemplate$: Observable<SourceTemplateDefaults> | null = null;
    private readonly sourceTemplateTexts = new Map<string, Observable<string[]>>();

    constructor(private readonly httpClient: HttpClient) {}

    public getSourceTemplateDefaults(): Observable<SourceTemplateDefaults> {
        if (this.sourceTemplate$ === null) {
            this.sourceTemplate$ = this.httpClient.get<SourceTemplateDefaults>('/api/source-templates').pipe(shareReplay(1));
        }
        return this.sourceTemplate$;
    }

    public getSourceTemplates(): Observable<SourceTemplateSummary[]> {
        return this.httpClient.get<SourceTemplateSummary[]>(`/api/source-templates/current-user`);
    }

    public getSourceTemplateTexts(languageId: string): Observable<string[]> {
        if (!this.sourceTemplateTexts.has(languageId)) {
            const request$ = this.httpClient.get<string[]>(`/api/source-templates/current-user/texts/${languageId}`).pipe(shareReplay(1));
            this.sourceTemplateTexts.set(languageId, request$);
        }
        return this.sourceTemplateTexts.get(languageId)!;
    }

    public delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/source-templates/${id}`).pipe(tap(() => this.sourceTemplateTexts.clear()));
    }

    public create(data: SourceTemplateModification): Observable<void> {
        return this.httpClient.post<void>('/api/source-templates', data).pipe(tap(() => this.sourceTemplateTexts.clear()));
    }

    public update(data: SourceTemplateModification): Observable<void> {
        return this.httpClient.put<void>(`/api/source-templates/${data.id}`, data).pipe(tap(() => this.sourceTemplateTexts.clear()));
    }
}
