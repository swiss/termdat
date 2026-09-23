import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ControlContainer, FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatAutocompleteModule, MatAutocompleteSelectedEvent, MatAutocompleteTrigger} from '@angular/material/autocomplete';
import {MatChipInputEvent, MatChipsModule} from '@angular/material/chips';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {MockModule, MockPipe} from 'ng-mocks';
import {Chip} from './Chip';
import {ChiplistAutocompleteComponent} from './chiplist-autocomplete.component';

describe('ChiplistAutocompleteComponent', () => {
    let component: ChiplistAutocompleteComponent;
    let fixture: ComponentFixture<ChiplistAutocompleteComponent>;

    const chips: Chip[] = [new Chip('T01', 'TE1 - Test 01', 'test01'), new Chip('ClI', 'ClI - ChiplistItem', 'cli')];

    const chipValue = 'test01';
    const chipTag = 'T01';

    let formControl: FormControl;
    const formControlName = 'formControlName';

    const controlContainerMock = {
        control: {
            get: jest.fn(),
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatChipsModule),
                MockModule(MatIconModule),
                MockModule(MatOptionModule),
                MockModule(MatAutocompleteModule),
                MockModule(MatTooltipModule),
                ChiplistAutocompleteComponent,
                MockPipe(TranslatePipe),
            ],
            providers: [{provide: ControlContainer, useValue: controlContainerMock}],
        }).compileComponents();

        fixture = TestBed.createComponent(ChiplistAutocompleteComponent);
        component = fixture.componentInstance;

        formControl = new FormControl();
        controlContainerMock.control.get.mockImplementation(fcn => (fcn === formControlName ? formControl : null));

        component.formControl = formControl;
        component.formControlName = formControlName;
        component.chips = chips;

        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should throw when form control is missing', () => {
        component.formControlName = '';
        component.formControl = undefined;

        expect(() => component.control).toThrow('Termdat-Chiplist-Autocomplete needs a form control. Please provide [formControl] or [formControlName].');

        component.formControl = formControl;
        component.formControlName = formControlName;
    });

    it('should ensure autocomplete', () => {
        const autocompleteMock = {
            panelOpen: false,
            openPanel: jest.fn(),
        } as unknown as MatAutocompleteTrigger;

        component.ensureOpenAutocomplete(autocompleteMock);

        expect(autocompleteMock.openPanel).toHaveBeenCalled();
    });

    it('should add Item when triggered by (matChipInputTokenEnd)', () => {
        jest.spyOn(component, 'addInputToSelectedValues');

        const eventMock = {value: chipTag, chipInput: {clear: () => {}}} as MatChipInputEvent;

        component.addItem(eventMock);

        expect(component.addInputToSelectedValues).toHaveBeenCalledWith(chipValue);
    });

    it('should select Item when triggered by (optionSelected)', () => {
        jest.spyOn(component, 'addInputToSelectedValues');

        const eventMock = {option: {value: chipValue}} as MatAutocompleteSelectedEvent;

        component.selectItem(eventMock);

        expect(component.addInputToSelectedValues).toHaveBeenCalledWith(chipValue);
    });

    it('should add input to selected values and remove Item when triggered by (removed)', () => {
        component.addInputToSelectedValues(chipValue);

        expect(component.selectedChips.length).toEqual(1);
        expect(component.selectedChips[0].value).toEqual(chipValue);
        expect(component.control.value).toEqual([chipValue]);

        component.removeItem(chipValue);
        expect(component.selectedChips.length).toEqual(0);
        expect(component.control.value).toEqual([]);
    });

    it('should filter Chips', () => {
        const filteredChips = component.filterChips('test');

        expect(filteredChips.length).toBeLessThan(component.chips.length);

        const unfilteredChips = component.filterChips('cli');

        expect(unfilteredChips.length).toBeLessThan(component.chips.length);
    });

    it('should call writeValue', () => {
        Object.defineProperty(component, 'formControlDirective', {
            value: {
                valueAccessor: {
                    writeValue: jest.fn(),
                },
            },
        });

        component.writeValue('');
        // eslint-disable-next-line dot-notation
        expect(component['formControlDirective']?.valueAccessor?.writeValue).toHaveBeenCalledTimes(1);
    });

    it('should call registerOnChange', () => {
        Object.defineProperty(component, 'formControlDirective', {
            value: {
                valueAccessor: {
                    registerOnChange: jest.fn(),
                },
            },
        });

        component.registerOnChange('');
        // eslint-disable-next-line dot-notation
        expect(component['formControlDirective']?.valueAccessor?.registerOnChange).toHaveBeenCalledTimes(1);
    });

    it('should call registerOnTouched', () => {
        Object.defineProperty(component, 'formControlDirective', {
            value: {
                valueAccessor: {
                    registerOnTouched: jest.fn(),
                },
            },
        });

        component.formControl = formControl;

        component.registerOnTouched('');
        // eslint-disable-next-line dot-notation
        expect(component['formControlDirective']?.valueAccessor?.registerOnTouched).toHaveBeenCalledTimes(1);
    });

    it('should handle input clear', () => {
        component.control.setValue([chipValue]);
        expect(component.selectedChips.length).toEqual(1);

        component.control.reset();
        expect(component.selectedChips.length).toEqual(0);
    });
});
