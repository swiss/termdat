import {EntryModificationLanguageDetail} from 'api/EntryModificationLanguageDetail';

export function languageDetailCompare(a: EntryModificationLanguageDetail, b: EntryModificationLanguageDetail): number {
    return a.languageId * 100 + a.sequence - (b.languageId * 100 + b.sequence);
}

export function sequenceCompare(a: {sequence: number}, b: {sequence: number}): number {
    return a.sequence - b.sequence;
}
