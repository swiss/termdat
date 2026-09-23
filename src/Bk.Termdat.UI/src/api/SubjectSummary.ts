import {DomainSummary} from 'api/DomainSummary';

export interface SubjectSummary {
    id: number;
    name: string;
    domains: DomainSummary[];
}
