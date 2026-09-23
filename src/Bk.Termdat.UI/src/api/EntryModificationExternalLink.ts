import {ExternalLinkType} from 'api/ExternalLinkType';

export interface EntryModificationExternalLink {
    id?: number;
    url: number;
    type: ExternalLinkType;
}
