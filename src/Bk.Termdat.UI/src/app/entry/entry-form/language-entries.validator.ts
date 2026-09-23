import {UntypedFormArray, UntypedFormGroup} from '@angular/forms';

export function languageEntriesValidator(min = 1) {
    return (formArray: UntypedFormArray) => {
        const controls = formArray?.controls as UntypedFormGroup[];

        if (!controls?.every((group: UntypedFormGroup) => !!group.controls?.entries)) {
            throw new Error('Provided FormArray does not match expected structure');
        }

        const languagesWithEntries = controls
            .map((group: UntypedFormGroup) => {
                const entries = group.controls.entries as UntypedFormArray;

                return entries.length > 0;
            })
            .reduce((prev, next) => (next ? ++prev : prev), 0);

        return languagesWithEntries >= min ? null : {language: true};
    };
}
