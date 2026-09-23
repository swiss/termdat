import {WatchlistEntry} from 'api/WatchlistEntry';

export interface WatchlistExtendedEntry extends WatchlistEntry {
    id: number;
    statusTranslation: string;
    isValidated: boolean;
    searchPhrase: string;
}
