export interface CollectionSummary {
    id: number;
    code: string;
    name: string;
    classification?: string;
    isObsolete: boolean;
    deleted: boolean;
    active: boolean;
    activeApi: boolean;
    exportLindas: boolean;
}
