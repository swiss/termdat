import {toCodeNameLabel} from './LabelFormat';

describe('Label', () => {
    it('should transform to code name string', () => {
        expect(toCodeNameLabel({code: 'CODE', name: 'Name'})).toEqual('CODE - Name');
    });

    it('should be able to handle transforms for null values', () => {
        expect(toCodeNameLabel(undefined)).toEqual('-');
    });
});
