export interface UserAccountModification {
    id?: number;
    information?: string;
    authorizedCollectionIds: number[];
    authorizedGlossaryIds: number[];
}
