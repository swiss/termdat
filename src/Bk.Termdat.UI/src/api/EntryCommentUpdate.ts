import {MultiEditAction} from './MultiEditAction';

export interface EntryCommentUpdate {
    ids: number[];
    comment: string | null;
    action: MultiEditAction;
}
