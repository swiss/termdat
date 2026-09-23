import {AsyncPipe, NgClass} from '@angular/common';
import {Component, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {ControlContainer, ControlValueAccessor, FormControlDirective, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatOption} from '@angular/material/autocomplete';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {ObInputClearDirective} from '@oblique/oblique';
import {combineLatest, Observable, Subject} from 'rxjs';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {ParentErrorStateMatcher} from '../parent-error-state-matcher';
import {Option} from './Option';

@Component({
    selector: 'td-autocomplete',
    templateUrl: './autocomplete.component.html',
    styleUrls: ['./autocomplete.component.scss'],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: AutocompleteComponent,
        },
    ],
    imports: [
        MatFormField,
        MatLabel,
        MatInput,
        ReactiveFormsModule,
        MatAutocompleteTrigger,
        ObInputClearDirective,
        MatIcon,
        MatAutocomplete,
        MatOption,
        NgClass,
        MatError,
        AsyncPipe,
    ],
})
export class AutocompleteComponent implements OnInit, OnDestroy, ControlValueAccessor {
    @Input() formControl?: UntypedFormControl;
    @Input() formControlName?: string;
    @Input() disableAutocompleteEllipsis = true;

    @Input() set isDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.input.disable();
        } else {
            this.input.enable();
        }
    }

    get control() {
        const control = this.formControl ?? this.controlContainer.control?.get(this.formControlName!);
        if (!control) {
            throw new Error('Termdat Autocomplete needs a form control.');
        }
        return control;
    }

    input: UntypedFormControl = new UntypedFormControl();

    @Input() set options(value: Option[] | null) {
        this._options = value ?? [];
        this.optionsSubject.next(value ?? []);
        this.filteredOptions$ = this.buildFilteredOptions();
    }
    get options(): Option[] {
        return this._options;
    }

    filteredOptions$: Observable<Option[]> = this.buildFilteredOptions();

    errorStateMatcher!: ParentErrorStateMatcher;

    @ViewChild(FormControlDirective, {static: true}) private readonly formControlDirective?: FormControlDirective;
    private _options: Option[] = [];
    private readonly optionsSubject = new Subject<Option[]>();
    private readonly unsubscribe = new Subject<void>();

    constructor(private readonly controlContainer: ControlContainer) {}

    ngOnInit(): void {
        // We can't know, if the options or the initial value is set first. In case the value is set first,
        // it's not found in the (empty) options and therefore the control is resetted. With combineLatest,
        // we'll get the latest value again when the options are set. Then we can successfully update the controls.
        combineLatest([this.control.valueChanges, this.optionsSubject])
            .pipe(takeUntil(this.unsubscribe))
            .subscribe(([value, _]) => {
                const validValue = value ? this.options.find(o => o.id === value) : null;

                this.input.setValue(validValue);
                this.control.setValue(validValue?.id, {emitEvent: false});
            });

        this.errorStateMatcher = ParentErrorStateMatcher.create(this.control);
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    setValue(id?: number | null) {
        this.onChange(id);
        this.control.setValue(id);
    }

    onInputChange() {
        const options = this.filter(this.input.value);
        if (options.length === 1) {
            this.setValue(options[0]?.id);
        }

        if (!this.input.value) {
            this.setValue(null);
        }
    }

    selectItem(event: MatAutocompleteSelectedEvent): void {
        this.setValue(event.option.value);
    }

    clear() {
        this.setValue(null);
    }

    // We need a function builder to properly capture this.options.
    /* eslint-disable @typescript-eslint/no-explicit-any */
    getDisplayFn(): (value: any) => string {
        return (value?: any) => this.getOptionDisplay(this.options.find(o => o.id === value));
        /* eslint-enable */
    }

    buildFilteredOptions() {
        return this.input.valueChanges.pipe(
            startWith(''),
            map(v => this.filter(v))
        );
    }

    getOptionDisplay(option?: Option): string {
        return option ? AutocompleteComponent.toLabel(option) : '';
    }

    filter(value: string | number): Option[] {
        if (value == null) {
            return this.options;
        }

        const v = value.toString().toLocaleUpperCase();
        return this.options?.filter(
            o =>
                o.code?.toLocaleUpperCase().startsWith(v) ||
                o.name?.toLocaleUpperCase().includes(v) ||
                this.getOptionDisplay(o).toLocaleUpperCase().startsWith(v)
        );
    }

    // Empty functions to avoid calling them when undefined.
    // Although the events are passed to the value accessor of the form control directive,
    // we need to call them explicitly too (select all/none, onBlur, onSelectionChange).
    // Therefore we need properties to register them too.
    // If they aren't called correctly, the form control fails to update dirty/pristine/touched.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    onChange = (_: any) => {
        // This is intentional
    };
    onTouched = () => {
        // This is intentional
    };

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    writeValue(obj: any): void {
        if (this.formControlDirective) {
            this.formControlDirective.valueAccessor!.writeValue(obj);
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    registerOnChange(fn: any): void {
        this.onChange = fn;
        if (this.formControlDirective) {
            this.formControlDirective.valueAccessor!.registerOnChange(fn);
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    registerOnTouched(fn: any): void {
        this.onTouched = fn;
        if (this.formControlDirective) {
            this.formControlDirective.valueAccessor!.registerOnTouched(fn);
        }
    }

    private static toLabel(obj?: {code?: string; name: string}): string {
        if (!obj) {
            return '-';
        }

        if (!obj.code) {
            return obj.name ?? '-';
        }

        return `${obj.code} - ${obj.name}`;
    }
}
