import {Component, OnDestroy, OnInit} from '@angular/core';
import {TranslatePipe} from '@ngx-translate/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {ConfigsService} from '../../config.service';
import {AuthService} from '../auth.service';

@Component({
    selector: 'td-unauthorized',
    templateUrl: './unauthorized.component.html',
    styleUrls: ['./unauthorized.component.scss'],
    imports: [TranslatePipe],
})
export class UnauthorizedComponent implements OnInit, OnDestroy {
    showRequestAccessLink = false;
    requestAccessLink = '';

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly authService: AuthService,
        private readonly configService: ConfigsService
    ) {}

    ngOnInit(): void {
        this.authService.roles$.pipe(takeUntil(this.unsubscribe)).subscribe(roles => {
            if (!roles.length) {
                this.showRequestAccessLink = true;
                this.requestAccessLink = this.configService.frontendConfig.requestAccessLink + encodeURIComponent(this.configService.frontendConfig.appUrl);
            }
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }
}
