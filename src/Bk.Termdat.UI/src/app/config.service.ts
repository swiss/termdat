import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {ObHttpApiInterceptorEvents, ObIBanner} from '@oblique/oblique';
import {Observable, tap} from 'rxjs';

export interface FrontendConfig {
    banner: string;
    isProduction: boolean;
    requestAccessLink: string;
    myAccount: string;
    appUrl: string;
    roles: {
        allow: string;
        admin: string;
        writer: string;
        superUser: string;
        superReader: string;
    };
    defaultOfficeId: number;
    defaultCollectionId: number;
    feedbackEmail: string;
    analytics: {
        enabled: boolean;
        trackerUrl: string;
        containerUrl: string;
    };
    froalaKey: string;
}

@Injectable({providedIn: 'root'})
export class ConfigsService {
    frontendConfig!: FrontendConfig;
    bannerInfo!: ObIBanner;

    constructor(
        private readonly http: HttpClient,
        private readonly httpInterceptorEvents: ObHttpApiInterceptorEvents
    ) {}

    loadFrontendConfig(): Observable<FrontendConfig> {
        this.httpInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);

        return this.http.get<FrontendConfig>('/api/configs/frontend').pipe(
            tap(c => {
                this.bannerInfo = {text: c.banner};
                this.frontendConfig = c;
            })
        );
    }
}
