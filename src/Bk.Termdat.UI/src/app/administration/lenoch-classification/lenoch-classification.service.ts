import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {LenochClassificationDetail} from 'api/LenochClassificationDetail';
import {LenochClassificationSummary} from 'api/LenochClassificationSummary';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class LenochClassificationService {
    constructor(private readonly httpClient: HttpClient) {}

    getLenochClassifications(): Observable<LenochClassificationSummary[]> {
        return this.httpClient.get<LenochClassificationSummary[]>('/api/lenoch-classifications');
    }

    getLenochClassificationDetails(): Observable<LenochClassificationDetail[]> {
        return this.httpClient.get<LenochClassificationDetail[]>('/api/lenoch-classifications/details');
    }
}
