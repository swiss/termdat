export interface EntryLanguageSummary {
    languageId: number;
    sequence: number;
    terminus?: string;
    name?: string;
    phraseology?: string;
    abbreviation?: string;
    isObsolete?: boolean;
    isRare?: boolean;
    shouldAvoid?: boolean;
}
