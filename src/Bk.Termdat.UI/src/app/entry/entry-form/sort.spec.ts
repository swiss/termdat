import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {languageDetailCompare} from './sort';

describe('Sort', () => {
    it('compares EntryLanguageDetail correctly', () => {
        const languages = [
            {id: 1, languageId: 5, sequence: 3} as EntryLanguageDetail,
            {id: 2, languageId: 3, sequence: 2} as EntryLanguageDetail,
            {id: 3, languageId: 1, sequence: 5} as EntryLanguageDetail,
            {id: 4, languageId: 1, sequence: 2} as EntryLanguageDetail,
            {id: 5, languageId: 1, sequence: 1} as EntryLanguageDetail,
            {id: 6, languageId: 3, sequence: 100} as EntryLanguageDetail,
            {id: 7, languageId: 3, sequence: 1} as EntryLanguageDetail,
            {id: 7, languageId: 5, sequence: 4} as EntryLanguageDetail,
        ];

        expect(languages.sort(languageDetailCompare)).toEqual([
            {
                id: 5,
                languageId: 1,
                sequence: 1,
            },
            {
                id: 4,
                languageId: 1,
                sequence: 2,
            },
            {
                id: 3,
                languageId: 1,
                sequence: 5,
            },
            {
                id: 7,
                languageId: 3,
                sequence: 1,
            },
            {
                id: 2,
                languageId: 3,
                sequence: 2,
            },
            {
                id: 6,
                languageId: 3,
                sequence: 100,
            },
            {
                id: 1,
                languageId: 5,
                sequence: 3,
            },
            {
                id: 7,
                languageId: 5,
                sequence: 4,
            },
        ]);
    });
});
