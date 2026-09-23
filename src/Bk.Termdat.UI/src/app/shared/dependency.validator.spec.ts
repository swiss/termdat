/* eslint-disable @typescript-eslint/no-unsafe-function-type */
import {FormControl, ValidatorFn} from '@angular/forms';
import {dependencyValidator} from './dependency.validator';

describe('dependencyValidator', () => {
    const validator = jest.fn();
    let dependency: Function;
    const sourceControl = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
        value: 'test',
    };
    const requiredControl = {
        hasError: jest.fn(),
        errors: {} as object | null,
        setErrors: jest.fn(),
    };

    beforeEach(() => {
        sourceControl.errors = {};
        sourceControl.value = 'test';

        requiredControl.errors = {};

        validator.mockReturnValue(null);

        dependency = dependencyValidator(validator as ValidatorFn, sourceControl as unknown as FormControl, requiredControl as unknown as FormControl);
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should return null when sourceControl has no value', () => {
        sourceControl.value = '';

        dependency = dependencyValidator(validator as ValidatorFn, sourceControl as unknown as FormControl, requiredControl as unknown as FormControl);

        expect(dependency()).toBeNull();
    });

    it('should return null when given source control validates successfully', () => {
        expect(dependency()).toBeNull();
    });

    it('should clear dependency error in control when given source control validates successfully and has dependency error', () => {
        sourceControl.hasError.mockReturnValueOnce(true);
        sourceControl.errors = {
            'dependency.mockConstructor': true,
            otherError: true,
            anotherError: {},
        };

        const result = dependency();
        expect(result).toBeNull();
        expect(validator).toHaveBeenCalledTimes(1);
        expect(validator).toHaveBeenCalledWith(requiredControl);

        expect(sourceControl.hasError).toHaveBeenCalledTimes(1);
        expect(sourceControl.hasError).toHaveBeenCalledWith('dependency.mockConstructor');
        expect(sourceControl.setErrors).toHaveBeenCalledWith({otherError: true, anotherError: {}});
    });

    it('should only clear dependency error from source control', () => {
        sourceControl.hasError.mockReturnValue(true);
        sourceControl.errors = {
            'dependency.mockConstructor': true,
            otherError: true,
            anotherError: {},
        };

        dependency();
        expect(sourceControl.setErrors).toHaveBeenLastCalledWith({otherError: true, anotherError: {}});

        sourceControl.errors = {'dependency.mockConstructor': true};

        dependency();
        expect(sourceControl.setErrors).toHaveBeenLastCalledWith(null);
    });

    it('should set dependency error when validator fails for sourceControl and return dependency error', () => {
        validator.mockReturnValue({test: true});
        sourceControl.errors = {
            otherError: true,
            anotherError: {},
        };

        const result = dependency();
        expect(result).toEqual({
            'dependency.mockConstructor': true,
        });
        expect(validator).toHaveBeenCalledTimes(1);
        expect(validator).toHaveBeenCalledWith(requiredControl);

        expect(sourceControl.setErrors).toHaveBeenCalledWith({otherError: true, anotherError: {}, 'dependency.mockConstructor': true});
    });
});
