import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {Injectable, OnDestroy} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ConceptSystemPositionSummary} from 'api/ConceptSystemPositionSummary';
import {EntryChipFieldInputUpdate} from 'api/EntryChipFieldInputUpdate';
import {EntryChipFieldUpdate} from 'api/EntryChipFieldUpdate';
import {EntryCommentUpdate} from 'api/EntryCommentUpdate';
import {EntryConceptSystemPositionUpdate} from 'api/EntryConceptSystemPositionUpdate';
import {EntryContentMove} from 'api/EntryContentMove';
import {EntryContentMoveResult} from 'api/EntryContentMoveResult';
import {EntryDetail} from 'api/EntryDetail';
import {EntryHeaderModification} from 'api/EntryHeaderModification';
import {EntryModification} from 'api/EntryModification';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryMultipleUpdateInfo} from 'api/EntryMultipleUpdateInfo';
import {EntryMultipleUpdateResult} from 'api/EntryMultipleUpdateResult';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {EntrySearchAndReplace} from 'api/EntrySearchAndReplace';
import {EntrySourceUpdate} from 'api/EntrySourceUpdate';
import {EntrySourceUpdateResult} from 'api/EntrySourceUpdateResult';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {LockResult} from 'api/LockResult';
import {ReferenceType} from 'api/ReferenceType';
import {Reliability} from 'api/Reliability';
import {Observable, of, Subject} from 'rxjs';
import {map, tap} from 'rxjs/operators';
import {createFormData} from '../shared/FormDataCreator';
import {Option} from '../shared/Option';

@Injectable({
    providedIn: 'root',
})
export class EntryService implements OnDestroy {
    entryDeleted$: Observable<number>;
    entryUpdated$: Observable<number>;
    entrySelected$: Observable<number>;

    private readonly entryDeletedSubject = new Subject<number>();
    private readonly entryUpdatedSubject = new Subject<number>();
    private readonly entrySelectedSubject = new Subject<number>();

    constructor(
        private readonly httpClient: HttpClient,
        private readonly translateService: TranslateService
    ) {
        this.entryUpdated$ = this.entryUpdatedSubject.asObservable();
        this.entryDeleted$ = this.entryDeletedSubject.asObservable();
        this.entrySelected$ = this.entrySelectedSubject.asObservable();
    }

    ngOnDestroy(): void {
        this.entryUpdatedSubject.complete();
        this.entryDeletedSubject.complete();
        this.entrySelectedSubject.complete();
    }

    get(id: number, sourceLanguageIds?: number[], targetLanguageIds?: number[]): Observable<EntryDetail> {
        let params = new HttpParams();

        if (sourceLanguageIds?.length) {
            sourceLanguageIds.forEach(sourceLanguageId => {
                params = params.append('sourceLanguageIds', sourceLanguageId);
            });
        }
        if (targetLanguageIds?.length) {
            targetLanguageIds.forEach(targetLanguageId => {
                params = params.append('targetLanguageIds', targetLanguageId);
            });
        }
        return this.httpClient.get<EntryDetail>(`/api/entries/${id}`, {params}).pipe(map(EntryService.createDates));
    }

    getModification(id: number): Observable<EntryModificationDetail> {
        return this.httpClient.get<EntryModificationDetail>(`/api/entries/${id}/modification`);
    }

    delete(id: number, soft = true): Observable<EntryMultipleUpdateInfo[]> {
        return this.httpClient.delete<EntryMultipleUpdateInfo[]>(`/api/entries/${id}`, {params: {soft}}).pipe(tap(() => this.entryDeletedSubject.next(id)));
    }

    deleteMultiple(ids: number[], soft = true): Observable<EntryMultipleUpdateInfo[]> {
        return this.httpClient.delete<EntryMultipleUpdateInfo[]>(`/api/entries/multi-delete`, {body: ids, params: {soft}});
    }

