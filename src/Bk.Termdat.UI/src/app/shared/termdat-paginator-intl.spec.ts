import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {of} from 'rxjs';
import {TermdatPaginatorIntl} from './termdat-paginator-intl';

describe('DpoPaginatorIntl', () => {
    let paginatorIntl: TermdatPaginatorIntl;

    const translateServiceMock = {
        stream: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [TermdatPaginatorIntl, {provide: TranslateService, useValue: translateServiceMock}],
        });

        translateServiceMock.stream.mockReturnValue(of([]));

        paginatorIntl = TestBed.inject(TermdatPaginatorIntl);
    });

    it('should be created', () => {
        expect(paginatorIntl).toBeTruthy();
    });
});
