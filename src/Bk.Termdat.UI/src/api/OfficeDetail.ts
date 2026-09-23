import {OfficeSummary} from './OfficeSummary';

export interface OfficeDetail extends OfficeSummary {
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
