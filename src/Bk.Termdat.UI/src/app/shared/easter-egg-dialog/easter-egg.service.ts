import {Injectable, OnDestroy} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {ObGlobalEventsService} from '@oblique/oblique';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {EasterEggDialogComponent} from './easter-egg-dialog.component';

@Injectable({
    providedIn: 'root',
})
export class EasterEggService implements OnDestroy {
    private readonly expectedKeyEvents = ['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA'];
    private currentPosition = 0;

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly globalEventsService: ObGlobalEventsService,
        private readonly dialog: MatDialog
    ) {}

    init() {
        this.globalEventsService.keyUp$.pipe(takeUntil(this.unsubscribe)).subscribe(key => {
            if (key.code === this.expectedKeyEvents[this.currentPosition]) {
                this.currentPosition++;
                if (this.currentPosition >= this.expectedKeyEvents.length) {
                    this.dialog.open(EasterEggDialogComponent, {
                        disableClose: false,
                        width: 'auto',
                        minWidth: 'auto',
                    });
                    this.currentPosition = 0;
                }
            } else {
                this.currentPosition = 0;
            }
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }
}
