import {UntypedFormControl, ValidatorFn} from '@angular/forms';

const ERROR_NAME = 'dependency';

export function dependencyValidator(validator: ValidatorFn, sourceControl: UntypedFormControl, requiredControl: UntypedFormControl) {
    return () => {
        if (!sourceControl.value) {
            return null;
        }

        const key = validator.name ? `${ERROR_NAME}.${validator.name}` : ERROR_NAME;

        if (validator(requiredControl) === null) {
            if (sourceControl.hasError(key)) {
                delete sourceControl.errors![key];

                const controlErrors = Object.keys(sourceControl.errors!).length > 0 ? sourceControl.errors : null;
                sourceControl.setErrors(controlErrors);
            }

            return null;
        }

        const sourceErrors = sourceControl.errors ?? {};
        sourceErrors[key] = true;
        sourceControl.setErrors(sourceErrors);

        const errors: {[key: string]: boolean} = {};
        errors[key] = true;

        return errors;
    };
}
