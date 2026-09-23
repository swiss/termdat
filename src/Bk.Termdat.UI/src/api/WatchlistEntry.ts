import {CollectionSummary} from 'api/CollectionSummary';
import {Status} from 'api/Status';
import {ConceptSystemPositionSummary} from './ConceptSystemPositionSummary';

export interface WatchlistEntry {
    entryId: number;
    identification?: number;
    conceptSystemPosition: ConceptSystemPositionSummary;
    value: string;
    status: Status;
    deleted: boolean;
    locked: boolean;
    collection: CollectionSummary;
    isMainOffice: boolean;
    glossaries: string;
    adminInfos: string;
}
