import {LanguageDescription} from './LanguageDescription';

export interface OfficeModification extends LanguageDescription {
    id?: number;
    code: string;
}
