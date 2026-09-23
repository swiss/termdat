import {LanguageDescription} from './LanguageDescription';

export interface GlossaryModification extends LanguageDescription {
    id?: number;
    code: string;
    description?: string;
    created?: Date;
    createdBy?: string;
    modified?: Date;
    modifiedBy?: string;
}
