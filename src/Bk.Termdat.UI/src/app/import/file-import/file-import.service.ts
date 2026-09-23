import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {FileImport} from 'api/FileImport';
import {FileImportResult} from 'api/FileImportResult';
import {Observable} from 'rxjs';
import {createFormData} from '../../shared/FormDataCreator';

@Injectable({
    providedIn: 'root',
})
export class FileImportService {
    constructor(private readonly httpClient: HttpClient) {}

    importFile(data: FileImport): Observable<FileImportResult> {
        const headers = new HttpHeaders();
        headers.append('Content-Type', 'multipart/form-data');
        headers.append('Accept', 'application/json');

        const formData = createFormData(data);

        return this.httpClient.post<FileImportResult>('/api/files/entries:import', formData, {
            headers,
        });
    }
}
