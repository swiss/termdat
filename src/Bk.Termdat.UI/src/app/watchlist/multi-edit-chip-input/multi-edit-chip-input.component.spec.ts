import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatInputModule} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {Chip} from '../../shared/chiplist-autocomplete/Chip';
import {ChiplistAutocompleteInputComponent} from '../../shared/chiplist-autocomplete-input/chiplist-autocomplete-input.component';
import {ErrorService} from '../../shared/error.service';
import {MultiEditChipInputComponent} from './multi-edit-chip-input.component';

describe('MultiEditChipInputComponent', () => {
    let component: MultiEditChipInputComponent;
    let fixture: ComponentFixture<MultiEditChipInputComponent>;

    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            providers: [{provide: ErrorService, useValue: errorServiceMock}],
            imports: [
                ReactiveFormsModule,
                MockModule(MatInputModule),
                MultiEditChipInputComponent,
                MockComponent(ChiplistAutocompleteInputComponent),
                MockPipe(TranslatePipe),
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditChipInputComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should emit add', () => {
        jest.spyOn(component.addEvent, 'emit');
        component.form.controls.multiSelect.setValue([{value: 'test', displayText: 'test', deleted: false, tag: 'test'} as Chip]);

        component.add();

        expect(component.addEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.addEvent.emit).toHaveBeenCalledWith([{code: 'test', id: 'test'}]);
    });

    it('should emit set', () => {
        jest.spyOn(component.setEvent, 'emit');
        component.form.controls.multiSelect.setValue([{value: 'test2', displayText: 'test2', deleted: false, tag: 'test2'} as Chip]);

        component.set();

        expect(component.setEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.setEvent.emit).toHaveBeenCalledWith([{code: 'test2', id: 'test2'}]);
    });

    it('should emit remove', () => {
        jest.spyOn(component.removeEvent, 'emit');
        component.form.controls.multiSelect.setValue([{value: 'test3', displayText: 'test3', deleted: false, tag: 'test3'} as Chip]);

        component.remove();

        expect(component.removeEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.removeEvent.emit).toHaveBeenCalledWith();
    });
});
