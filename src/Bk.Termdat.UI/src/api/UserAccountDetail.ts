import {CollectionSummary} from './CollectionSummary';
import {GlossarySummary} from './GlossarySummary';

export interface UserAccountDetail {
    id: number;
    lastName: string;
    firstName: string;
    email: string;
    hasRole: boolean;
    role?: string;
    lastAction?: Date;
    collections: CollectionSummary[];
    glossaries: GlossarySummary[];
    hasFullAccess: boolean;
    isWriter: boolean;
    information?: string;
}
