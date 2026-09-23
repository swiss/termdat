import {TestBed} from '@angular/core/testing';
import {MAT_DATE_LOCALE, NativeDateAdapter} from '@angular/material/core';
import {TdDateAdapter} from './td-date-adapter';

describe('TdDateAdapter', () => {
    let dateAdapter: TdDateAdapter;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                {provide: MAT_DATE_LOCALE, useValue: 'de'},
                {provide: TdDateAdapter, useClass: TdDateAdapter},
                {provide: NativeDateAdapter, useClass: NativeDateAdapter},
            ],
        });
        dateAdapter = TestBed.inject(TdDateAdapter);
    });

    it('should create an instance', () => {
        expect(dateAdapter).toBeTruthy();
    });

    it('should parse dd.MM.yyyy', () => {
        const value = '01.01.20';
        const date = dateAdapter.parse(value) ?? fail();

        expect(dateAdapter.isValid(date)).toBe(true);
    });

    it('should not mix up day and month', () => {
        const value = '11.01.2020';
        const date = dateAdapter.parse(value);

        expect(date?.toString()).toContain('Jan');

        const value2 = '01.11.2020';
        const date2 = dateAdapter.parse(value2);

        expect(date2?.toString()).toContain('Nov');
    });

    it('should start weeks with monday', () => {
        expect(dateAdapter.getFirstDayOfWeek()).toBe(1);
    });

    it('should format eb-input dd.MM.yyyy', () => {
        const date: Date = new Date(2020, 0, 1);
        const displayFormat = 'td-format';

        // @ts-ignore
        expect(dateAdapter.format(date, displayFormat)).toEqual('01.01.2020');
    });

    it('should parse long month and year', () => {
        const value = 'Sep 2030';
        const date = dateAdapter.parse(value) ?? fail();

        expect(dateAdapter.isValid(date)).toBe(true);
    });
});
