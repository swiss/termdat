import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatCheckbox, MatCheckboxModule} from '@angular/material/checkbox';
import {By} from '@angular/platform-browser';
import {MockModule} from 'ng-mocks';
import {SelectAllComponent} from './select-all.component';

describe('SelectAllComponent', () => {
    let component: SelectAllComponent;
    let fixture: ComponentFixture<SelectAllComponent>;

    let formBuilder: FormBuilder;
    let selectControl: FormControl;

    const clickEventMock = {
        stopPropagation: jest.fn(),
        preventDefault: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ReactiveFormsModule, MockModule(MatCheckboxModule), SelectAllComponent],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        selectControl = formBuilder.control([]);

        fixture = TestBed.createComponent(SelectAllComponent);
        component = fixture.componentInstance;
        component.selectControl = selectControl;
        component.values = [1, 2, 3, 4, 5];

        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize isChecked and isIndeterminate', () => {
        expect(component.isChecked).toEqual(false);
        expect(component.isIndeterminate).toEqual(false);
    });

    it('should set isChecked correctly', () => {
        selectControl.setValue([1]);
        expect(component.isChecked).toEqual(false);

        selectControl.setValue(component.values);
        expect(component.isChecked).toEqual(true);

        selectControl.setValue([1, 2, 3]);
        expect(component.isChecked).toEqual(false);

        selectControl.setValue([]);
        expect(component.isChecked).toEqual(false);
    });

    it('should set isIndeterminate correctly', () => {
        selectControl.setValue([1]);
        expect(component.isIndeterminate).toEqual(true);

        selectControl.setValue(component.values);
        expect(component.isIndeterminate).toEqual(false);

        selectControl.setValue([1, 2, 3]);
        expect(component.isIndeterminate).toEqual(true);

        selectControl.setValue([]);
        expect(component.isIndeterminate).toEqual(false);
    });

    it('should set all values when checkbox is not checked on click', () => {
        const checkbox = fixture.debugElement.query(By.directive(MatCheckbox));
        const checkboxComponent = checkbox.componentInstance as MatCheckbox;
        checkboxComponent.checked = false;

        selectControl.setValue([1, 2, 3]);

        checkbox.triggerEventHandler('click', clickEventMock);

        expect(component.selectControl.value).toEqual(component.values);
    });

    it('should clear values when checkbox is checked on click', () => {
        const checkbox = fixture.debugElement.query(By.directive(MatCheckbox));
        const checkboxComponent = checkbox.componentInstance as MatCheckbox;
        checkboxComponent.checked = true;

        selectControl.setValue([1, 2, 3]);

        checkbox.triggerEventHandler('click', clickEventMock);

        expect(component.selectControl.value).toEqual([]);
    });
});
