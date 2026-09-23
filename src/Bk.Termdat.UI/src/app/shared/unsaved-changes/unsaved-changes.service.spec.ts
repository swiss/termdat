import {RendererFactory2} from '@angular/core';
import {TestBed} from '@angular/core/testing';
import {AbstractControl} from '@angular/forms';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import {TranslateService} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {of} from 'rxjs';
import {finalize} from 'rxjs/operators';
import {UnsavedChangeAction} from './unsaved-change-action';
import {UnsavedChangesComponent} from './unsaved-changes.component';
import {UnsavedChangesService} from './unsaved-changes.service';

describe('UnsavedChangesService', () => {
    let service: UnsavedChangesService;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    let register: any[] = [];
    const rendererFactoryMock = {
        createRenderer: () => rendererMock,
    };
    const rendererMock = {
        listen: jest.fn(),
    };

    const dialogMock = {
        open: jest.fn(),
    };
    const openDialogMock = {
        afterClosed: jest.fn(),
    };

    const translateServiceMock = {
        instant: jest.fn(),
    };

    const saveMock = jest.fn();
    const discardMock = jest.fn();

    const formMock = {
        valueChanges: of({}),
    } as unknown as AbstractControl;

    beforeEach(() => {
        dialogMock.open.mockReturnValue(openDialogMock);
        rendererMock.listen.mockImplementation((target: string, eventName: string, callBack: () => void) => {
            register.push([`${target}:${eventName}`, callBack]);
        });

        TestBed.configureTestingModule({
            imports: [MockModule(MatDialogModule)],
            providers: [
                {provide: RendererFactory2, useValue: rendererFactoryMock},
                {provide: MatDialog, useValue: dialogMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        });
        service = TestBed.inject(UnsavedChangesService);

        saveMock.mockReturnValue(of(true));
    });

    afterEach(() => {
        jest.clearAllMocks();
        register = [];
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should bind unload events on create', () => {
        expect(rendererMock.listen).toHaveBeenCalledTimes(2);
        expect(rendererMock.listen).toHaveBeenCalledWith('window', 'beforeunload', expect.any(Function));
        expect(rendererMock.listen).toHaveBeenCalledWith('window', 'unload', expect.any(Function));
        expect(register.length).toEqual(2);
    });

    it('should run canUnload on watched components when beforeunload() is triggered', () => {
        const beforeunloadHandler = register[0];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');

        const fakeComponent = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };

        service.watch('fake', fakeComponent, formMock);

        const event = {};

        beforeunloadHandler[1](event);

        expect(fakeComponent.canUnload).toHaveBeenCalledTimes(1);
    });

    it('should run canUnload on watched components when unload() is triggered', () => {
        const unloadHandler = register[1];
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };

        service.watch('fake', fakeComponent, formMock);

        const event = {};

        unloadHandler[1](event);

        expect(fakeComponent.canUnload).toHaveBeenCalledTimes(1);
    });

    it('should add returnValue to event to trigger browser dialog when canUnload returns false on any component', () => {
        const beforeunloadHandler = register[0];
        const unloadHandler = register[1];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent1 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent1.canUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent2.canUnload.mockReturnValue(false);
        service.watch('fake1', fakeComponent1, formMock);
        service.watch('fake2', fakeComponent2, formMock);
        translateServiceMock.instant.mockReturnValue('i18n.validation.unsavedChanges');

        const event1 = {};
        const return1 = beforeunloadHandler[1](event1);
        expect(return1).toEqual('i18n.validation.unsavedChanges');

        // @ts-ignore
        expect(event1.returnValue).toEqual('i18n.validation.unsavedChanges');

        const event2 = {};
        const return2 = unloadHandler[1](event2);
        expect(return2).toEqual('i18n.validation.unsavedChanges');

        // @ts-ignore
        expect(event2.returnValue).toEqual('i18n.validation.unsavedChanges');
    });

    it('should not add returnValue to event when canUnload returns true on all components', () => {
        const beforeunloadHandler = register[0];
        const unloadHandler = register[1];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent1 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent1.canUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent2.canUnload.mockReturnValue(true);
        service.watch('fake1', fakeComponent1, formMock);
        service.watch('fake2', fakeComponent2, formMock);

        const event = {};
        const return1 = beforeunloadHandler[1](event);
        expect(return1).toBeNull();

        expect(fakeComponent1.canUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canUnload).toHaveBeenCalledTimes(1);

        // @ts-ignore
        expect(event.returnValue).toBeUndefined();

        const event2 = {};
        const return2 = unloadHandler[1](event2);
        expect(return2).toBeNull();

        expect(fakeComponent1.canUnload).toHaveBeenCalledTimes(2);
        expect(fakeComponent2.canUnload).toHaveBeenCalledTimes(2);

        // @ts-ignore
        expect(event2.returnValue).toBeUndefined();
    });

    it('should not call canUnload on unwatched components', () => {
        const fakeComponent1 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent1.canUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent2.canUnload.mockReturnValue(true);
        service.watch('fake1', fakeComponent1, formMock);
        service.watch('fake2', fakeComponent2, formMock);

        service.hasUnconfirmedChanges();

        expect(fakeComponent1.canUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canUnload).toHaveBeenCalledTimes(1);

        service.unwatch('fake1');
        service.hasUnconfirmedChanges();

        expect(fakeComponent1.canUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canUnload).toHaveBeenCalledTimes(2);

        service.watch('fake1', fakeComponent1, formMock);
        service.unwatch('fake2');
        service.hasUnconfirmedChanges();

        expect(fakeComponent1.canUnload).toHaveBeenCalledTimes(2);
        expect(fakeComponent2.canUnload).toHaveBeenCalledTimes(2);
    });

    it('should not calculated hasUnsavedChanges for components', () => {
        const fakeComponent1 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent1.canUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canUnload: jest.fn(),
            canDeactivate: jest.fn(),
        };
        fakeComponent2.canUnload.mockReturnValue(false);

        expect(service.hasUnconfirmedChanges()).toBeFalsy();
        service.watch('fake1', fakeComponent1, formMock);
        service.watch('fake2', fakeComponent2, formMock);

        expect(service.hasUnconfirmedChanges()).toBeTruthy();

        service.unwatch('fake1');
        expect(service.hasUnconfirmedChanges()).toBeTruthy();

        service.unwatch('fake2');
        expect(service.hasUnconfirmedChanges()).toBeFalsy();
    });

    it('should open dialog on createDeactivateConfirmDialog and call the save observable when dialog is confirmed with save', done => {
        const afterClosedObservable = of(UnsavedChangeAction.SAVE);
        openDialogMock.afterClosed.mockReturnValue(afterClosedObservable);

        const observable = service.createDeactivateConfirmDialog(saveMock);
        observable.pipe(finalize(() => done())).subscribe(value => {
            expect(value).toEqual(true);
            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(UnsavedChangesComponent);
            expect(openDialogMock.afterClosed).toHaveBeenCalledTimes(1);
            expect(saveMock).toHaveBeenCalled();
            expect(discardMock).not.toHaveBeenCalled();
        });
    });

    it('should open dialog on createDeactivateConfirmDialog and not call save when discard is returned on dialog close', done => {
        const afterClosedObservable = of(UnsavedChangeAction.DISCARD);
        openDialogMock.afterClosed.mockReturnValue(afterClosedObservable);

        const observable = service.createDeactivateConfirmDialog(saveMock, discardMock);
        observable.pipe(finalize(() => done())).subscribe(value => {
            expect(value).toEqual(true);
            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(UnsavedChangesComponent);
            expect(openDialogMock.afterClosed).toHaveBeenCalledTimes(1);
            expect(saveMock).not.toHaveBeenCalled();
            expect(discardMock).toHaveBeenCalled();
        });
    });

    it('should open dialog on createDeactivateConfirmDialog and return false when afterClose is not DISCARD or SAVE', done => {
        const afterClosedObservable = of(undefined);
        openDialogMock.afterClosed.mockReturnValue(afterClosedObservable);

        const observable = service.createDeactivateConfirmDialog(saveMock, discardMock);
        observable.pipe(finalize(() => done())).subscribe(value => {
            expect(value).toEqual(false);
            expect(dialogMock.open).toHaveBeenCalledTimes(1);
            expect(dialogMock.open).toHaveBeenCalledWith(UnsavedChangesComponent);
            expect(openDialogMock.afterClosed).toHaveBeenCalledTimes(1);
            expect(saveMock).not.toHaveBeenCalled();
            expect(discardMock).not.toHaveBeenCalled();
        });
    });
});
