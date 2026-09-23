import {ObIAlertType} from '@oblique/oblique';

export interface ConfirmData {
    title: string;
    text: string;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    textValue: any;
    confirm: string;
    alert?: {
        text: string;
        type?: ObIAlertType;
    };
}
