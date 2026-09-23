import {UntypedFormArray} from '@angular/forms';

export function selectedCheckboxesValidator(min = 1) {
    return (formArray: UntypedFormArray) => {
        const totalSelected = formArray.controls.map(control => !!control.value).reduce((prev, next) => (next ? ++prev : prev), 0);

        return totalSelected >= min ? null : {required: true};
    };
}
