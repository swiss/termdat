import {Component, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {MatCheckbox} from '@angular/material/checkbox';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';

@Component({
    selector: 'td-select-all',
    templateUrl: './select-all.component.html',
    styleUrls: ['./select-all.component.scss'],
    imports: [MatCheckbox],
})
export class SelectAllComponent implements OnInit, OnDestroy {
    @Input() selectControl!: AbstractControl;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    @Input() values: any[] = [];

    isChecked = false;
    isIndeterminate = false;

    @ViewChild(MatCheckbox) checkbox!: MatCheckbox;

    private readonly unsubscribe = new Subject<void>();

    ngOnInit(): void {
        this.calculateState();

        this.selectControl.valueChanges.pipe(takeUntil(this.unsubscribe)).subscribe(() => {
            this.calculateState();
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    toggleSelectionFromClick($event: MouseEvent) {
        $event.stopPropagation();
        $event.preventDefault();

        if (!this.checkbox.checked) {
            this.selectControl.setValue(this.values);
        } else {
            this.selectControl.setValue([]);
        }
    }

    private calculateState() {
        this.isChecked = !!(this.selectControl.value && this.values.length && this.selectControl.value.length === this.values.length);
        this.isIndeterminate = !!(
            this.selectControl.value &&
            this.values.length &&
            this.selectControl.value.length &&
            this.selectControl.value.length < this.values.length
        );
    }
}
