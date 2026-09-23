import {AbstractControl, UntypedFormGroup, ValidatorFn, Validators} from '@angular/forms';
import {atLeastOneValidator} from '../../../shared/at-least-one.validator';

const ERROR_NAME = 'picture';
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
];
const AT_LEAST_ONE_ERROR_NAME = 'atLeastOne';

export function pictureValidator() {
    return (group: UntypedFormGroup) => {
        if (!group?.controls) {
            return null;
        }

        const imageControl = group.controls.image;
        const formControls = CONTROLS.map(c => group.get(c));

        if (imageControl.value) {
            const atLeastOne = atLeastOneValidator(Validators.required, [
                'descriptionDe',
                'descriptionFr',
                'descriptionEn',
                'descriptionIt',
                'descriptionRm',
            ]) as ValidatorFn;

            formControls.filter(c => c?.hasError(ERROR_NAME)).forEach(c => removeError(c!, ERROR_NAME));

            return atLeastOne(group);
        }

        let hasValue = false;
        formControls.forEach(control => {
            if (control?.value) {
                hasValue = true;
                const errors = control.errors ?? {};
                errors[ERROR_NAME] = true;

                control.setErrors(errors);
            } else if (control?.hasError(ERROR_NAME)) {
                removeError(control, ERROR_NAME);
            } else if (control?.hasError(AT_LEAST_ONE_ERROR_NAME)) {
                removeError(control, AT_LEAST_ONE_ERROR_NAME);
            }
        });

        if (hasValue) {
            group.markAllAsTouched();
            const errors: {[key: string]: boolean} = {};
            errors[ERROR_NAME] = true;

            return errors;
        }

        return null;
    };
}

function removeError(control: AbstractControl, error: string) {
    const errors = control.errors ?? {};
    delete errors[error];

    if (Object.keys(errors).length === 0) {
        control.setErrors(null);
    } else {
        control.setErrors(errors);
    }
}
