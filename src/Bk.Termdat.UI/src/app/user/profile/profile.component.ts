import {Component, OnDestroy, OnInit} from '@angular/core';
import {TranslatePipe} from '@ngx-translate/core';
import {Subject, combineLatest, merge} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {UserInfo} from '../../auth/UserInfo';
import {ConfigsService} from '../../config.service';
import {SourceTemplatesService} from '../../entry/entry-form/source-templates/source-templates.service';
import {EntryTemplateListComponent} from './entry-template-list/entry-template-list.component';
import {SourceTemplateListComponent} from './source-template-list/source-template-list.component';

@Component({
    selector: 'td-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.scss'],
    imports: [SourceTemplateListComponent, EntryTemplateListComponent, TranslatePipe],
})
export class ProfileComponent implements OnInit, OnDestroy {
    readonly myAccountHref: string;
    userInfo: UserInfo | undefined;
    isAdmin = false;
    isSuperUser = false;
    isWriter = false;

    private readonly sourceTemplateReload = new Subject<void>();

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly authService: AuthService,
        private readonly sourceTemplatesService: SourceTemplatesService,
        private readonly configService: ConfigsService
    ) {
        this.myAccountHref = this.configService.frontendConfig.myAccount;
    }

    ngOnInit(): void {
        this.authService.userInfo$.pipe(takeUntil(this.unsubscribe)).subscribe(data => {
            const roles = this.configService.frontendConfig.roles;
            const filteredRoles = data.roles?.filter(
                r => r === roles.allow || r === roles.admin || r === roles.superUser || r === roles.writer || r === roles.superReader
            );
            this.userInfo = {...data, roles: filteredRoles};
        });
        merge(this.sourceTemplateReload)
            .pipe(
                switchMap(() => this.sourceTemplatesService.getSourceTemplates()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(sourceTemplates => {
                this.userInfo = {...this.userInfo, sourceTemplates};
            });
        combineLatest([this.authService.isAdmin$, this.authService.isSuperUser$, this.authService.isWriter$])
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(([isAdmin, isSuperUser, isWriter]) => {
                this.isAdmin = isAdmin;
                this.isSuperUser = isSuperUser;
                this.isWriter = isWriter;

                if (isAdmin || isSuperUser || isWriter) {
                    this.reloadSourceTemplates();
                }
            });
    }

    reloadSourceTemplates() {
        this.sourceTemplateReload.next();
    }

    ngOnDestroy(): void {
        this.sourceTemplateReload.complete();
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }
}
