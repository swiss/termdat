import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {OfficeDetail} from 'api/OfficeDetail';
import {OfficeSummary} from 'api/OfficeSummary';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {OfficeModification} from '../../../api/OfficeModification';

@Injectable({
    providedIn: 'root',
})
export class OfficeService {
    constructor(private readonly httpClient: HttpClient) {}

    get(id: number): Observable<OfficeModification> {
        return this.httpClient.get<OfficeModification>(`/api/offices/${id}`);
    }

    getOffices(): Observable<OfficeSummary[]> {
        return this.httpClient.get<OfficeSummary[]>('/api/offices');
    }

    getOfficeDetails(): Observable<OfficeDetail[]> {
        return this.httpClient.get<OfficeDetail[]>('/api/offices/details').pipe(
            map(data => {
                return data.map(OfficeService.createDates);
            })
        );
    }

    update(data: OfficeModification): Observable<OfficeDetail> {
        return this.httpClient.put<OfficeDetail>(`/api/offices/${data.id}`, data).pipe(map(OfficeService.createDates));
    }

    create(data: OfficeModification): Observable<OfficeDetail> {
        return this.httpClient.post<OfficeDetail>('/api/offices', data).pipe(map(OfficeService.createDates));
    }

    delete(id: number): Observable<void> {
        return this.httpClient.delete<void>(`/api/offices/${id}`);
    }

    private static createDates(data: OfficeDetail) {
        data.created = new Date(data.created);
        data.modified = new Date(data.modified);

        return data;
    }
}
