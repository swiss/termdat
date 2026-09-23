import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {ConceptSystemDetail} from 'api/ConceptSystemDetail';
import {ConceptSystemModification} from 'api/ConceptSystemModification';
import {ConceptSystemSummary} from 'api/ConceptSystemSummary';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class ConceptSystemService {
    constructor(private readonly httpClient: HttpClient) {}

    get(id: ConceptSystemSummary['id']): Observable<ConceptSystemModification> {
        return this.httpClient.get<ConceptSystemModification>(`/api/concept-systems/${id}`);
    }

    getConceptSystems(): Observable<ConceptSystemSummary[]> {
        return this.httpClient.get<ConceptSystemSummary[]>('/api/concept-systems/');
    }

    getConceptSystemDetails(): Observable<ConceptSystemDetail[]> {
        return this.httpClient.get<ConceptSystemDetail[]>('/api/concept-systems/details').pipe(
            map(data => {
                return data.map(ConceptSystemService.createDates);
            })
        );
    }

    update(data: ConceptSystemModification): Observable<ConceptSystemDetail> {
        return this.httpClient.put<ConceptSystemDetail>(`/api/concept-systems/${data.id}`, data).pipe(map(ConceptSystemService.createDates));
    }

    create(data: ConceptSystemModification): Observable<ConceptSystemDetail> {
        return this.httpClient.post<ConceptSystemDetail>(`/api/concept-systems`, data).pipe(map(ConceptSystemService.createDates));
    }

    delete(id: ConceptSystemDetail['id']): Observable<void> {
        return this.httpClient.delete<void>(`/api/concept-systems/${id}`);
    }

    private static createDates(data: ConceptSystemDetail) {
        data.created = new Date(data.created);
        data.modified = new Date(data.modified);

        return data;
    }
}
