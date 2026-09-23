import {HttpClient, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {PagingParameters} from 'api/PagingParameters';
import {SearchPage} from 'api/SearchPage';
import {SearchQuery} from 'api/SearchQuery';
import {SearchQueryFields} from 'api/SearchQueryFields';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class SearchService {
    private targetLanguages: number[] = [];
    private sourceLanguages: number[] = [];

    constructor(private readonly httpClient: HttpClient) {}

    getSearchResult(
        phrase: string,
        paging: PagingParameters,
        sourceLanguageIds?: number[],
        targetLanguageIds?: number[],
        extendedSearch?: SearchQuery
    ): Observable<SearchPage> {
        const params = SearchService.getSearchHttpParams(phrase, paging, sourceLanguageIds, targetLanguageIds, extendedSearch);

        return this.httpClient.get<SearchPage>('/api/search/search', {params});
    }

    getRecommendation(phrase: string, sourceLanguageIds?: number[]): Observable<string[]> {
        let params = new HttpParams().set('phrase', phrase);
        if (sourceLanguageIds?.length) {
            sourceLanguageIds.forEach(sourceLanguageId => {
                params = params.append('sourceLanguageIds', sourceLanguageId);
            });
        }

        return this.httpClient.get<string[]>('/api/search/recommend', {params});
    }

    getSourceLanguages(): number[] {
        return this.sourceLanguages;
    }

    getTargetLanguages(): number[] {
        return this.targetLanguages;
    }

    changeTargetLanguages(chosenLanguages: number[]) {
        this.targetLanguages = chosenLanguages;
    }

    changeSourceLanguages(chosenLanguages: number[]) {
        this.sourceLanguages = chosenLanguages;
    }

    getVisibleLanguages(): number[] {
        if (this.targetLanguages.length === 0 || this.sourceLanguages.length === 0) {
            return [];
        }

        return [...this.sourceLanguages, ...this.targetLanguages];
    }

    private static getSearchHttpParams(
        phrase: string,
        paging: PagingParameters,
        sourceLanguageIds?: number[],
        targetLanguageIds?: number[],
        extendedSearch?: SearchQuery
    ): HttpParams {
        let params = new HttpParams().set('pageindex', paging.pageIndex).set('pagesize', paging.pageSize);
        if (phrase) {
            params = SearchService.append(params, 'phrase', phrase);
        }
        params = SearchService.appendArray(params, 'sourceLanguageIds', sourceLanguageIds);
        params = SearchService.appendArray(params, 'targetLanguageIds', targetLanguageIds);

        if (extendedSearch) {
            params = SearchService.append(params, 'entryIds', extendedSearch.entryIds);
            params = SearchService.appendArray(params, 'offices', extendedSearch.offices);
            params = SearchService.append(params, 'officesPriority', extendedSearch.officesPriority);
            params = SearchService.appendArray(params, 'lenochClassifications', extendedSearch.lenochClassifications);
            params = SearchService.append(params, 'lenochClassificationsPriority', extendedSearch.lenochClassificationsPriority);
            params = SearchService.appendArray(params, 'subjectDomains', extendedSearch.subjectDomains);
            params = SearchService.append(params, 'subjectDomainsPriority', extendedSearch.subjectDomainsPriority);
            params = SearchService.appendArray(params, 'collections', extendedSearch.collections);
            params = SearchService.append(params, 'collectionsPriority', extendedSearch.collectionsPriority);
            params = SearchService.append(params, 'collectionsExclude', extendedSearch.collectionsExclude);
            params = SearchService.appendArray(params, 'relatedCollections', extendedSearch.relatedCollections);
            params = SearchService.appendArray(params, 'glossaries', extendedSearch.glossaries);
            params = SearchService.append(params, 'glossariesExclude', extendedSearch.glossariesExclude);
            params = SearchService.appendArray(params, 'status', extendedSearch.status);
            params = SearchService.append(params, 'statusPriority', extendedSearch.statusPriority);
            params = SearchService.appendArray(params, 'adminInfos', extendedSearch.adminInfos);
            params = SearchService.append(params, 'adminInfosPriority', extendedSearch.adminInfosPriority);
            params = SearchService.append(params, 'adminInfosExclude', extendedSearch.adminInfosExclude);
            params = SearchService.appendArray(params, 'commentStatus', extendedSearch.commentStatus);
            params = SearchService.append(params, 'createdBy', extendedSearch.createdBy);
            params = SearchService.append(params, 'createdByUser', extendedSearch.createdByUser);
            params = SearchService.append(params, 'createdByPriority', extendedSearch.createdByPriority);
            params = SearchService.append(params, 'createdByExclude', extendedSearch.createdByExclude);
            params = SearchService.append(params, 'modifiedBy', extendedSearch.modifiedBy);
            params = SearchService.append(params, 'modifiedByUser', extendedSearch.modifiedByUser);
            params = SearchService.append(params, 'modifiedByPriority', extendedSearch.modifiedByPriority);
            params = SearchService.append(params, 'modifiedByExclude', extendedSearch.modifiedByExclude);
            params = SearchService.append(params, 'modifiedFrom', extendedSearch.modifiedFrom);
            params = SearchService.append(params, 'modifiedTo', extendedSearch.modifiedTo);
            params = SearchService.append(params, 'modifiedPeriodPriority', extendedSearch.modifiedPeriodPriority);
            params = SearchService.append(params, 'modifiedPeriodExclude', extendedSearch.modifiedPeriodExclude);
            params = SearchService.appendArray(params, 'reliability', extendedSearch.reliability);
            params = SearchService.append(params, 'reliabilityPriority', extendedSearch.reliabilityPriority);
            params = SearchService.append(params, 'deleted', extendedSearch.deleted);
            params = SearchService.append(params, 'deletedPriority', extendedSearch.deletedPriority);
            params = SearchService.append(params, 'hasMissingSource', extendedSearch.hasMissingSource);
            params = SearchService.appendFields(params, extendedSearch.fields);
        }

        return params;
    }

    private static appendArray(params: HttpParams, name: string, ids?: number[] | string[]): HttpParams {
        if (!ids?.length) {
            return params;
        }

        ids.forEach(id => {
            params = params.append(name, id);
        });

        return params;
    }

    private static append(params: HttpParams, name: string, value?: string | Date | boolean) {
        if (value === null || value === undefined || value === '' || value === false) {
            return params;
        }

        if (value instanceof Date) {
            return params.set(name, value.toISOString());
        }

        return params.set(name, value);
    }

    private static appendFields(params: HttpParams, fields?: SearchQueryFields) {
        if (!fields) {
            return params;
        }

        params = params
            .set('fields.term', fields.term)
            .set('fields.name', fields.name)
            .set('fields.abbreviation', fields.abbreviation)
            .set('fields.phraseology', fields.phraseology)
            .set('fields.definition', fields.definition)
            .set('fields.note', fields.note)
            .set('fields.context', fields.context)
            .set('fields.source', fields.source)
            .set('fields.metadata', fields.metadata)
            .set('fields.country', fields.country)
            .set('fields.comment', fields.comment);

        return params;
    }
}
