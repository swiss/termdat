import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ControlContainer, FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatAutocompleteModule, MatAutocompleteSelectedEvent} from '@angular/material/autocomplete';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';
import {ObInputClearDirective} from '@oblique/oblique';
import {MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {AutocompleteComponent} from './autocomplete.component';
import {Option} from './Option';

describe('AutocompleteComponent', () => {
    let component: AutocompleteComponent;
    let fixture: ComponentFixture<AutocompleteComponent>;

    const options: Option[] = [
        {id: 123, code: 'ABC', name: 'Option One'},
        {id: 234, code: 'AAA', name: 'Option Two'},
        {id: 345, code: 'ZZz', name: 'Option Three'},
    ];

    const formControl = new FormControl();
    const formControlName = 'autocomplete';

    const controlContainerMock = {
        control: {
            get: jest.fn(fcn => (fcn === formControlName ? formControl : null)),
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatInputModule),
                MockModule(MatIconModule),
                MockModule(MatOptionModule),
                MockModule(MatAutocompleteModule),
                AutocompleteComponent,
                MockDirective(ObInputClearDirective),
                MockPipe(TranslatePipe),
            ],
            providers: [{provide: ControlContainer, useValue: controlContainerMock}],
        }).compileComponents();

        fixture = TestBed.createComponent(AutocompleteComponent);
        component = fixture.componentInstance;

        formControl.reset();

        component.options = options;
        component.formControlName = formControlName;

        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should get form control', () => {
        component.formControlName = undefined;
        component.formControl = formControl;

        expect(component.control).toBe(formControl);
    });

    it('should select item when filtered options length is one', () => {
        expect(component.control.value).toBeFalsy();

        component.input.setValue('One');
        component.onInputChange();

        expect(component.control.value).toBe(123);
    });

    it.each([[''], [null], [undefined]])("should set value to null when input is '%s'", value => {
        component.input.setValue(value);
        component.onInputChange();

        expect(component.control.value).toBeNull();
    });

    it('should throw when form control is missing', () => {
        component.formControlName = '';
        component.formControl = undefined;

        expect(() => component.control).toThrow('Termdat Autocomplete needs a form control.');
    });

    it('should set value when triggered by (optionSelected)', () => {
        jest.spyOn(component, 'setValue');

        const eventMock = {option: {value: 234}} as MatAutocompleteSelectedEvent;

        component.selectItem(eventMock);

        expect(component.setValue).toHaveBeenCalledWith(234);
    });

    it('should clear', () => {
        component.setValue(123);
        expect(component.control.value).toBe(123);

        component.clear();
        expect(component.control.value).toBeFalsy();
        expect(component.input.value).toBeFalsy();
    });

    it('should filter Chips', () => {
        const filteredChips = component.filter('Option T');

        expect(filteredChips.length).toBeLessThan(component.options.length);

        const unfilteredChips = component.filter('');

        expect(unfilteredChips.length).toBe(component.options.length);
    });

    it('should call filter when input value changes', () => {
        jest.spyOn(component, 'filter');
        component.input.setValue('test');
        expect(component.filter).toHaveBeenCalledTimes(1);
    });

    it('should reset form', () => {
        component.options = options;
        component.control.setValue(123);
        expect(component.control.value).toBe(123);
        expect(component.input.value?.id).toBe(123);

        component.control.reset();
        expect(component.control.value).toBeFalsy();
        expect(component.input.value).toBeFalsy();
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

        component.registerOnTouched('');
        // eslint-disable-next-line dot-notation
        expect(component['formControlDirective']?.valueAccessor?.registerOnTouched).toHaveBeenCalledTimes(1);
    });

    it('should disable input control when isDisabled', () => {
        component.isDisabled = true;

        expect(component.input.disabled).toBe(true);
    });
});
