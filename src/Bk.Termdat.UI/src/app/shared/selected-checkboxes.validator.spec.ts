/* eslint-disable @typescript-eslint/no-unsafe-function-type */
import {FormControl} from '@angular/forms';
import {selectedCheckboxesValidator} from './selected-checkboxes.validator';

describe('selectedCheckboxesValidator', () => {
    const control1 = {
        value: true,
    };
    const control2 = {
        value: true,
    };
    const control3 = {
        value: true,
    };
    const formArray = {
        controls: [] as FormControl[],
    };
    let selectedCheckboxes: Function;

    beforeEach(() => {
        control1.value = true;
        control2.value = true;
        control3.value = true;

        formArray.controls = [control1 as FormControl, control2 as FormControl, control3 as FormControl];

        selectedCheckboxes = selectedCheckboxesValidator();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should return null when at least one control has value true', () => {
        expect(selectedCheckboxes(formArray)).toBeNull();

        control1.value = false;
        control2.value = false;

        expect(selectedCheckboxes(formArray)).toBeNull();
    });

    it('should return error when no control has value true', () => {
        control1.value = false;
        control2.value = false;
        control3.value = false;

        expect(selectedCheckboxes(formArray)).toEqual({required: true});
    });

    it('can configure the minimum required controls', () => {
        selectedCheckboxes = selectedCheckboxesValidator(3);

        expect(selectedCheckboxes(formArray)).toBeNull();

        control3.value = false;

        expect(selectedCheckboxes(formArray)).toEqual({required: true});
    });
});
