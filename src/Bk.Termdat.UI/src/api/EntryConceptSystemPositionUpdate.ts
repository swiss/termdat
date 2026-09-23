import {MultiEditAction} from './MultiEditAction';

export interface EntryConceptSystemPositionUpdate {
    ids: number[];
    conceptSystemPositionId?: number;
    action: MultiEditAction;
}
