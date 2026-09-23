import {EntryReferenceLanguageDetail} from 'api/EntryReferenceLanguageDetail';
import {ReferenceType} from 'api/ReferenceType';

export interface EntryReferenceDetail {
    id?: number;
    entryId: number;
    type?: ReferenceType;
    languages: EntryReferenceLanguageDetail[];
}
