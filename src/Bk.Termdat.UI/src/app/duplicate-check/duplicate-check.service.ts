import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {DuplicateCheckRequest} from 'api/DuplicateCheckRequest';
import {DuplicateCheckResponse} from 'api/DuplicateCheckResponse';
import {map, Observable, tap} from 'rxjs';
import {downloadFile, downloadTxtFile} from '../shared/download.helper';

@Injectable({
    providedIn: 'root',
})
export class DuplicateCheckService {
    constructor(
        private readonly httpClient: HttpClient,
        private readonly translateService: TranslateService
    ) {}

    public performDuplicateCheck(data: DuplicateCheckRequest): Observable<DuplicateCheckResponse> {
        return this.httpClient.post<DuplicateCheckResponse>('/api/search/duplicate-check', data);
    }

    public exportDuplicateCheckXlsx(data: DuplicateCheckRequest): Observable<void> {
        return this.httpClient
            .post('/api/search/duplicate-check/xlsx', data, {
                responseType: 'blob',
                observe: 'response',
            })
            .pipe(
                tap(blob => downloadFile(blob, 'Export.xlsx')),
                map(() => undefined)
            );
    }

    exportDuplicateCheckTxt(entryIds: number[]): void {
        const data = entryIds.join(',');

        const datePrefix = new Date().toISOString().split('T')[0].replace(/-/g, '');

        downloadTxtFile(data, this.translateService.instant('duplicateCheck.exportEntryIdsTxtFilename', {date: datePrefix}));
    }
}
