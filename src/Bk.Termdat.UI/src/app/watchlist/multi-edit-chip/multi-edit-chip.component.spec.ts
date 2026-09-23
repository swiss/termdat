import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatInputModule} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {ChiplistAutocompleteComponent} from '../../shared/chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../shared/error.service';
import {MultiEditChipComponent} from './multi-edit-chip.component';

describe('MultiEditChipComponent', () => {
    let component: MultiEditChipComponent;
    let fixture: ComponentFixture<MultiEditChipComponent>;

    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            providers: [{provide: ErrorService, useValue: errorServiceMock}],
            imports: [
                ReactiveFormsModule,
                MockModule(MatInputModule),
                MultiEditChipComponent,
                MockComponent(ChiplistAutocompleteComponent),
                MockPipe(TranslatePipe),
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditChipComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should emit add', () => {
        jest.spyOn(component.addEvent, 'emit');
        component.form.controls.multiSelect.setValue('2');

        component.add();

        expect(component.addEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.addEvent.emit).toHaveBeenCalledWith('2');
    });

    it('should emit set', () => {
        jest.spyOn(component.setEvent, 'emit');
        component.form.controls.multiSelect.setValue('14');

        component.set();

        expect(component.setEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.setEvent.emit).toHaveBeenCalledWith('14');
    });

    it('should emit remove', () => {
        jest.spyOn(component.removeEvent, 'emit');
        component.form.controls.multiSelect.setValue('1');

        component.remove();

        expect(component.removeEvent.emit).toHaveBeenCalledTimes(1);
        expect(component.removeEvent.emit).toHaveBeenCalledWith('1');
    });
});
