import {LenochClassificationDetail} from 'api/LenochClassificationDetail';
import {sortLenochClassificationDetails} from './LenochClassificationSort';

describe('sortLenochClassificationDetails', () => {
    let data: LenochClassificationDetail[] = [];

    beforeEach(() => {
        data = [
            {
                id: 8006,
                code: 'YAD4',
                name: 'VERWALTUNGSPERSONAL',
                entryCount: 1676,
                subject: 'VERWALTUNG',
                domain: 'Personalwesen',
            },
            {
                id: 8007,
                code: 'AD5',
                name: 'ÖFFENTLICHE ARBEITEN',
                entryCount: 324,
                subject: 'VERWALTUNG',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'ÖFFENTLICHE GEWALT',
                entryCount: 1417,
                subject: 'NICHT ÖFFENTLICHE SICHERHEIT',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'ÖFFENTLICHE GEWALT',
                entryCount: 1417,
                subject: 'ÖFFENTLICHE SICHERHEIT',
                domain: 'Berufe',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'ÖFFENTLICHE GEWALT',
                entryCount: 1417,
                subject: 'VERWALTUNG',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'ÖFFENTLICHE SICHERHEIT',
                domain: 'Cybersicherheit',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'ÖFFENTLICHE SICHERHEIT',
                domain: 'Bevölkerungsschutz',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'ÖFFENTLICHE SICHERHEIT',
                domain: 'Zivildienst',
            },
            {
                id: 1,
                code: '',
                name: '',
                entryCount: 0,
                subject: '',
                domain: '',
            },
            {
                id: 2,
                code: '',
                name: '',
                entryCount: 0,
                subject: '',
            },
        ];
    });

    it('should sort details correctly', () => {
        data.sort((a, b) => sortLenochClassificationDetails(a, b, 'de'));

        expect(data).toEqual([
            {
                code: '',
                domain: '',
                entryCount: 0,
                id: 1,
                name: '',
                subject: '',
            },
            {
                code: '',
                entryCount: 0,
                id: 2,
                name: '',
                subject: '',
            },
            {
                code: 'AD5',
                entryCount: 324,
                id: 8007,
                name: 'ÖFFENTLICHE ARBEITEN',
                subject: 'VERWALTUNG',
            },
            {
                code: 'AD6',
                entryCount: 1417,
                id: 8008,
                name: 'ÖFFENTLICHE GEWALT',
                subject: 'NICHT ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'AD6',
                domain: 'Berufe',
                entryCount: 1417,
                id: 8008,
                name: 'ÖFFENTLICHE GEWALT',
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'AD6',
                entryCount: 1417,
                id: 8008,
                name: 'ÖFFENTLICHE GEWALT',
                subject: 'VERWALTUNG',
            },
            {
                code: 'AD7',
                entryCount: 3893,
                id: 8009,
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'AD7',
                domain: 'Bevölkerungsschutz',
                entryCount: 3893,
                id: 8009,
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'AD7',
                domain: 'Cybersicherheit',
                entryCount: 3893,
                id: 8009,
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'AD7',
                domain: 'Zivildienst',
                entryCount: 3893,
                id: 8009,
                name: 'ÖFFENTLICHE SICHERHEIT (auch: ME8)',
                subject: 'ÖFFENTLICHE SICHERHEIT',
            },
            {
                code: 'YAD4',
                domain: 'Personalwesen',
                entryCount: 1676,
                id: 8006,
                name: 'VERWALTUNGSPERSONAL',
                subject: 'VERWALTUNG',
            },
        ]);
    });
});
