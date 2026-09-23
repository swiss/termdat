import {NgClass} from '@angular/common';
import {AfterViewChecked, ChangeDetectorRef, Component, OnDestroy, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatCheckbox} from '@angular/material/checkbox';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatSelect, MatSelectChange} from '@angular/material/select';
import {ActivatedRoute, ParamMap} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObAlertComponent} from '@oblique/oblique';
import {EntryDetail} from 'api/EntryDetail';
import {merge, of, Subject} from 'rxjs';
import {catchError, distinctUntilChanged, filter, switchMap, takeUntil} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {SearchService} from '../../search/search.service';
import {DisplayMode} from '../../shared/DisplayMode';
import {EntryButtonListComponent} from '../entry-button-list/entry-button-list.component';
import {EntryHeaderComponent} from '../entry-header/entry-header.component';
import {EntryService} from '../entry.service';
import {DisplayService} from './entry-detail/display.service';
import {EntryDetailComponent} from './entry-detail/entry-detail.component';

@Component({
    selector: 'td-entry-detail-form',
    templateUrl: './entry-detail-form.component.html',
    styleUrls: ['./entry-detail-form.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatCheckbox,
        MatFormField,
        MatLabel,
        MatSelect,
        MatOption,
        EntryButtonListComponent,
        ObAlertComponent,
        EntryHeaderComponent,
        EntryDetailComponent,
        NgClass,
        TranslatePipe,
    ],
})
export class EntryDetailFormComponent implements OnInit, AfterViewChecked, OnDestroy {
    currentEntry: EntryDetail | undefined;
    canCreateAndDeleteAndUpdate = false;
    hideHeadData = false;

    currentDisplay: DisplayMode = DisplayMode.Complete;

    form: UntypedFormGroup;

    DisplayMode = DisplayMode;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly entryService: EntryService,
        private readonly authService: AuthService,
        private readonly activatedRoute: ActivatedRoute,
        private readonly translateService: TranslateService,
        private readonly searchService: SearchService,
        private readonly displayService: DisplayService,
        private readonly changeDetectorRef: ChangeDetectorRef
    ) {
        this.form = this.formBuilder.group({
            displayOptions: null,
        });
    }

    ngAfterViewChecked(): void {
        this.changeDetectorRef.detectChanges();
    }

    ngOnInit(): void {
        merge(
            this.activatedRoute.paramMap.pipe(
                switchMap((params: ParamMap) => {
                    const value = params.get('id') ?? '0';
                    const id = parseInt(value, 10);
                    return of(isNaN(id) ? 0 : id);
                }),
                distinctUntilChanged()
            ),
            this.translateService.onLangChange.pipe(switchMap(() => of(this.currentEntry?.id ?? 0))),
            // This observable is used to reload the current entry, since filters for languages could have changed when search button is pressed
            // Reason: Angular does not re-emit on paramMap if the parameter has not changed on .navigate()
            this.entryService.entrySelected$.pipe(filter((id: number) => !!this.currentEntry && this.currentEntry.id === id))
        )
            .pipe(
                takeUntil(this.unsubscribe),
                filter((id: number) => id > 0),
                switchMap((id: number) => {
                    const sourceLanguages = this.searchService.getSourceLanguages();
                    const targetLanguages = this.searchService.getTargetLanguages();
                    return this.entryService.get(id, sourceLanguages, targetLanguages).pipe(
                        catchError(() => {
                            return of(undefined);
                        })
                    );
                })
            )
            .subscribe(entry => {
                this.currentEntry = entry;
            });

        this.authService.roles$.subscribe(roles => {
            this.canCreateAndDeleteAndUpdate = roles.length > 0;
        });

        this.setCurrentDisplay();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    changeDisplayMode(event: MatSelectChange): void {
        this.changeDisplay(event.value);
    }

    getLockedMinutes(locked: Date) {
        const now = new Date();
        const differenceMs = now.getTime() - locked.getTime();

        const minutes = Math.round(differenceMs / 1000 / 60);

        if (minutes < 1) {
            return 1;
        }

        return minutes;
    }

    private setCurrentDisplay() {
        this.currentDisplay = this.displayService.getDisplayMode();
        this.form.controls.displayOptions.patchValue(this.currentDisplay);
    }

    private changeDisplay(view: DisplayMode) {
        // write to local storage
        this.displayService.setDisplayMode(view);
        this.currentDisplay = view;
    }
}
