export interface TermdatClassificationDetail {
    id: number;
    subjectName: string;
    domainId?: number;
    domainName?: string;
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
