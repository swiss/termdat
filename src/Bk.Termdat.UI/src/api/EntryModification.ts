import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {EntryModificationReference} from 'api/EntryModificationReference';
import {PictureDetail} from 'api/PictureDetail';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {AdminInfoSummary} from './AdminInfoSummary';

export interface EntryModification {
    id?: number;
    status: Status;
    reliability: Reliability;
    identification?: number;
    comment?: string;

    officeId: number;
    collectionId: number;
    lenochClassificationIds: number[];
    subjectIds?: number[];
    domainIds?: number[];
    relatedCollectionIds: number[];
    glossaryIds: number[];
    adminInfos: AdminInfoSummary[];
    conceptSystemPositionId?: number;

    languageDetails?: EntryLanguageDetail[];

    concurrencyToken?: number;
    picture?: PictureDetail;
    image?: File;
    pictureDeleted?: boolean;

    references?: EntryModificationReference[];
    templateName?: string;
}
