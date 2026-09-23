import {CollectionSummary} from './CollectionSummary';
import {ConceptSystemSummary} from './ConceptSystemSummary';

export interface ConceptSystemDetail extends ConceptSystemSummary {
    title: string;
    collection: CollectionSummary['code'];
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
