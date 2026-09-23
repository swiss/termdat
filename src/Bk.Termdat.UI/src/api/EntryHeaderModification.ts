import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';

export interface EntryHeaderModification {
    ids: number[];
    status: Status;
    reliability: Reliability;
    officeId: number;
    collectionId: number;
}
