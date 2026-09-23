import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {EntryModification} from 'api/EntryModification';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryTemplateSummary} from 'api/EntryTemplateSummary';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class EntryTemplateService {
    constructor(private readonly httpClient: HttpClient) {}

    getTemplateById(id: number): Observable<EntryModificationDetail> {
        return this.httpClient.get<EntryModificationDetail>(`/api/entry-templates/${id}`);
    }

    getEntryTemplates(): Observable<EntryTemplateSummary[]> {
        return this.httpClient.get<EntryTemplateSummary[]>(`/api/entry-templates`);
    }

    createEntryTemplate(templateName: string, entryTemplate: EntryModification): Observable<EntryModificationDetail> {
        entryTemplate.templateName = templateName;
        return this.httpClient.post<EntryModificationDetail>(`/api/entry-templates`, entryTemplate);
    }

    update(data: EntryModification): Observable<void> {
        return this.httpClient.put<void>(`/api/entry-templates/${data.id}`, data);
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/entry-templates/${id}`);
    }
}
