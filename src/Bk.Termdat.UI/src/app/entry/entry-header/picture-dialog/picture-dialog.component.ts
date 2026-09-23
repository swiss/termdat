import {CdkScrollable} from '@angular/cdk/scrolling';
import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MAT_DIALOG_DATA, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {MatIcon} from '@angular/material/icon';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {PictureDetail} from 'api/PictureDetail';
import {from, of, Subject} from 'rxjs';
import {mergeAll, takeUntil} from 'rxjs/operators';

@Component({
    selector: 'td-picture-dialog',
    templateUrl: './picture-dialog.component.html',
    styleUrls: ['./picture-dialog.component.scss'],
    imports: [MatDialogTitle, MatIconButton, MatDialogClose, MatIcon, CdkScrollable, MatDialogContent, TranslatePipe],
})
export class PictureDialogComponent implements OnInit, OnDestroy {
    translatedPictureAltText: string | undefined;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        protected readonly dialogRef: MatDialogRef<void>,
        @Inject(MAT_DIALOG_DATA) public data: PictureDetail,
        private readonly translateService: TranslateService
    ) {}

    ngOnInit(): void {
        from([of({}), this.translateService.onLangChange])
            .pipe(mergeAll(), takeUntil(this.unsubscribe))
            .subscribe(() => {
                const descriptionKey = `description${this.translateService.getCurrentLang()[0].toLocaleUpperCase()}${this.translateService.getCurrentLang().slice(1)}`;
                this.translatedPictureAltText = this.data[descriptionKey as keyof PictureDetail] as string;
            });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }
}
