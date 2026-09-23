/* eslint-disable @typescript-eslint/no-unsafe-function-type */
import {AbstractControl} from '@angular/forms';
import {pictureValidator} from './picture.validator';

const CONTROLS = [
    'sourceDe',
    'descriptionDe',
    'sourceFr',
    'descriptionFr',
    'sourceEn',
    'descriptionEn',
    'sourceIt',
    'descriptionIt',
    'sourceRm',
    'descriptionRm',
    'image',
];
// eslint-disable-next-line @typescript-eslint/no-explicit-any
let formControls: {[key: string]: any} = {};

describe('pictureValidator', () => {
    let validator: Function;
    const group = {
        controls: {} as {[key: string]: AbstractControl},
        markAllAsTouched: jest.fn(),
        get: jest.fn(),
    };

    beforeEach(() => {
        validator = pictureValidator();

        formControls = {};
        CONTROLS.map(key => {
            formControls[key] = {
                hasError: jest.fn(),
                errors: {} as object | null,
                setErrors: jest.fn(),
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                value: null as any,
            };
            formControls[key].hasError.mockImplementation((errorKey: string) => formControls[key].errors && formControls[key].errors[errorKey] !== null);
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            formControls[key].setErrors.mockImplementation((errors: any) => (formControls[key].errors = errors));
        });

        formControls.image.value = true;
        formControls.descriptionDe.value = 'descriptionDe';

        group.controls = formControls;
        group.get.mockImplementation(key => formControls[key]);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('validates picture correctly when image has a value and at least one description entry', () => {
        const result = validator(group);
        expect(result).toBeNull();
    });

    it('adds error when image has a value and no description', () => {
        formControls.descriptionDe.value = null;

        const result = validator(group);
        expect(result).not.toBeNull();
        expect(result).toEqual({
            atLeastOne: true,
        });
    });

    it('removes picture error from controls when image value is set', () => {
        formControls.descriptionFr.errors = {picture: true};
        formControls.descriptionEn.errors = {picture: true, test: true};

        const result = validator(group);
        expect(result).toBeNull();

        expect(formControls.descriptionFr.errors).toBeNull();
        expect(formControls.descriptionEn.errors).toEqual({test: true});
    });

    it('adds picture error if image has no value but any other control has', () => {
        formControls.image.value = false;
        formControls.sourceRm.value = 'test';

        const result = validator(group);
        expect(result).not.toBeNull();
        expect(result).toEqual({
            picture: true,
        });
        expect(formControls.sourceRm.errors).toEqual({
            picture: true,
        });
        expect(formControls.descriptionDe.errors).toEqual({
            picture: true,
        });

        expect(group.markAllAsTouched).toHaveBeenCalledTimes(1);

        formControls.sourceRm.value = null;

        const result2 = validator(group);
        expect(result2).not.toBeNull();
        expect(result2).toEqual({
            picture: true,
        });
        expect(formControls.sourceRm.errors).toBeNull();
    });

    it('returns null when image has no value and any other control hasnt aswell', () => {
        formControls.image.value = false;
        formControls.descriptionDe.value = false;

        const result = validator(group);
        expect(result).toBeNull();
    });

    it('returns null when FormGroup or controls of FormGroup are not defined', () => {
        const result = validator({controls: null});
        expect(result).toBeNull();

        const result2 = validator(null);
        expect(result2).toBeNull();
    });
});
