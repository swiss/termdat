import {AsyncPipe, KeyValuePipe} from '@angular/common';
import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormGroup} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteTrigger, MatOption} from '@angular/material/autocomplete';
import {MatIconButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {LanguageDescription} from 'api/LanguageDescription';
import {EMPTY, Observable} from 'rxjs';
import {map, startWith} from 'rxjs/operators';

@Component({
    selector: 'td-classification-domain-form',
    templateUrl: './domain-form.component.html',
    styleUrls: ['./domain-form.component.scss'],
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatIconButton,
        ObButtonDirective,
        MatTooltip,
        MatIcon,
        MatAutocompleteTrigger,
        MatAutocomplete,
        MatOption,
        MatError,
        AsyncPipe,
        KeyValuePipe,
        TranslatePipe,
    ],
})
export class TermdatClassificationDomainFormComponent implements OnInit {
    @Input() form!: UntypedFormGroup;
    @Input() descriptions: LanguageDescription[] = [];
    @Output() readonly remove = new EventEmitter<void>();

    filteredDescriptionsDe$: Observable<LanguageDescription['descriptionDe'][]> = EMPTY;
    filteredDescriptionsFr$: Observable<LanguageDescription['descriptionFr'][]> = EMPTY;
    filteredDescriptionsEn$: Observable<LanguageDescription['descriptionEn'][]> = EMPTY;
    filteredDescriptionsRm$: Observable<LanguageDescription['descriptionRm'][]> = EMPTY;
    filteredDescriptionsIt$: Observable<LanguageDescription['descriptionIt'][]> = EMPTY;

    ngOnInit(): void {
        this.filteredDescriptionsDe$ = this.form.controls.descriptionDe.valueChanges.pipe(
            startWith(''),
            map(item => this.filterDescriptions('descriptionDe', item)),
            map(items => TermdatClassificationDomainFormComponent.sort(items)),
            map(items => TermdatClassificationDomainFormComponent.onlyUnique(items))
        );
        this.filteredDescriptionsFr$ = this.form.controls.descriptionFr.valueChanges.pipe(
            startWith(''),
            map(item => this.filterDescriptions('descriptionFr', item)),
            map(items => TermdatClassificationDomainFormComponent.sort(items)),
            map(items => TermdatClassificationDomainFormComponent.onlyUnique(items))
        );
        this.filteredDescriptionsEn$ = this.form.controls.descriptionEn.valueChanges.pipe(
            startWith(''),
            map(item => this.filterDescriptions('descriptionEn', item)),
            map(items => TermdatClassificationDomainFormComponent.sort(items)),
            map(items => TermdatClassificationDomainFormComponent.onlyUnique(items))
        );
        this.filteredDescriptionsRm$ = this.form.controls.descriptionRm.valueChanges.pipe(
            startWith(''),
            map(item => this.filterDescriptions('descriptionRm', item)),
            map(items => TermdatClassificationDomainFormComponent.sort(items)),
            map(items => TermdatClassificationDomainFormComponent.onlyUnique(items))
        );
        this.filteredDescriptionsIt$ = this.form.controls.descriptionIt.valueChanges.pipe(
            startWith(''),
            map(item => this.filterDescriptions('descriptionIt', item)),
            map(items => TermdatClassificationDomainFormComponent.sort(items)),
            map(items => TermdatClassificationDomainFormComponent.onlyUnique(items))
        );
    }

    private static sort(values: string[]): string[] {
        return values.sort((lhs, rhs) => lhs.localeCompare(rhs));
    }

    private static onlyUnique(values: string[]): string[] {
        return values.filter((value, index, self) => self.indexOf(value) === index);
    }

    private filterDescriptions<Key extends keyof LanguageDescription>(language: Key, value: LanguageDescription[Key]): LanguageDescription[Key][] {
        return this.descriptions.map(item => item[language]).filter(item => item.toLocaleLowerCase().startsWith(value.toLocaleLowerCase()));
    }
}
