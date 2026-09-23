import {AbstractControl, UntypedFormGroup, ValidatorFn} from '@angular/forms';

const ERROR_NAME = 'atLeastOne';

export function atLeastOneValidator(validator: ValidatorFn, controls: string[]) {
    return (group: UntypedFormGroup) => {
        if (!group?.controls) {
            return null;
        }

        if (!controls || controls.length === 0) {
            controls = Object.keys(group.controls);
        }

        const existingControls = controls.filter(con => !!group.controls[con]);
        const abstractControls = existingControls.map(con => group.controls[con]);
        const hasAtLeastOne = existingControls.some(con => !validator(group.controls[con]));

        if (hasAtLeastOne) {
            abstractControls.forEach((control: AbstractControl) => {
                if (!control.hasError(ERROR_NAME)) {
                    return;
                }

                delete control.errors![ERROR_NAME];

                const errors = Object.keys(control.errors!).length > 0 ? control.errors : null;
                control.setErrors(errors);
            });

            return null;
        }

        let hasBeenTouched = false;
        abstractControls.forEach((control: AbstractControl) => {
            const controlErrors = control.errors ?? {};
            controlErrors[ERROR_NAME] = true;
            control.setErrors(controlErrors);
            hasBeenTouched = control.touched || hasBeenTouched;
        });

        if (hasBeenTouched) {
            // If one has been touched, all must be marked as touched so the error is visible for all fields
            abstractControls.forEach((control: AbstractControl) => control.markAsTouched());
        }

        const errors: {[key: string]: boolean} = {};
        errors[ERROR_NAME] = true;

        return errors;
    };
}
