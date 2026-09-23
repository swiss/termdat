import {FocusRemoverDirective} from './focus-remover.directive';

describe('FocusRemoverDirective', () => {
    const elementRefMock = {
        nativeElement: jest.fn(),
    };

    it('should create an instance', () => {
        const directive = new FocusRemoverDirective(elementRefMock);
        expect(directive).toBeTruthy();
    });
});
