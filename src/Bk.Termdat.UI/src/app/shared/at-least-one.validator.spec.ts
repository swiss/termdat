/* eslint-disable @typescript-eslint/no-unsafe-function-type */
import {ValidatorFn} from '@angular/forms';
import {atLeastOneValidator} from './at-least-one.validator';

describe('atLeastOneValidator', () => {
    const validator = jest.fn();
    let atLeastOne: Function;
    const control1 = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
    };
    const control2 = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
    };
    const control3 = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
    };
    const control4 = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
    };
    const group = {
        controls: {
            control1,
            control2,
            control3,
            control4,
        },
    };

    beforeEach(() => {
        atLeastOne = atLeastOneValidator(validator as ValidatorFn, ['control2', 'control3']);
        control1.errors = {};
        control2.errors = {};
        control3.errors = {};
        control4.errors = {};
        group.controls = {
            control1,
            control2,
            control3,
            control4,
        };
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should return null when given controls validate successfully', () => {
        control2.hasError.mockReturnValueOnce(false);
        control3.hasError.mockReturnValueOnce(false);

        const result = atLeastOne(group);
        expect(result).toBeNull();

        expect(control1.hasError).toHaveBeenCalledTimes(0);
        expect(control4.hasError).toHaveBeenCalledTimes(0);

        expect(control2.hasError).toHaveBeenCalledTimes(1);
        expect(control2.hasError).toHaveBeenCalledWith('atLeastOne');

        expect(control3.hasError).toHaveBeenCalledTimes(1);
        expect(control3.hasError).toHaveBeenCalledWith('atLeastOne');

        expect(control1.setErrors).toHaveBeenCalledTimes(0);
        expect(control2.setErrors).toHaveBeenCalledTimes(0);
        expect(control3.setErrors).toHaveBeenCalledTimes(0);
        expect(control4.setErrors).toHaveBeenCalledTimes(0);
    });

    it('should clear atLeastOne errors in controls when validator returns true for at least one control', () => {
        control2.hasError.mockReturnValueOnce(true);
        control2.errors = {
            atLeastOne: true,
        };
        control3.hasError.mockReturnValueOnce(true);
        control3.errors = {
            test1: {},
            test2: {},
            atLeastOne: true,
        };

        const result = atLeastOne(group);
        expect(result).toBeNull();

        expect(control1.hasError).toHaveBeenCalledTimes(0);
        expect(control2.hasError).toHaveBeenCalledTimes(1);
        expect(control2.hasError).toHaveBeenCalledWith('atLeastOne');
        expect(control3.hasError).toHaveBeenCalledTimes(1);
        expect(control3.hasError).toHaveBeenCalledWith('atLeastOne');
        expect(control4.hasError).toHaveBeenCalledTimes(0);

        expect(control1.setErrors).toHaveBeenCalledTimes(0);
        expect(control2.setErrors).toHaveBeenCalledTimes(1);
        expect(control2.setErrors).toHaveBeenCalledWith(null);
        expect(control3.setErrors).toHaveBeenCalledTimes(1);
        expect(control3.setErrors).toHaveBeenCalledWith({test1: {}, test2: {}});
        expect(control4.setErrors).toHaveBeenCalledTimes(0);
    });

    it('returns null when FormGroup or controls of FormGroup are not defined', () => {
        const result = atLeastOne({controls: null});
        expect(result).toBeNull();

        const result2 = atLeastOne(null);
        expect(result2).toBeNull();
    });

    it('uses all controls when controls are not provided', () => {
        // @ts-ignore
        atLeastOne = atLeastOneValidator(validator as ValidatorFn, null);

        const result = atLeastOne(group);
        expect(result).toBeNull();

        expect(control1.hasError).toHaveBeenCalledTimes(1);
        expect(control1.hasError).toHaveBeenCalledWith('atLeastOne');
        expect(control2.hasError).toHaveBeenCalledTimes(1);
        expect(control2.hasError).toHaveBeenCalledWith('atLeastOne');
        expect(control3.hasError).toHaveBeenCalledTimes(1);
        expect(control3.hasError).toHaveBeenCalledWith('atLeastOne');
        expect(control4.hasError).toHaveBeenCalledTimes(1);
        expect(control4.hasError).toHaveBeenCalledWith('atLeastOne');
    });

    it('should return object when given controls validate unsuccessfully', () => {
        validator.mockReturnValue(true);
        control2.errors = {
            test1: {},
            test2: {},
        };

        const result = atLeastOne(group);
        expect(result).toEqual({atLeastOne: true});

        expect(control1.hasError).toHaveBeenCalledTimes(0);
        expect(control2.hasError).toHaveBeenCalledTimes(0);
        expect(control3.hasError).toHaveBeenCalledTimes(0);
        expect(control4.hasError).toHaveBeenCalledTimes(0);

        expect(control1.setErrors).toHaveBeenCalledTimes(0);
        expect(control2.setErrors).toHaveBeenCalledTimes(1);
        expect(control2.setErrors).toHaveBeenCalledWith({atLeastOne: true, test1: {}, test2: {}});
        expect(control3.setErrors).toHaveBeenCalledTimes(1);
        expect(control3.setErrors).toHaveBeenCalledWith({atLeastOne: true});
        expect(control4.setErrors).toHaveBeenCalledTimes(0);
    });

    it('handles errors not being defined on control when setError is called', () => {
        validator.mockReturnValue(true);
        control2.errors = null;

        const result = atLeastOne(group);
        expect(result).toEqual({atLeastOne: true});

        expect(control2.setErrors).toHaveBeenCalledTimes(1);
        expect(control2.setErrors).toHaveBeenCalledWith({atLeastOne: true});
    });

    it('can handle controls being defined that dont exist', () => {
        validator.mockReturnValue(true);
        atLeastOne = atLeastOneValidator(validator as ValidatorFn, ['controlNotExisting', 'control2', 'control3']);

        const result = atLeastOne(group);
        expect(result).toEqual({atLeastOne: true});
    });

    it('can handle controls being defined that dont exist when validation fails', () => {
        validator.mockReturnValue(false);
        atLeastOne = atLeastOneValidator(validator as ValidatorFn, ['controlNotExisting', 'control2', 'control3']);

        const result = atLeastOne(group);
        expect(result).toEqual(null);
    });
});
