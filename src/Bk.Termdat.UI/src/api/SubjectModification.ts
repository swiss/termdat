import {DomainModification} from './DomainModification';
import {LanguageDescription} from './LanguageDescription';

export interface SubjectModification extends LanguageDescription {
    id?: number;
    domains: DomainModification[];
}
