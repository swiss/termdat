import {FormControl} from '@angular/forms';
import {ParentErrorStateMatcher} from './parent-error-state-matcher';

describe('ParentErrorStateMatcher', () => {
    let parentErrorStateMatcher: ParentErrorStateMatcher;

    const formControl = new FormControl(null, c => (c.value === 'valid' ? null : {error: 'Error'}));

    beforeEach(() => {
        parentErrorStateMatcher = ParentErrorStateMatcher.create(formControl);
    });

    it('should create', () => {
        expect(parentErrorStateMatcher).toBeTruthy();
        expect(parentErrorStateMatcher.isErrorState(null, null)).toBeFalsy();
    });

    it('should return error on invalid and touched form control', () => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formControl.setValue('invalid' as any);
        formControl.markAsTouched();

        expect(parentErrorStateMatcher.isErrorState(null, null)).toBeTruthy();
    });

    it('should not return error on valid form control', () => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formControl.setValue('valid' as any);
        formControl.markAsTouched();

        expect(parentErrorStateMatcher.isErrorState(null, null)).toBeFalsy();
    });

    it('should not return error on untouched form control', () => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formControl.setValue('invalid' as any);
        formControl.markAsUntouched();

        expect(parentErrorStateMatcher.isErrorState(null, null)).toBeFalsy();
    });
});
