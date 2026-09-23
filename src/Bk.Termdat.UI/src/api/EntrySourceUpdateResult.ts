import {EntryMultipleUpdateInfo} from './EntryMultipleUpdateInfo';

export interface EntrySourceUpdateResult {
    dataWasChanged: boolean;
    notFullyUpdatedEntryIds: number[];
    lockInfos?: EntryMultipleUpdateInfo[];
}
