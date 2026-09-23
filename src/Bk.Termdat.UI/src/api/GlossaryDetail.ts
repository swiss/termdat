import {GlossarySummary} from './GlossarySummary';

export interface GlossaryDetail extends GlossarySummary {
    description?: string;
    entryCount: number;
    createdBy: string;
}
