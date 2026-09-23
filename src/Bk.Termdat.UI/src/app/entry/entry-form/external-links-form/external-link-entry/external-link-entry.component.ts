import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {FormsModule, ReactiveFormsModule, UntypedFormGroup} from '@angular/forms';
import {MatOption} from '@angular/material/autocomplete';
import {MatIconButton} from '@angular/material/button';
import {MatError, MatFormField} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput, MatLabel} from '@angular/material/input';
import {MatSelect} from '@angular/material/select';
import {MatTooltip} from '@angular/material/tooltip';
import {ActivatedRoute} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective, ObExternalLinkModule} from '@oblique/oblique';
import {ExternalLinkDetail} from 'api/ExternalLinkDetail';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {merge, of, Subject} from 'rxjs';
import {switchMap, takeUntil} from 'rxjs/operators';
import {ErrorService} from '../../../../shared/error.service';
import {Option} from '../../../../shared/Option';
import {EntryService} from '../../../entry.service';
import {ExternalLinkMode} from './ExternalLinkMode';

@Component({
    selector: 'td-external-link-entry',
    imports: [
        FormsModule,
        MatError,
        MatFormField,
        MatIcon,
        MatIconButton,
        MatInput,
        MatLabel,
        MatOption,
        MatSelect,
        MatTooltip,
        ObButtonDirective,
        ObExternalLinkModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
    templateUrl: './external-link-entry.component.html',
    styleUrl: './external-link-entry.component.scss',
})
export class ExternalLinkEntryComponent implements OnInit, OnDestroy {
    externalLinkTypes: Option[] = [];
    EXTERNAL_LINK_MODE = ExternalLinkMode;

    @Input() mode: ExternalLinkMode = ExternalLinkMode.Create;
    @Input() form!: UntypedFormGroup;
    @Input() externalLink?: ExternalLinkDetail;

    @Output() readonly externalLinkRemoved = new EventEmitter<void>();
    @Output() readonly externalLinkCreated = new EventEmitter<ExternalLinkDetail>();

    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly activatedRoute: ActivatedRoute,
        public readonly errorService: ErrorService,
        private readonly translateService: TranslateService,
        private readonly entryService: EntryService
    ) {}

    ngOnInit(): void {
        merge(of(true), this.translateService.onLangChange)
            .pipe(
                switchMap(() => this.entryService.getExternalLinkTypes()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(options => (this.externalLinkTypes = options));
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    remove(): void {
        this.externalLinkRemoved.emit();
    }

    create(): void {
        this.form.markAllAsTouched();

        const value = {
            ...this.form.value,
        } as ExternalLinkDetail;

        this.externalLinkCreated.emit(value);

        this.form.reset({type: ExternalLinkType.None, url: null}, {emitEvent: false});
    }
}
