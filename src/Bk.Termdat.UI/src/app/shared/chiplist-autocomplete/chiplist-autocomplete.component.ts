import {COMMA, ENTER} from '@angular/cdk/keycodes';
import {AsyncPipe, NgClass} from '@angular/common';
import {AfterViewInit, ChangeDetectionStrategy, Component, DoCheck, ElementRef, Input, OnDestroy, OnInit, Renderer2, ViewChild} from '@angular/core';
import {
    ControlContainer,
    ControlValueAccessor,
    FormControlDirective,
    NG_VALUE_ACCESSOR,
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
} from '@angular/forms';
import {MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatOption} from '@angular/material/autocomplete';
import {MatChipGrid, MatChipInput, MatChipInputEvent, MatChipRemove, MatChipRow} from '@angular/material/chips';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatTooltip} from '@angular/material/tooltip';
import {Subject} from 'rxjs';
import {Observable} from 'rxjs/internal/Observable';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {ParentErrorStateMatcher} from '../parent-error-state-matcher';
import {Chip} from './Chip';

@Component({
    selector: 'td-chiplist-autocomplete',
    templateUrl: './chiplist-autocomplete.component.html',
    styleUrls: ['./chiplist-autocomplete.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: ChiplistAutocompleteComponent,
        },
    ],
    imports: [
        MatFormField,
        MatLabel,
        MatChipGrid,
        MatChipRow,
        MatTooltip,
        MatIcon,
        MatChipRemove,
        ReactiveFormsModule,
        MatAutocompleteTrigger,
        MatChipInput,
        MatAutocomplete,
        MatOption,
        NgClass,
        MatError,
        AsyncPipe,
    ],
})
export class ChiplistAutocompleteComponent implements OnInit, AfterViewInit, OnDestroy, DoCheck, ControlValueAccessor {
    @Input() formControl?: UntypedFormControl;
    @Input() formControlName?: string;

    get control() {
        const control = this.formControl ?? this.controlContainer.control?.get(this.formControlName!);
        if (!control) {
            throw new Error('Termdat-Chiplist-Autocomplete needs a form control. Please provide [formControl] or [formControlName].');
        }
        return control;
    }

    @Input() set chips(value: Chip[]) {
        this._chips = value;
        this.filteredChips$ = this.buildFilteredChips();
    }
    get chips() {
        return this._chips;
    }

    @Input() disableChipTooltips = false;
    @Input() disableAutocompleteTooltips = true;
    @Input() disableAutocompleteEllipsis = true;

    autocompleteInput: UntypedFormControl;
    separatorKeysCodes: number[] = [ENTER, COMMA];
    selectedChips: Chip[] = [];
    filteredChips$: Observable<Chip[]>;

    errorStateMatcher!: ParentErrorStateMatcher;
    @ViewChild('chipList') chipList!: MatChipGrid;

    @ViewChild(FormControlDirective, {static: true}) private readonly formControlDirective?: FormControlDirective;

