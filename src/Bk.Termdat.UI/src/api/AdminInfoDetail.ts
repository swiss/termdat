import {AdminInfoSummary} from './AdminInfoSummary';

export interface AdminInfoDetail extends AdminInfoSummary {
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
