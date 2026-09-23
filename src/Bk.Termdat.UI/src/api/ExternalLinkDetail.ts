import {ExternalLinkType} from 'api/ExternalLinkType';

export interface ExternalLinkDetail {
    id?: number;
    type?: ExternalLinkType;
    url?: string;
}
