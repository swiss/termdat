import {AdminInfoSummary} from './AdminInfoSummary';

export interface FileImport {
    entryOffice: number;
    entryCollection: number;
    reliability: number;
    status: number;
    subjectIds?: number[];
    domainIds?: number[];
    comment: string;
    glossaryIds?: number[];
    adminInfos: AdminInfoSummary[];
    file: File;
}
