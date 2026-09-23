import {EntryTemplateSummary} from 'api/EntryTemplateSummary';
import {SourceTemplateSummary} from 'api/SourceTemplateSummary';

export interface UserInfo {
    name?: string;
    email?: string;
    roles?: string[];
    sourceTemplates?: SourceTemplateSummary[];
    entryTemplates?: EntryTemplateSummary[];
}
