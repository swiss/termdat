import {Directive, ElementRef, HostListener} from '@angular/core';

@Directive({selector: '[tdFocusRemover]'})
export class FocusRemoverDirective {
    constructor(private readonly elRef: ElementRef) {}

    @HostListener('click') onClick() {
        this.elRef.nativeElement.blur();
    }
}
