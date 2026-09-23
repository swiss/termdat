import {Role} from '../auth/Role';
import {RoleGuard} from '../auth/role.guard';
import {MultiEditCommentComponent} from './multi-edit-comment/multi-edit-comment.component';
import {MultiEditConceptSystemComponent} from './multi-edit-concept-system/multi-edit-concept-system.component';
import {MultiEditHeaderDataComponent} from './multi-edit-header-data/multi-edit-header-data.component';
import {MultiEditSourceComponent} from './multi-edit-source/multi-edit-source.component';
import {MultiMoveContentComponent} from './multi-move-content/multi-move-content.component';
import {WatchlistFormComponent} from './watchlist-form/watchlist-form.component';
import {WatchlistMultipleSearchAndReplaceComponent} from './watchlist-multiple-search-and-replace/watchlist-multiple-search-and-replace.component';
import {WatchlistUpdateMultipleHeaderComponent} from './watchlist-update-multiple-header/watchlist-update-multiple-header.component';

export const watchlistRoutes = [
    {
        path: '',
        component: WatchlistFormComponent,
        canActivate: [RoleGuard],
    },
    {
        path: 'updateMandatoryHeaderFields',
        component: WatchlistUpdateMultipleHeaderComponent,
        data: {title: 'watchlist.editHeaderData', roles: [Role.Admin, Role.Writer]},
        canActivate: [RoleGuard],
    },
    {
        path: 'updateOptionalChipFields',
        component: MultiEditHeaderDataComponent,
        data: {title: 'watchlist.editHeaderData', roles: [Role.Admin, Role.Writer]},
        canActivate: [RoleGuard],
    },
    {
        path: 'updateComments',
        component: MultiEditCommentComponent,
        data: {title: 'watchlist.editComments', roles: [Role.Admin, Role.Writer]},
    },
    {
        path: 'updateConceptSystem',
        component: MultiEditConceptSystemComponent,
        data: {title: 'watchlist.editConceptSystem', roles: [Role.Admin, Role.Writer]},
    },
    {
        path: 'updateSource',
        component: MultiEditSourceComponent,
        data: {title: 'watchlist.editSource', roles: [Role.Admin, Role.Writer, Role.SuperUser]},
        canActivate: [RoleGuard],
    },
    {
        path: 'moveContent',
        component: MultiMoveContentComponent,
        data: {title: 'watchlist.moveContent', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'multipleSearchAndReplace',
        component: WatchlistMultipleSearchAndReplaceComponent,
        data: {title: 'watchlist.multipleSearchAndReplace', roles: [Role.Admin, Role.Writer]},
        canActivate: [RoleGuard],
    },
];
