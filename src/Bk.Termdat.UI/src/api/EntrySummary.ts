import {CollectionSummary} from './CollectionSummary';
import {EntryLanguageSummary} from './EntryLanguageSummary';
import {Status} from './Status';

export interface EntrySummary {
    id: number;
    status: Status;
    deleted: boolean;
    isMainOffice: boolean;
    collection: CollectionSummary;
    terms: EntryLanguageSummary[];
    canBeAddedToWatchList: boolean;
}
