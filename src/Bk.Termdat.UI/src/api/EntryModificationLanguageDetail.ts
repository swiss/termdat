export interface EntryModificationLanguageDetail {
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
    context?: string;
    contextSource?: string;
    country?: string;
    metadata?: string;
    metadataSource?: string;

    isObsolete?: boolean;
    isRare?: boolean;
    shouldAvoid?: boolean;
}
