import {CollectionSummary} from './CollectionSummary';
import {ConceptSystemPositionModification} from './ConceptSystemPositionModification';

export interface ConceptSystemModification {
    id?: number;
    title: string;
    collectionId: CollectionSummary['id'];
    positions?: ConceptSystemPositionModification[];
}
