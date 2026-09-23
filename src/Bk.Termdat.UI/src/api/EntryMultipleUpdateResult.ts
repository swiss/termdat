import {EntryMultipleUpdateInfo} from './EntryMultipleUpdateInfo';

export interface EntryMultipleUpdateResult {
    entryCount: number;
    affectedOccurrences: number;
    lockInfos?: EntryMultipleUpdateInfo[];
    dataWasChanged: boolean;
}
