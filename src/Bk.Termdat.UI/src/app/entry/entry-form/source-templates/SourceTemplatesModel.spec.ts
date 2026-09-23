import {SourceTemplateDefaults} from './SourceTemplateDefaults';
import {SourceTemplatesModel} from './SourceTemplatesModel';
import {SourceTemplateType} from './SourceTemplateType';

describe('SourceTemplatesModel', () => {
    let model: SourceTemplatesModel;
    let templates: SourceTemplateDefaults;

    beforeEach(() => {
        templates = {
            de: {
                federalLawSr: 'Titel, Art.  Abs.  Bst.  Ziff.  Anh.  N.  (SR XYZ, Stand JJJJ-MM)',
                federalLawBbl: 'Titel (BBl)',
                federalLawAs: 'Titel (AS)',
                federalCourt: 'Bundesgerichtsentscheide, Titel ()',
                federalCouncilMessage: 'BoBR Titel (BBL)',
                internet: 'Titel [Internet, {{ date }}] (LINK)',
            },
            fr: {
                federalLawSr: 'test',
            },
        } as unknown as SourceTemplateDefaults;
        model = new SourceTemplatesModel(templates);

        jest.spyOn(global.Date, 'now').mockImplementationOnce(() => new Date('2021-11-30T15:01:58.135Z').valueOf());
    });

    it('can get federal law SR template', () => {
        expect(model.getFederalLawSrTemplate('de')).toEqual('Titel, Art.  Abs.  Bst.  Ziff.  Anh.  N.  (SR XYZ, Stand JJJJ-MM)');
    });

    it('can get federal law BBl template', () => {
        expect(model.getFederalLawBblTemplate('de')).toEqual('Titel (BBl)');
    });

    it('can get federal law AS template', () => {
        expect(model.getFederalLawAsTemplate('de')).toEqual('Titel (AS)');
    });

    it('can get federal court template', () => {
        expect(model.getFederalCourtTemplate('de')).toEqual('Bundesgerichtsentscheide, Titel ()');
    });

    it('can get bundesrat message template', () => {
        expect(model.getFederalCouncilMessageTemplate('de')).toEqual('BoBR Titel (BBL)');
    });

    it('can get internet template', () => {
        expect(model.getInternetTemplate('de')).toEqual('Titel [Internet, 2021-11-30] (LINK)');
    });

    it('formats date in internet template in german format for other languages', () => {
        model = new SourceTemplatesModel({
            de: {internet: '{{date}}'},
            fr: {internet: '{{ date }}'},
            it: {internet: '{{  date}}'},
            rm: {internet: '{{date  }}'},
            en: {internet: '{{date}}'},
        });

        expect(model.getInternetTemplate('de')).toEqual('2021-11-30');
        expect(model.getInternetTemplate('fr')).toEqual('2021-11-30');
        expect(model.getInternetTemplate('it')).toEqual('2021-11-30');
        expect(model.getInternetTemplate('rm')).toEqual('2021-11-30');
        expect(model.getInternetTemplate('en')).toEqual('2021-11-30');
    });

    it('can get other language', () => {
        expect(model.getFederalLawSrTemplate('fr')).toEqual('test');
    });

    it('returns empty string for undefined keys', () => {
        expect(model.getFederalLawSrTemplate('it')).toEqual('');
    });

    it('can handle source templates being undefined', () => {
        model = new SourceTemplatesModel(undefined as unknown as SourceTemplateDefaults);

        expect(model.getFederalLawSrTemplate('it')).toEqual('');
    });

    it.each([
        {
            type: SourceTemplateType.FederalLawSr,
            typeName: SourceTemplateType[SourceTemplateType.FederalLawSr],
            expected: 'Titel, Art.  Abs.  Bst.  Ziff.  Anh.  N.  (SR XYZ, Stand JJJJ-MM)',
        },
        {type: SourceTemplateType.FederalLawBbl, typeName: SourceTemplateType[SourceTemplateType.FederalLawBbl], expected: 'Titel (BBl)'},
        {type: SourceTemplateType.FederalLawAs, typeName: SourceTemplateType[SourceTemplateType.FederalLawAs], expected: 'Titel (AS)'},
        {
            type: SourceTemplateType.FederalCourt,
            typeName: SourceTemplateType[SourceTemplateType.FederalCourt],
            expected: 'Bundesgerichtsentscheide, Titel ()',
        },
        {
            type: SourceTemplateType.FederalCouncilMessage,
            typeName: SourceTemplateType[SourceTemplateType.FederalCouncilMessage],
            expected: 'BoBR Titel (BBL)',
        },
        {
            type: SourceTemplateType.Internet,
            typeName: SourceTemplateType[SourceTemplateType.Internet],
            expected: 'Titel [Internet, 2021-11-30] (LINK)',
        },
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ])('can get template by type $typeName', ({type, typeName, expected}) => {
        expect(model.getByType(type, 'de')).toEqual(expected);
    });

    it('throws error for getByType if type is not implemented', () => {
        expect(() => {
            model.getByType(1337 as SourceTemplateType, 'de');
        }).toThrow(`'1337' of SourceTemplateType is not implemented`);
    });
});
