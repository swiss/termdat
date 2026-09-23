export interface NewsItemDetail {
    id: number;
    isEnabled: boolean;
    validFrom: Date;
    validTo: Date;
    titleDe: string;
    titleFr: string;
    titleIt: string;
    titleRm: string;
    titleEn: string;
    textDe: string;
    textFr: string;
    textIt: string;
    textRm: string;
    textEn: string;
    created?: Date;
    createdBy?: string;
    modified?: Date;
    modifiedBy?: string;
}
