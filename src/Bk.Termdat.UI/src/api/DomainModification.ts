import {LanguageDescription} from './LanguageDescription';
import {SubjectSummary} from './SubjectSummary';

export interface DomainModification extends LanguageDescription {
    id?: number;
    subjectId?: SubjectSummary['id'];
}
