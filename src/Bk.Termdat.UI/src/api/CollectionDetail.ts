import {CollectionSummary} from './CollectionSummary';
import {ConceptSystemSummary} from './ConceptSystemSummary';

export interface CollectionDetail extends CollectionSummary {
    entryCount: number;
    conceptSystem?: ConceptSystemSummary;
}
