import {AdminInfoSummary} from './AdminInfoSummary';
import {MultiEditAction} from './MultiEditAction';

export interface EntryChipFieldInputUpdate {
    ids: number[];
    codes: AdminInfoSummary[];
    action: MultiEditAction;
}
