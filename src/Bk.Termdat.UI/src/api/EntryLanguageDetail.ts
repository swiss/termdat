import {EntryReferenceSummary} from 'api/EntryReferenceSummary';
import {NoteSubCodeTooltip} from './NoteSubCodeTooltip';

export interface EntryLanguageDetail {
    id?: number;
    languageId: number;
    sequence: number;

    terminus?: string;
    terminusSource?: string;
    name?: string;
    nameSource?: string;
    abbreviation?: string;
    abbreviationSource?: string;
    phraseology?: string;
    phraseologySource?: string;
    definition?: string;
    definitionSource?: string;
    note?: string;
    noteSource?: string;
    noteTooltips?: NoteSubCodeTooltip[];
    context?: string;
    contextSource?: string;
    country?: string;
    metadata?: string;
    metadataSource?: string;
    references?: EntryReferenceSummary[];
    isObsolete?: boolean;
    isRare?: boolean;
    shouldAvoid?: boolean;
}
