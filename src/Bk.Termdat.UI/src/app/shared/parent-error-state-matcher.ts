import {AbstractControl, FormGroupDirective, NgForm, UntypedFormControl} from '@angular/forms';
import {ErrorStateMatcher} from '@angular/material/core';

export class ParentErrorStateMatcher implements ErrorStateMatcher {
    constructor(private readonly parentControl: AbstractControl) {}

    static create(control: AbstractControl) {
        return new ParentErrorStateMatcher(control);
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    isErrorState(control: UntypedFormControl | null, form: FormGroupDirective | NgForm | null): boolean {
        return this.parentControl.touched && this.parentControl.invalid;
    }
}
