import {Injectable} from '@angular/core';
import {MatDateFormats, NativeDateAdapter} from '@angular/material/core';

@Injectable({
    providedIn: 'root',
})
export class TdDateAdapter extends NativeDateAdapter {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    parse(value: any): Date | null {
        if (typeof value !== 'string') {
            return new Date('');
        }

        const splits = value.split('.').map(Number);

        if (splits.length !== 3) {
            return super.parse(value);
        }

        const [day, month, year] = splits;
        return new Date(year < 100 ? year + 2000 : year, month - 1, day);
    }

    format(date: Date, displayFormat: object): string {
        const pad = (n: number) => `00${n}`.slice(-2);
        // @ts-ignore
        return displayFormat === 'td-format' ? `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()}` : super.format(date, displayFormat);
    }

    getFirstDayOfWeek(): number {
        return 1;
    }
}

export const TD_DATE_FORMATS: MatDateFormats = {
    parse: {
        dateInput: 'dd.MM.yyyy',
    },
    display: {
        dateInput: 'td-format',
        monthYearLabel: {year: 'numeric', month: 'long'},
        dateA11yLabel: {year: 'numeric', month: 'long', day: 'numeric'},
        monthYearA11yLabel: {year: 'numeric', month: 'long'},
    },
};
