import {DomainSummary} from './DomainSummary';

export interface DomainDetail extends DomainSummary {
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
