import {MultiEditAction} from './MultiEditAction';

export interface EntrySourceUpdate {
    ids: number[];
    languageIds: number[];
    targetFields: string[];
    updateValue: string;
    action: MultiEditAction;
}
