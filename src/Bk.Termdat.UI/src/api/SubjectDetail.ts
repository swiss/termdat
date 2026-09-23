import {DomainDetail} from './DomainDetail';

export interface SubjectDetail {
    id: number;
    name: string;
    domains: DomainDetail[];
    entryCount: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
