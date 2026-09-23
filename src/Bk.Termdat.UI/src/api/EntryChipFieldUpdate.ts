import {MultiEditAction} from './MultiEditAction';

export interface EntryChipFieldUpdate {
    ids: number[];
    codes: string[];
    action: MultiEditAction;
}

export enum ChipFieldType {
    AdminInfo = 0,
    Glossary = 1,
    Collection = 2,
}
