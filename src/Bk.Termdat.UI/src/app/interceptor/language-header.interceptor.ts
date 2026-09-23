import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {Observable} from 'rxjs';

@Injectable()
export class LanguageHeaderInterceptor implements HttpInterceptor {
    constructor(private readonly translateService: TranslateService) {}

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        const currentLang = this.translateService.getCurrentLang() || 'de';
        request = request.clone({headers: request.headers.append('Accept-Language', currentLang)});
        return next.handle(request);
    }
}
