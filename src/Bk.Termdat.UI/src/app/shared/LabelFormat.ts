export function toCodeNameLabel(obj?: {code: string; name: string}): string {
    return obj ? `${obj.code} - ${obj.name}` : '-';
}
