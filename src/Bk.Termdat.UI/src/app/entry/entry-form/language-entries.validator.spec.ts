/* eslint-disable @typescript-eslint/no-unsafe-function-type */
import {FormArray, FormGroup} from '@angular/forms';
import {languageEntriesValidator} from './language-entries.validator';

describe('languageEntriesValidator', () => {
    let validator: Function;
    const formArray = {
        controls: [],
    } as unknown as FormArray;
    const formGroup1Entry1 = {};
    const formGroup1 = {
        controls: {
            entries: [formGroup1Entry1],
        },
    } as unknown as FormGroup;
    const formGroup1Entry2 = {};
    const formGroup2 = {
        controls: {
            entries: [formGroup1Entry2],
        },
    } as unknown as FormGroup;

    beforeEach(() => {
        formGroup1.controls.entries = [formGroup1Entry1] as unknown as FormArray;
        formGroup2.controls.entries = [formGroup1Entry2] as unknown as FormArray;
        formArray.controls = [formGroup1, formGroup2];

        validator = languageEntriesValidator(1);
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should return throw error when formArray does not contain expected form structure', () => {
        formArray.controls = [{} as unknown as FormGroup];
        expect(() => {
            validator(formArray);
        }).toThrow('Provided FormArray does not match expected structure');

        formArray.controls = [{controls: {}} as unknown as FormGroup];
        expect(() => {
            validator(formArray);
        }).toThrow('Provided FormArray does not match expected structure');

        formArray.controls = [{controls: {entries: null}} as unknown as FormGroup];
        expect(() => {
            validator(formArray);
        }).toThrow('Provided FormArray does not match expected structure');

        expect(() => {
            validator(null);
        }).toThrow('Provided FormArray does not match expected structure');

        expect(() => {
            validator({});
        }).toThrow('Provided FormArray does not match expected structure');
    });

    it('should return null when given source control validates successfully', () => {
        expect(validator(formArray)).toBeNull();

        formArray.controls = [formGroup1];
        expect(validator(formArray)).toBeNull();

        formGroup1.controls.entries = [] as unknown as FormArray;
        formGroup2.controls.entries = [] as unknown as FormArray;
        formArray.controls = [formGroup1, formGroup2];
        expect(validator(formArray)).not.toBeNull();
    });

    it('should default minimum required entries to 1', () => {
        formArray.controls = [formGroup1];
        const validator2 = languageEntriesValidator();

        expect(validator2(formArray)).toEqual(validator(formArray));
        formArray.controls = [];

        expect(validator2(formArray)).toEqual(validator(formArray));
        expect(validator2(formArray)).not.toBeNull();
    });

    it('should be able to configure minimum required entries', () => {
        formArray.controls = [formGroup1];
        expect(validator(formArray)).toBeNull();

        validator = languageEntriesValidator(2);
        expect(validator(formArray)).not.toBeNull();

        validator = languageEntriesValidator(3);
        expect(validator(formArray)).not.toBeNull();
    });

    it('should return error key if validation fails', () => {
        validator = languageEntriesValidator(3);
        expect(validator(formArray)).toEqual({
            language: true,
        });

        formArray.controls = [];
        expect(validator(formArray)).toEqual({
            language: true,
        });
    });
});
