import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {CollectionDetail} from 'api/CollectionDetail';
import {CollectionModification} from 'api/CollectionModification';
import {CollectionSummary} from 'api/CollectionSummary';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class CollectionService {
    constructor(private readonly httpClient: HttpClient) {}

    getCollectionDetails(): Observable<CollectionDetail[]> {
        return this.httpClient.get<CollectionDetail[]>('/api/collections/details');
    }

    getActiveCollections(): Observable<CollectionSummary[]> {
        return this.httpClient.get<CollectionSummary[]>('/api/collections/active');
    }

    getModificationCollections(): Observable<CollectionSummary[]> {
        return this.httpClient.get<CollectionSummary[]>('/api/collections/modification');
    }

    getAllCollections(): Observable<CollectionSummary[]> {
        return this.httpClient.get<CollectionSummary[]>('/api/collections');
    }

    get(id: number): Observable<CollectionModification> {
        return this.httpClient.get<CollectionModification>(`/api/collections/${id}`).pipe(map(CollectionService.parseModificationDates));
    }

    update(data: CollectionModification): Observable<CollectionDetail> {
        return this.httpClient.put<CollectionDetail>(`/api/collections/${data.id}`, data);
    }

    create(data: CollectionModification): Observable<CollectionDetail> {
        return this.httpClient.post<CollectionDetail>(`/api/collections`, data);
    }

    delete(id: number, soft = true): Observable<void> {
        return this.httpClient.delete<void>(`/api/collections/${id}`, {params: {soft}});
    }

    restore(id: number): Observable<CollectionDetail> {
        return this.httpClient.post<CollectionDetail>(`/api/collections/restore`, id);
    }

    private static parseModificationDates(data: CollectionModification): CollectionModification {
        if (data.created) {
            data.created = new Date(data.created);
        }
        if (data.modified) {
            data.modified = new Date(data.modified);
        }
        return data;
    }
}
