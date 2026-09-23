import {formatDateTime} from './date-util';

describe('date-util', () => {
    it('should format date and time', () => {
        const date = new Date('2026-07-09T14:05:00Z');
        jest.spyOn(date, 'toLocaleDateString').mockReturnValue('09/07/2026');
        jest.spyOn(date, 'toLocaleTimeString').mockReturnValue('02:05 PM');

        expect(formatDateTime(date)).toBe('09/07/2026 02:05 PM');
    });
});
