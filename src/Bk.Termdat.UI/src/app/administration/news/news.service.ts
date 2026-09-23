import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {NewsItemDetail} from 'api/NewsItemDetail';
import {NewsItemModification} from 'api/NewsItemModification';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {map, Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class NewsService {
    constructor(private readonly httpClient: HttpClient) {}

    getNewsSummaries(): Observable<NewsItemSummary[]> {
        return this.httpClient.get<NewsItemSummary[]>('/api/news').pipe(map(data => data.map(mapNewsSummaryDates)));
    }

    getActiveNewsSummaries(): Observable<NewsItemSummary[]> {
        return this.httpClient.get<NewsItemSummary[]>('/api/news/active').pipe(map(data => data.map(mapNewsSummaryDates)));
    }

    get(id: number): Observable<NewsItemModification> {
        return this.httpClient.get<NewsItemModification>(`/api/news/${id}`).pipe(map(mapNewsModificationDates));
    }

    create(data: NewsItemModification): Observable<NewsItemDetail> {
        return this.httpClient.post<NewsItemDetail>(`/api/news`, data);
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/news/${id}`);
    }

    update(data: NewsItemModification): Observable<NewsItemDetail> {
        return this.httpClient.put<NewsItemDetail>(`/api/news/${data.id}`, data);
    }

    moveUp(id: number): Observable<void> {
        return this.httpClient.put<void>(`/api/news/${id}/move-up`, {});
    }

    moveDown(id: number): Observable<void> {
        return this.httpClient.put<void>(`/api/news/${id}/move-down`, {});
    }
}

export const mapNewsSummaryDates = (data: NewsItemSummary): NewsItemSummary => ({
    ...data,
    created: new Date(data.created),
    modified: new Date(data.modified),
    validFrom: new Date(data.validFrom),
    validTo: new Date(data.validTo),
});

export const mapNewsModificationDates = (data: NewsItemModification): NewsItemModification => ({
    ...data,
    created: data.created ? new Date(data.created) : undefined,
    modified: data.modified ? new Date(data.modified) : undefined,
});
