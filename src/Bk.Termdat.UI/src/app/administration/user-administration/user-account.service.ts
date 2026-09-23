import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {UserAccountModification} from 'api/UserAccountModification';
import {UserAccountSummary} from 'api/UserAccountSummary';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class UserAccountService {
    constructor(private readonly httpClient: HttpClient) {}

    getUserAccountSummaries(): Observable<UserAccountSummary[]> {
        return this.httpClient.get<UserAccountSummary[]>('/api/user-accounts');
    }

    get(id: number): Observable<UserAccountDetail> {
        return this.httpClient.get<UserAccountDetail>(`/api/user-accounts/${id}`);
    }

    update(update: UserAccountModification): Observable<void> {
        return this.httpClient.put<void>('/api/user-accounts', update);
    }

    getUserAccountDetails(): Observable<UserAccountDetail[]> {
        return this.httpClient.get<UserAccountDetail[]>('/api/user-accounts/details');
    }

    createOrUpdate(): Observable<UserAccountDetail> {
        return this.httpClient.post<UserAccountDetail>('/api/user-accounts', {});
    }
}
