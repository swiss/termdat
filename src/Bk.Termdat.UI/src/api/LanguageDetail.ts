import {LanguageSummary} from './LanguageSummary';

export interface LanguageDetail extends LanguageSummary {
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
