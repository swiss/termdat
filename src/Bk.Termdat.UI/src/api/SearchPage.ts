import {EntrySummary} from './EntrySummary';

export interface SearchPage {
    totalEntryCount: number;
    searchEntries: EntrySummary[];
}
