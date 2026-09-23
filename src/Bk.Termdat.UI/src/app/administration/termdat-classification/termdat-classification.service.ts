import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {DomainDetail} from 'api/DomainDetail';
import {DomainModification} from 'api/DomainModification';
import {DomainSummary} from 'api/DomainSummary';
import {LanguageDescription} from 'api/LanguageDescription';
import {SubjectDetail} from 'api/SubjectDetail';
import {SubjectModification} from 'api/SubjectModification';
import {SubjectSummary} from 'api/SubjectSummary';
import {TermdatClassificationDetail} from 'api/TermdatClassificationDetail';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class TermdatClassificationService {
    constructor(private readonly httpClient: HttpClient) {}

    getSubjects(): Observable<SubjectSummary[]> {
        return this.httpClient.get<SubjectSummary[]>('/api/subjects');
    }

    getSubjectDetails(): Observable<TermdatClassificationDetail[]> {
        return this.httpClient.get<SubjectDetail[]>('/api/subjects/details').pipe(
            map(subjectDetails =>
                subjectDetails
                    .flatMap(subject => [null, ...subject.domains].map(domain => ({subject, domain})))
                    .map(({subject, domain}) => {
                        const changeTracking = domain ?? subject;
                        return {
                            id: subject.id,
                            subjectName: subject.name,
                            domainId: domain?.id,
                            domainName: domain?.name,
                            entryCount: changeTracking.entryCount,
                            created: changeTracking.created,
                            createdBy: changeTracking.createdBy,
                            modified: changeTracking.modified,
                            modifiedBy: changeTracking.modifiedBy,
                        };
                    })
            ),
            map(tClassificationDetails => tClassificationDetails.map(TermdatClassificationService.createDates))
        );
    }

    getSubject(id: SubjectDetail['id']): Observable<SubjectModification> {
        return this.httpClient.get<SubjectModification>(`/api/subjects/${id}`);
    }

    createSubject(data: SubjectModification): Observable<SubjectDetail> {
        return this.httpClient.post<SubjectDetail>('/api/subjects', data).pipe(map(TermdatClassificationService.createDates));
    }

    updateSubject(data: SubjectModification): Observable<SubjectDetail> {
        return this.httpClient.put<SubjectDetail>(`/api/subjects/${data.id}`, data).pipe(map(TermdatClassificationService.createDates));
    }

    deleteSubject(id: SubjectDetail['id']) {
        return this.httpClient.delete(`/api/subjects/${id}`);
    }

    getDomainLanguageDescriptions(): Observable<LanguageDescription[]> {
        return this.httpClient.get<LanguageDescription[]>(`/api/domains/descriptions`);
    }

    updateDomain(data: DomainModification): Observable<DomainDetail> {
        return this.httpClient.put<DomainDetail>(`/api/domains/${data.id}`, data).pipe(map(TermdatClassificationService.createDates));
    }

    deleteDomain(id: DomainSummary['id']): Observable<void> {
        return this.httpClient.delete<void>(`/api/domains/${id}`);
    }

    private static createDates<T extends TermdatClassificationDetail | SubjectDetail | DomainDetail>(data: T) {
        data.created = new Date(data.created);
        data.modified = new Date(data.modified);
        return data;
    }
}
