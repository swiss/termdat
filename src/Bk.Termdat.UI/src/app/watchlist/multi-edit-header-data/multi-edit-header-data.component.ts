import {Location} from '@angular/common';
import {Component, OnInit} from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonModule} from '@oblique/oblique';
import {MultiEditChipCollectionsComponent} from '../multi-edit-chip/multi-edit-chip-collections/multi-edit-chip-collections.component';
import {MultiEditChipGlossariesComponent} from '../multi-edit-chip/multi-edit-chip-glossaries/multi-edit-chip-glossaries.component';
import {MultiEditAdminInfosComponent} from '../multi-edit-chip-input/multi-edit-admininfos/multi-edit-admininfos.component';

@Component({
    selector: 'td-multi-edit-header-data',
    templateUrl: './multi-edit-header-data.component.html',
    styleUrls: ['./multi-edit-header-data.component.scss'],
    imports: [
        MultiEditAdminInfosComponent,
        MultiEditChipGlossariesComponent,
        MultiEditChipCollectionsComponent,
        MatIcon,
        RouterLink,
        TranslatePipe,
        MatButtonModule,
        ObButtonModule,
    ],
})
export class MultiEditHeaderDataComponent implements OnInit {
    ids: number[] = [];

    constructor(
        private readonly location: Location,
        private readonly router: Router
    ) {}

    ngOnInit(): void {
        const state = this.location.getState() as {ids: number[]};
        this.ids = state?.ids ?? [];
        if (this.ids.length === 0) {
            void this.router.navigate(['watchlist']);
        }
    }
}
