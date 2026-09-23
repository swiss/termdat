import {TestBed} from '@angular/core/testing';
import {MatDialog} from '@angular/material/dialog';
import {ObGlobalEventsService} from '@oblique/oblique';
import {Subject} from 'rxjs';
import {EasterEggDialogComponent} from './easter-egg-dialog.component';
import {EasterEggService} from './easter-egg.service';

describe('EasterEggService', () => {
    let service: EasterEggService;

    const dialogMock = {
        open: jest.fn(),
    };

    const keyUpSubject = new Subject<KeyboardEvent>();
    const globalEventsServiceMock = {
        keyUp$: keyUpSubject.asObservable(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                {
                    provide: MatDialog,
                    useValue: dialogMock,
                },
                {
                    provide: ObGlobalEventsService,
                    useValue: globalEventsServiceMock,
                },
            ],
        });
        service = TestBed.inject(EasterEggService);
        service.init();
    });

    afterEach(() => {
        service.ngOnDestroy();
        jest.resetAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(service['unsubscribe'], 'next');
        jest.spyOn(service['unsubscribe'], 'complete');
        service.ngOnDestroy();
        expect(service['unsubscribe'].next).toHaveBeenCalledTimes(1);
        expect(service['unsubscribe'].complete).toHaveBeenCalledTimes(1);
    });

    it.each([
        [['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA']],
        [['KeyZ', 'ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA']],
        [['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA', 'KeyZ']],
        [['KeyZ', 'ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA', 'KeyZ']],
    ])('should show easter egg with valid key sequence', keySequence => {
        for (const key of keySequence) {
            keyUpSubject.next({code: key} as KeyboardEvent);
        }

        expect(dialogMock.open).toHaveBeenCalledTimes(1);
        expect(dialogMock.open).toHaveBeenCalledWith(EasterEggDialogComponent, {
            disableClose: false,
            minWidth: 'auto',
            width: 'auto',
        });
    });

    it.each([
        [['ArrowUp', 'ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA']],
        [['ArrowUp', 'ArrowUp', 'KeyZ', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA']],
        [['ArrowUp', 'ArrowUp', 'ArrowDown', 'KeyZ', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA', 'KeyZ']],
        [['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'KeyZ', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA', 'KeyZ']],
        [['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'KeyZ', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA', 'KeyZ']],
    ])('should not show easter egg with valid key sequence', keySequence => {
        for (const key of keySequence) {
            keyUpSubject.next({code: key} as KeyboardEvent);
        }

        expect(dialogMock.open).toHaveBeenCalledTimes(0);
    });
});