    updateMultipleAdminInfos(update: EntryChipFieldInputUpdate): Observable<EntryMultipleUpdateResult> {
        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-multiple-admin-infos`, update);
    }

    updateMultipleGlossaries(update: EntryChipFieldUpdate): Observable<EntryMultipleUpdateResult> {
        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-multiple-glossaries`, update);
    }

    updateMultipleRelatedCollections(update: EntryChipFieldUpdate): Observable<EntryMultipleUpdateResult> {
        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-multiple-related-collections`, update);
    }

    updateMultipleComment(update: EntryCommentUpdate): Observable<EntryMultipleUpdateResult> {
        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-multiple-comment`, update);
    }

    updateMultipleConceptSystemPosition(update: EntryConceptSystemPositionUpdate): Observable<EntryMultipleUpdateResult> {
        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-multiple-concept-system-position`, update);
    }

    updateMultipleSource(update: EntrySourceUpdate): Observable<EntrySourceUpdateResult> {
        return this.httpClient.put<EntrySourceUpdateResult>(`/api/entries/update-multiple-source`, update);
    }

    moveContent(update: EntryContentMove): Observable<EntryContentMoveResult> {
        return this.httpClient.put<EntryContentMoveResult>(`/api/entries/move-content`, update);
    }

    restore(id: EntryDetail['id']): Observable<EntryDetail> {
        return this.httpClient.post<EntryDetail>(`/api/entries/restore`, id).pipe(
            map(EntryService.createDates),
            tap(() => this.entryUpdatedSubject.next(id))
        );
    }

    restoreMultiple(ids: number[]): Observable<EntryMultipleUpdateInfo[]> {
        return this.httpClient.post<EntryMultipleUpdateInfo[]>(`/api/entries/multi-restore`, ids);
    }

    create(data: EntryModification): Observable<EntryModificationDetail> {
        return this.httpClient.post<EntryModificationDetail>(`/api/entries`, data);
    }

    update(data: EntryModification): Observable<EntryModificationDetail> {
        const headers = new HttpHeaders();
        headers.append('Content-Type', 'multipart/form-data');
        headers.append('Accept', 'application/json');

        const formData = createFormData(data);

        return this.httpClient
            .put<EntryModificationDetail>(`/api/entries/${data.id}`, formData, {
                headers,
            })
            .pipe(
                tap(data => {
                    this.entryUpdatedSubject.next(data.id!);
                })
            );
    }

    updateMandatoryHeaderFields(data: EntryHeaderModification): Observable<EntryMultipleUpdateResult> {
        const headers = new HttpHeaders();
        headers.append('Content-Type', 'multipart/form-data');
        headers.append('Accept', 'application/json');

        const formData = createFormData(data);

        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/update-mandatory-header-fields`, formData, {
            headers,
        });
    }

    multipleSearchAndReplace(data: EntrySearchAndReplace): Observable<EntryMultipleUpdateResult> {
        const headers = new HttpHeaders();
        headers.append('Content-Type', 'multipart/form-data');
        headers.append('Accept', 'application/json');

        const formData = createFormData(data);

        return this.httpClient.put<EntryMultipleUpdateResult>(`/api/entries/multiple-search-and-replace`, formData, {
            headers,
        });
    }

    getReliabilities(): Observable<Option[]> {
        const reliabilities = Object.keys(Reliability)
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            .filter(k => typeof Reliability[k as any] === 'number')
            .map(value => {
                return {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    id: Reliability[value as any] as unknown as number,
                    name: this.translateService.instant(`field.reliability.${value}`),
                };
            });

        return of(reliabilities);
    }

    getConceptSystemPositions(collectionId: number): Observable<ConceptSystemPositionSummary[]> {
        const params = new HttpParams().set('collectionId', collectionId);

        return this.httpClient.get<ConceptSystemPositionSummary[]>('/api/concept-system-positions', {params});
    }

    selectEntry(entryId: number): void {
        this.entrySelectedSubject.next(entryId);
    }

    getReferenceTypes(): Observable<Option[]> {
        const reliabilities = Object.keys(ReferenceType)
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            .filter(k => typeof ReferenceType[k as any] === 'number')
            .map(value => {
                return {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    id: ReferenceType[value as any] as unknown as number,
                    name: this.translateService.instant(`field.reference.${value}`),
                };
            });

        return of(reliabilities);
    }

    getExternalLinkTypes(): Observable<Option[]> {
        const reliabilities = Object.keys(ExternalLinkType)
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            .filter(k => typeof ExternalLinkType[k as any] === 'number')
            .map(value => {
                return {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    id: ExternalLinkType[value as any] as unknown as number,
                    name: this.translateService.instant(`field.external-link.${value}`),
                };
            });

        return of(reliabilities);
    }

    getReferenceEntry(entryId: number): Observable<EntryReferenceDetail> {
        return this.httpClient.get<EntryReferenceDetail>(`/api/entries/${entryId}/reference`);
    }

    lock(entryId: number): Observable<LockResult> {
        return this.httpClient.put<LockResult>(`/api/entries/${entryId}/lock`, {});
    }

    unlock(entryId: number): Observable<LockResult> {
        return this.httpClient.put<LockResult>(`/api/entries/${entryId}/unlock`, {});
    }

    private static createDates(data: EntryDetail) {
        if (data.created) {
            data.created = new Date(data.created);
        }
        if (data.modified) {
            data.modified = new Date(data.modified);
        }
        if (data.locked) {
            data.locked = new Date(data.locked);
        }
        return data;
    }
}
