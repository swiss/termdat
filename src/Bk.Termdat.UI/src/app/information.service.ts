import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Version} from 'api/Version';
import {first} from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class InformationService {
    applicationVersion?: string;

    constructor(
        private readonly http: HttpClient,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    loadApplicationVersion(): void {
        this.httpInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);

        this.http
            .get<Version>('/api/system-information/version')
            .pipe(first())
            .subscribe(v => (this.applicationVersion = v.applicationVersion));
    }
}
