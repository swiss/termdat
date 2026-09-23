import {Clipboard} from '@angular/cdk/clipboard';
import {DatePipe, NgClass} from '@angular/common';
import {Component, Inject, Input, OnDestroy, OnInit} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatIcon} from '@angular/material/icon';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObNotificationService} from '@oblique/oblique';
import {EntryDetail} from 'api/EntryDetail';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {PictureDetail} from 'api/PictureDetail';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {combineLatest, from, of, Subject} from 'rxjs';
import {map, mergeAll, takeUntil} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {ConfigsService} from '../../config.service';
import {WINDOW} from '../../shared/injection.tokens';
import {PictureDialogComponent} from './picture-dialog/picture-dialog.component';
import {PictureMetadataComponent} from './picture-metadata/picture-metadata.component';
import {ReferencedByListComponent} from './referenced-by-list/referenced-by-list.component';

@Component({
    selector: 'td-entry-header',
    templateUrl: './entry-header.component.html',
    styleUrls: ['./entry-header.component.scss'],
    imports: [MatIconButton, ObButtonDirective, MatTooltip, MatIcon, NgClass, PictureMetadataComponent, ReferencedByListComponent, TranslatePipe, DatePipe],
})
export class EntryHeaderComponent implements OnInit, OnDestroy {
    @Input() set entryDetail(entry: EntryDetail | undefined) {
        this.entry = entry;
        this.updatedEntryDetail.next();
    }

    entry: EntryDetail | undefined;
    translatedPictureAltText: string | undefined;
    isValidated: boolean | undefined;
    statusName: string | undefined;
    reliabilityName: string | undefined;
    isMainOffice: boolean | undefined;
    emailLink = '';

    isAdmin = false;
    canSeeActiveApi = false;

    protected readonly ExternalLinkType = ExternalLinkType;

    private readonly updatedEntryDetail = new Subject<void>();
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly dialog: MatDialog,
        private readonly translateService: TranslateService,
        private readonly notificationService: ObNotificationService,
        private readonly clipboard: Clipboard,
        private readonly authService: AuthService,
        private readonly configService: ConfigsService,
        @Inject(WINDOW) private readonly window: Window
    ) {}

    ngOnInit(): void {
        from([of({}), this.translateService.onLangChange, this.updatedEntryDetail.asObservable()])
            .pipe(mergeAll(), takeUntil(this.unsubscribe))
            .subscribe(() => {
                if (!this.entry) {
                    return;
                }

                if (this.entry.picture) {
                    const descriptionKey = `description${this.translateService.getCurrentLang()[0].toLocaleUpperCase()}${this.translateService
                        .getCurrentLang()
                        .slice(1)}`;
                    this.translatedPictureAltText =
                        (this.entry.picture[descriptionKey as keyof PictureDetail] as string) ??
                        this.translateService.instant('entry.picture.descriptionNotSet');
                }

                this.isValidated = this.entry.status === Status.Validated;

                const status = this.entry.status ?? Status.InProgress;
                this.statusName = Status[status];

                const reliability = this.entry.reliability ?? Reliability.FirstDraft;
                this.reliabilityName = Reliability[reliability];

                this.isMainOffice = this.entry.isMainOffice!;
                this.emailLink = this.generateEmailLink();
            });

        this.authService.isAdmin$.subscribe(isAdmin => {
            this.isAdmin = isAdmin;
        });

        combineLatest([this.authService.isAdmin$, this.authService.isSuperUser$, this.authService.isWriter$])
            .pipe(
                map(([isAdmin, isSuperUser, isWriter]) => isAdmin || isSuperUser || isWriter),
                takeUntil(this.unsubscribe)
            )
            .subscribe(canSeeActiveApi => {
                this.canSeeActiveApi = canSeeActiveApi;
            });
    }

    ngOnDestroy(): void {
        this.updatedEntryDetail.complete();
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    openPicture(): void {
        this.dialog.open(PictureDialogComponent, {
            data: this.entry?.picture,
            disableClose: false,
            minWidth: '36vw',
            width: 'auto',
            maxWidth: '90vw',
        });
    }

    copyAsEntryUri(): void {
        if (!this.entry) {
            return;
        }

        const link = [this.window.location.origin, 'entry', this.entry?.id].join('/');
        if (this.clipboard.copy(link)) {
            this.notificationService.success('common.copySuccess');
        }
    }

    copyToClipboard(value: string): void {
        if (this.clipboard.copy(value)) {
            this.notificationService.success('common.copyToClipboardSuccess');
        }
    }

    private generateEmailLink(): string {
        if (!this.entry) {
            return '';
        }

        const entryUrl = encodeURI(`${this.window.location.origin}/entry/${this.entry?.id}`);

        // '%0D' entspricht einer New-Line. \n wird nicht korrekt per encodeURI encodiert, daher hier hart codiert
        const emailBody = `${encodeURI(this.translateService.instant('feedback.body'))}%0D%0D${entryUrl}`;
        const subject = encodeURI(this.translateService.instant('feedback.subject', {entryId: this.entry?.id}));

        return `mailto:${this.configService.frontendConfig.feedbackEmail}?subject=${subject}&body=${emailBody}`;
    }
}
