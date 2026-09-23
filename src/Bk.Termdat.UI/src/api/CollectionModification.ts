import {ConceptSystemSummary} from './ConceptSystemSummary';
import {LanguageDescription} from './LanguageDescription';

export interface CollectionModification extends LanguageDescription {
    id?: number;
    active: boolean;
    activeApi: boolean;
    exportLindas: boolean;
    code: string;
    conceptSystem?: ConceptSystemSummary;
    description?: string;
    documentation?: string;
    created?: Date;
    createdBy?: string;
    modified?: Date;
    modifiedBy?: string;
}
