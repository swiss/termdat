import {MatSelect} from '@angular/material/select';
import {OpenOnFocusDirective} from './open-on-focus.directive';

describe('OpenOnFocusDirective', () => {
    let directive: OpenOnFocusDirective;
    const matSelectMock = {
        open: jest.fn(),
    };

    it('should create an instance', () => {
        directive = new OpenOnFocusDirective(matSelectMock as unknown as MatSelect);
        expect(directive).toBeTruthy();
    });

    it('should call open on onFocus', () => {
        directive.onFocus();

        expect(matSelectMock.open).toHaveBeenCalledTimes(1);
    });
});
