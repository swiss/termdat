import {ReferenceType} from 'api/ReferenceType';

export interface EntryModificationReference {
    id?: number;
    entryId: number;
    type: ReferenceType;
}
