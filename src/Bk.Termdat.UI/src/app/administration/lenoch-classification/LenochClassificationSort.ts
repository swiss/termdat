import {LenochClassificationDetail} from 'api/LenochClassificationDetail';

export function sortLenochClassificationDetails(a: LenochClassificationDetail, b: LenochClassificationDetail, currentLang = 'de') {
    return sortInternal(a.code, b.code, currentLang) || sortInternal(a.subject, b.subject, currentLang) || sortInternal(a.domain, b.domain, currentLang);
}

function sortInternal(a: string | undefined, b: string | undefined, currentLang: string) {
    if (!a && !b) {
        return 0;
    }
    if (!a) {
        return -1;
    }
    if (!b) {
        return 1;
    }

    return a.localeCompare(b, currentLang, {sensitivity: 'accent'});
}
