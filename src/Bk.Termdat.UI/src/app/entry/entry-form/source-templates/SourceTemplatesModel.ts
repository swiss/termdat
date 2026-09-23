import {SourceTemplateDefault} from './SourceTemplateDefault';
import {SourceTemplateDefaults} from './SourceTemplateDefaults';
import {SourceTemplateType} from './SourceTemplateType';

export class SourceTemplatesModel {
    private static readonly DATE_REGEX = /{{[\s]*date[\s]*}}/;
    private readonly sourceTemplate: SourceTemplateDefaults;

    constructor(sourceTemplate: SourceTemplateDefaults) {
        this.sourceTemplate = sourceTemplate;
    }

    public getFederalLawSrTemplate(language: keyof SourceTemplateDefaults): string {
        return this.getSourceTemplateValue(language, 'federalLawSr');
    }

    public getFederalLawBblTemplate(language: keyof SourceTemplateDefaults): string {
        return this.getSourceTemplateValue(language, 'federalLawBbl');
    }

    public getFederalLawAsTemplate(language: keyof SourceTemplateDefaults): string {
        return this.getSourceTemplateValue(language, 'federalLawAs');
    }

    public getFederalCourtTemplate(language: keyof SourceTemplateDefaults): string {
        return this.getSourceTemplateValue(language, 'federalCourt');
    }

    public getFederalCouncilMessageTemplate(language: keyof SourceTemplateDefaults): string {
        return this.getSourceTemplateValue(language, 'federalCouncilMessage');
    }

    public getInternetTemplate(language: keyof SourceTemplateDefaults): string {
        let template = this.getSourceTemplateValue(language, 'internet');
        template = template.replace(SourceTemplatesModel.DATE_REGEX, SourceTemplatesModel.getCurrentDate());

        return template;
    }

    public getByType(type: SourceTemplateType, language: keyof SourceTemplateDefaults) {
        switch (type) {
            case SourceTemplateType.FederalLawSr:
                return this.getFederalLawSrTemplate(language);
            case SourceTemplateType.FederalLawBbl:
                return this.getFederalLawBblTemplate(language);
            case SourceTemplateType.FederalLawAs:
                return this.getFederalLawAsTemplate(language);
            case SourceTemplateType.FederalCourt:
                return this.getFederalCourtTemplate(language);
            case SourceTemplateType.FederalCouncilMessage:
                return this.getFederalCouncilMessageTemplate(language);
            case SourceTemplateType.Internet:
                return this.getInternetTemplate(language);
            default:
                throw new Error(`'${type}' of SourceTemplateType is not implemented`);
        }
    }

    private static getCurrentDate(): string {
        const now = new Date(Date.now()); // Date.now() is used to be able to mock Date in Tests

        return now.toISOString().split('T')[0];
    }

    private getSourceTemplateValue(language: keyof SourceTemplateDefaults, key: keyof SourceTemplateDefault) {
        if (!this.sourceTemplate) {
            return '';
        }

        const value = this.sourceTemplate[language] ?? '';

        return value[key] ?? '';
    }
}
