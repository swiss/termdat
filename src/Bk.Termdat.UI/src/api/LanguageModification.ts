import {LanguageDescription} from './LanguageDescription';

export interface LanguageModification extends LanguageDescription {
    id?: number;
    isoCode: string;
    isUiLanguage: boolean;
    sortIndex?: number;
}