    private _chips: Chip[] = [];
    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly controlContainer: ControlContainer,
        private readonly el: ElementRef,
        private readonly renderer: Renderer2
    ) {
        this.autocompleteInput = this.formBuilder.control('');
        this.filteredChips$ = this.buildFilteredChips();
    }

    ngOnInit(): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        this.control.valueChanges.pipe(startWith(this.control.value), takeUntil(this.unsubscribe)).subscribe((formValues?: any[]) => {
            const validChips = formValues?.map(v => this.chips.find(Chip.findByValue(v.toString())))?.filter(c => !!c) ?? [];

            this.selectedChips = validChips;
            // Maybe too much? Here because setting the value on the control from outside
            // doesn't update the filter without that (alternative: new Subject to update filter).
            this.autocompleteInput.setValue('');
            this.control.setValue(
                validChips.map(c => c.value),
                {emitEvent: false}
            );
        });

        this.errorStateMatcher = ParentErrorStateMatcher.create(this.control);
        this.control.statusChanges.pipe(takeUntil(this.unsubscribe)).subscribe(() => this.chipList.updateErrorState());
    }

    ngAfterViewInit() {
        this.checkIfMultiline();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    ngDoCheck(): void {
        this.chipList?.updateErrorState();
    }

    addItem(event: MatChipInputEvent): void {
        event.chipInput?.clear();

        const chipTag = event.value?.trim() ?? '';
        const chip = this.chips.find(Chip.findByTag(chipTag));

        if (chip?.value !== null && chip?.value !== undefined) {
            this.addInputToSelectedValues(chip.value.toString());
        }
    }

    selectItem(event: MatAutocompleteSelectedEvent): void {
        const value = event.option.value;

        if (value) {
            this.addInputToSelectedValues(value.toString());
        }
    }

    removeItem(value: string | number): void {
        this.onTouched();
        this.selectedChips = this.selectedChips.filter(c => c.value?.toString() !== value.toString());
        this.setValue(this.selectedChips.filter(c => c.value !== null && c.value !== undefined).map(c => c.value!));
    }

    addInputToSelectedValues(value: string) {
        const alreadySelected = this.selectedChips.some(Chip.findByValue(value));
        const newChip = this.chips.find(Chip.findByValue(value));

        if (newChip && !alreadySelected) {
            this.selectedChips.push(newChip);
            this.setValue(this.selectedChips.filter(c => c.value !== null && c.value !== undefined).map(c => c.value!));
        }

        // Patch value after updating selected chips, so the selected chips are correctly filtered out.
        this.autocompleteInput.patchValue('');
    }

    ensureOpenAutocomplete(autocomplete: MatAutocompleteTrigger) {
        if (!autocomplete.panelOpen) {
            autocomplete.openPanel();
        }
    }

    buildFilteredChips(): Observable<Chip[]> {
        return this.autocompleteInput.valueChanges.pipe(
            startWith(''),
            map((v?: string) => this.filterChips(v))
        );
    }

    filterChips(value?: string) {
        const unselectedChips = this.chips.filter(chip => !this.selectedChips.some(Chip.findByValue(chip.value ?? '')));

        if (!value) {
            return unselectedChips;
        }

        return unselectedChips.filter(chip => chip.displayText.toUpperCase().includes(value.toString().toUpperCase()));
    }

    setValue(ids?: (number | string)[]) {
        this.onChange(ids ?? []);
        this.control.setValue(ids ?? []);
    }

    // Empty functions to avoid calling them when undefined.
    // Although the events are passed to the value accessor of the form control directive,
    // we need to call them explicitly too (select all/none, onBlur, onSelectionChange).
    // Therefore we need properties to register them too.
    // If they aren't called correctly, the form control fails to update dirty/pristine/touched.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    onChange = (_: any[]) => {
        // This is intentional
    };
    onTouched = () => {
        // This is intentional
        this.chipList.updateErrorState();
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
        this.onTouched = () => {
            fn.call();
            this.chipList.updateErrorState();
        };

        if (this.formControlDirective) {
            this.formControlDirective.valueAccessor!.registerOnTouched(fn);
        }
    }

    setDisabledState(isDisabled: boolean): void {
        if (this.chipList) {
            this.chipList.disabled = isDisabled;
        }
    }

    private checkIfMultiline() {
        const parentElement = this.el.nativeElement.querySelector('.mat-mdc-text-field-wrapper');
        const childElement = parentElement?.querySelector('.mat-mdc-form-field-flex');

        if (childElement) {
            const observer = new ResizeObserver(entries => {
                for (const entry of entries) {
                    const height = entry.contentRect.height;

                    if (height > 65) {
                        this.renderer.addClass(parentElement, 'multi-line');
                    } else {
                        this.renderer.removeClass(parentElement, 'multi-line');
                    }
                }
            });

            observer.observe(childElement);
        }
    }
}
