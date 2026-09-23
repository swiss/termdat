import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {ClassificationSummary} from '../../../api/ClassificationSummary';

@Injectable({
    providedIn: 'root',
})
export class ClassificationService {
    constructor(private readonly httpClient: HttpClient) {}

    getClassifications(): Observable<ClassificationSummary[]> {
        return this.httpClient.get<ClassificationSummary[]>('/api/classifications');
    }
}
