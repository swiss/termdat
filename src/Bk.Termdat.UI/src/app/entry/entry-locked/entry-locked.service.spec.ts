import {RendererFactory2} from '@angular/core';
import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {EntryLockedService} from './entry-locked.service';

describe('EntryLockedService', () => {
    let service: EntryLockedService;

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    let register: any[] = [];
    const rendererFactoryMock = {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        createRenderer: (_: any) => rendererMock,
    };
    const rendererMock = {
        listen: jest.fn(),
    };

    const translateServiceMock = {
        instant: jest.fn(),
    };

    beforeEach(() => {
        rendererMock.listen.mockImplementation((target: string, eventName: string, callBack: () => void) => {
            register.push([`${target}:${eventName}`, callBack]);
        });
        translateServiceMock.instant.mockImplementation(key => key);

        TestBed.configureTestingModule({
            providers: [
                {provide: RendererFactory2, useValue: rendererFactoryMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        });
        service = TestBed.inject(EntryLockedService);
    });

    afterEach(() => {
        jest.clearAllMocks();
        register = [];
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should run canUnload on watched components when beforeunload() is triggered', () => {
        const beforeunloadHandler = register[0];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');

        const fakeComponent = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };

        service.watch('fake', fakeComponent);

        const event = {};

        beforeunloadHandler[1](event);

        expect(fakeComponent.canLockedUnload).toHaveBeenCalledTimes(1);
    });

    it('should run canUnload on watched components when unload() is triggered', () => {
        const unloadHandler = register[1];
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };

        service.watch('fake', fakeComponent);

        const event = {};

        unloadHandler[1](event);

        expect(fakeComponent.canLockedUnload).toHaveBeenCalledTimes(1);
    });

    it('should add returnValue to event to trigger browser dialog when canUnload returns false on any component', () => {
        const beforeunloadHandler = register[0];
        const unloadHandler = register[1];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent1 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent1.canLockedUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent2.canLockedUnload.mockReturnValue(false);
        service.watch('fake1', fakeComponent1);
        service.watch('fake2', fakeComponent2);

        const event1 = {};
        const return1 = beforeunloadHandler[1](event1);
        expect(return1).toEqual('entry.locked.quitMessage');

        // @ts-ignore
        expect(event1.returnValue).toEqual('entry.locked.quitMessage');

        const event2 = {};
        const return2 = unloadHandler[1](event2);
        expect(return2).toEqual('entry.locked.quitMessage');

        // @ts-ignore
        expect(event2.returnValue).toEqual('entry.locked.quitMessage');
    });

    it('should not add returnValue to event when canUnload returns true on all components', () => {
        const beforeunloadHandler = register[0];
        const unloadHandler = register[1];
        expect(beforeunloadHandler[0]).toEqual('window:beforeunload');
        expect(unloadHandler[0]).toEqual('window:unload');

        const fakeComponent1 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent1.canLockedUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent2.canLockedUnload.mockReturnValue(true);
        service.watch('fake1', fakeComponent1);
        service.watch('fake2', fakeComponent2);

        const event = {};
        const return1 = beforeunloadHandler[1](event);
        expect(return1).toBeNull();

        expect(fakeComponent1.canLockedUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canLockedUnload).toHaveBeenCalledTimes(1);

        // @ts-ignore
        expect(event.returnValue).toBeUndefined();

        const event2 = {};
        const return2 = unloadHandler[1](event2);
        expect(return2).toBeNull();

        expect(fakeComponent1.canLockedUnload).toHaveBeenCalledTimes(2);
        expect(fakeComponent2.canLockedUnload).toHaveBeenCalledTimes(2);

        // @ts-ignore
        expect(event2.returnValue).toBeUndefined();
    });

    it('should not call canUnload on unwatched components', () => {
        const fakeComponent1 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent1.canLockedUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent2.canLockedUnload.mockReturnValue(true);
        service.watch('fake1', fakeComponent1);
        service.watch('fake2', fakeComponent2);

        service.hasLockedEntries();

        expect(fakeComponent1.canLockedUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canLockedUnload).toHaveBeenCalledTimes(1);

        service.unwatch('fake1');
        service.hasLockedEntries();

        expect(fakeComponent1.canLockedUnload).toHaveBeenCalledTimes(1);
        expect(fakeComponent2.canLockedUnload).toHaveBeenCalledTimes(2);

        service.watch('fake1', fakeComponent1);
        service.unwatch('fake2');
        service.hasLockedEntries();

        expect(fakeComponent1.canLockedUnload).toHaveBeenCalledTimes(2);
        expect(fakeComponent2.canLockedUnload).toHaveBeenCalledTimes(2);
    });

    it('should check for locked entries', () => {
        const fakeComponent1 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent1.canLockedUnload.mockReturnValue(true);
        const fakeComponent2 = {
            canLockedUnload: jest.fn(),
            canLockedDeactivate: jest.fn(),
        };
        fakeComponent2.canLockedUnload.mockReturnValue(false);
        service.watch('fake1', fakeComponent1);
        service.watch('fake2', fakeComponent2);

        expect(service.hasLockedEntries()).toEqual(true);

        fakeComponent2.canLockedUnload.mockReturnValue(true);

        expect(service.hasLockedEntries()).toEqual(false);
    });
});
