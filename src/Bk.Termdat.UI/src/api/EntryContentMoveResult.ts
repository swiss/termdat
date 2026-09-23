import {EntryMultipleUpdateInfo} from './EntryMultipleUpdateInfo';

export interface EntryContentMoveResult {
    nonAffectedEntryIds: number[];
    lockInfos?: EntryMultipleUpdateInfo[];
    dataWasChanged: boolean;
}
