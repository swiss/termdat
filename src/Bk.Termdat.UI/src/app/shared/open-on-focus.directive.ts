import {Directive, HostListener} from '@angular/core';
import {MatSelect} from '@angular/material/select';

@Directive({selector: '[tdOpenOnFocus]'})
export class OpenOnFocusDirective {
    constructor(private readonly select: MatSelect) {}

    @HostListener('focus')
    onFocus() {
        this.select.open();
    }
}
