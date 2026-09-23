import {Component, Input} from '@angular/core';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {PictureDetail} from 'api/PictureDetail';

@Component({
    selector: 'td-picture-metadata',
    templateUrl: './picture-metadata.component.html',
    styleUrls: ['./picture-metadata.component.scss'],
    imports: [TranslatePipe],
})
export class PictureMetadataComponent {
    @Input() picture?: PictureDetail;

    constructor(public readonly translateService: TranslateService) {}
}
