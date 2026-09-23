import {TestBed} from '@angular/core/testing';
import {DisplayMode} from '../../../shared/DisplayMode';
import {LOCAL_STORAGE} from '../../../shared/injection.tokens';
import {DisplayService} from './display.service';

describe('DisplayService', () => {
    let service: DisplayService;

    const localStorageMock = {
        getItem: jest.fn(),
        setItem: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [{provide: LOCAL_STORAGE, useValue: localStorageMock}],
        });
        service = TestBed.inject(DisplayService);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should set display mode to storage', () => {
        const testValue = DisplayMode.Minimal;
        service.setDisplayMode(testValue);

        expect(localStorageMock.setItem).toHaveBeenCalledTimes(1);
        expect(localStorageMock.setItem).toHaveBeenCalledWith('DisplayMode', testValue);
    });

    it('should get display mode from storage', () => {
        localStorageMock.getItem.mockReturnValueOnce('context');
        const result = service.getDisplayMode();

        expect(result).toBeTruthy();
        expect(localStorageMock.getItem).toHaveBeenCalledTimes(1);
        expect(result).toEqual(DisplayMode.Context);
    });

    it('should return a default value, when no value is stored', () => {
        localStorageMock.getItem.mockReturnValueOnce('');
        const result = service.getDisplayMode();

        expect(result).toBeTruthy();
        expect(localStorageMock.getItem).toHaveBeenCalledTimes(1);
        expect(result).toEqual(DisplayMode.Complete);
    });
});
