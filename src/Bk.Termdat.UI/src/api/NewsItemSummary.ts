export interface NewsItemSummary {
    id: number;
    isEnabled: boolean;
    title: string;
    validFrom: Date;
    validTo: Date;
    text: string;
    canMoveUp: boolean;
    canMoveDown: boolean;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
