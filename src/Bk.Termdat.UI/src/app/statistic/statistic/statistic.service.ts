import {HttpClient, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {StatisticSearchResult} from 'api/StatisticSearchResult';
import {Observable} from 'rxjs';
import {append} from '../../shared/http-params-util';
import {Pagination} from '../../shared/Pagination';

@Injectable({
    providedIn: 'root',
})
export class StatisticService {
    constructor(private readonly httpClient: HttpClient) {}

    getStatisticsBySearchQuery(pagination: Pagination, sort: string, phrase: string, fromDate?: Date, toDate?: Date): Observable<StatisticSearchResult> {
        const params = StatisticService.getSearchHttpParams(pagination, sort, phrase, fromDate, toDate);

        return this.httpClient.get<StatisticSearchResult>('/api/statistics', {params});
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/statistics/${id}`);
    }

    deleteSelection(pagination: Pagination, sort: string, phrase: string, fromDate: Date, toDate: Date): Observable<void> {
        const params = StatisticService.getSearchHttpParams(pagination, sort, phrase, fromDate, toDate);

        return this.httpClient.delete<void>('/api/statistics', {params});
    }

    private static getSearchHttpParams(pagination: Pagination, sort: string, phrase: string, fromDate?: Date, toDate?: Date): HttpParams {
        let params = new HttpParams();

        if (pagination.pageIndex) {
            params = append(params, 'pageIndex', pagination.pageIndex);
        }

        if (pagination.pageSize) {
            params = append(params, 'pageSize', pagination.pageSize);
        }

        if (sort) {
            params = append(params, 'sort', sort);
        }

        if (phrase) {
            params = append(params, 'phrase', phrase);
        }

        if (fromDate) {
            params = append(params, 'fromDate', fromDate);
        }

        if (toDate) {
            params = append(params, 'toDate', toDate);
        }

        return params;
    }
}
