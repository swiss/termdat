import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {AdminInfoDetail} from 'api/AdminInfoDetail';
import {AdminInfoModification} from 'api/AdminInfoModification';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class AdminInfoService {
    constructor(private readonly httpClient: HttpClient) {}

    get(id: number): Observable<AdminInfoModification> {
        return this.httpClient.get<AdminInfoModification>(`/api/admin-infos/${id}`);
    }

    getAdminInfos(): Observable<AdminInfoSummary[]> {
        return this.httpClient.get<AdminInfoSummary[]>('/api/admin-infos');
    }

    getAdminInfoDetails(): Observable<AdminInfoDetail[]> {
        return this.httpClient.get<AdminInfoDetail[]>('/api/admin-infos/details').pipe(
            map(data => {
                return data.map(AdminInfoService.createDates);
            })
        );
    }

    update(data: AdminInfoModification): Observable<AdminInfoDetail> {
        return this.httpClient.put<AdminInfoDetail>(`/api/admin-infos/${data.id}`, data).pipe(map(AdminInfoService.createDates));
    }

    create(data: AdminInfoModification): Observable<AdminInfoDetail> {
        return this.httpClient.post<AdminInfoDetail>('/api/admin-infos', data).pipe(map(AdminInfoService.createDates));
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/admin-infos/${id}`);
    }

    private static createDates(data: AdminInfoDetail) {
        data.created = new Date(data.created);
        data.modified = new Date(data.modified);

        return data;
    }
}
