import {ReferenceType} from 'api/ReferenceType';

export interface EntryReferenceSummary {
    type: ReferenceType;
    text: string;
    entryId: number;
}
