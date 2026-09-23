import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {GlossaryDetail} from 'api/GlossaryDetail';
import {GlossaryModification} from 'api/GlossaryModification';
import {GlossarySummary} from 'api/GlossarySummary';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class GlossaryService {
    constructor(private readonly httpClient: HttpClient) {}

    getGlossaryDetails(): Observable<GlossaryDetail[]> {
        return this.httpClient.get<GlossaryDetail[]>('/api/glossaries/details');
    }

    getGlossarySummaries(): Observable<GlossarySummary[]> {
        return this.httpClient.get<GlossarySummary[]>('/api/glossaries');
    }

    getModificationGlossaries(): Observable<GlossarySummary[]> {
        return this.httpClient.get<GlossarySummary[]>('/api/glossaries/modification');
    }

    get(id: number): Observable<GlossaryModification> {
        return this.httpClient.get<GlossaryModification>(`/api/glossaries/${id}`).pipe(map(GlossaryService.parseModificationDates));
    }

    update(data: GlossaryModification): Observable<GlossaryDetail> {
        return this.httpClient.put<GlossaryDetail>(`/api/glossaries/${data.id}`, data);
    }

    create(data: GlossaryModification): Observable<GlossaryDetail> {
        return this.httpClient.post<GlossaryDetail>(`/api/glossaries`, data);
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/glossaries/${id}`);
    }

    private static parseModificationDates(data: GlossaryModification): GlossaryModification {
        if (data.created) {
            data.created = new Date(data.created);
        }
        if (data.modified) {
            data.modified = new Date(data.modified);
        }
        return data;
    }
}
