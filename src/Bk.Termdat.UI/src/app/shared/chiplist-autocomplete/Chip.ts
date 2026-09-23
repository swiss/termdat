export class Chip {
    tag: string;
    displayText: string;
    value?: string | number;
    deleted?: boolean;
    inactive?: boolean;

    constructor(tag: string, displayText: string, value?: string | number, deleted = false, inactive = false) {
        this.tag = tag;
        this.displayText = displayText;
        this.value = value;
        this.deleted = deleted;
        this.inactive = inactive;
    }

    static findByTag(tag: string) {
        return (chip: Chip) => chip.tag.toLocaleUpperCase() === tag.toLocaleUpperCase();
    }

    static findByValue(value: string | number) {
        return (chip: Chip) => chip.value?.toString().toLocaleUpperCase() === value.toString().toLocaleUpperCase();
    }

    static fromOption({id, code, name}: {id: number; code: string; name: string}) {
        return new Chip(code, `${code} - ${name}`, id);
    }
}
