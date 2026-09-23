import {RoleGuard} from '../auth/role.guard';
import {UnsavedChangesAndEntryLockedGuard} from '../shared/unsaved-changes/unsaved-changes-and-entry-locked.guard';
import {UnsavedChangesGuard} from '../shared/unsaved-changes/unsaved-changes.guard';
import {EntryCopyComponent} from './entry-copy/entry-copy.component';
import {EntryCreateComponent} from './entry-create/entry-create.component';
import {EntryDetailFormComponent} from './entry-detail-form/entry-detail-form.component';
import {EntryTemplateComponent} from './entry-template/entry-template.component';
import {EntryTemplateUpdateComponent} from './entry-template-update/entry-template-update.component';
import {EntryUpdateComponent} from './entry-update/entry-update.component';

export const entryRoutes = [
    {
        path: 'create',
        component: EntryCreateComponent,
        canActivate: [RoleGuard],
        canDeactivate: [UnsavedChangesGuard],
    },
    {
        path: ':id/update',
        component: EntryUpdateComponent,
        canActivate: [RoleGuard],
        canDeactivate: [UnsavedChangesAndEntryLockedGuard],
    },
    {
        path: ':id/copy',
        component: EntryCopyComponent,
        canActivate: [RoleGuard],
        canDeactivate: [UnsavedChangesGuard],
    },
    {
        path: ':id/template',
        component: EntryTemplateComponent,
        canActivate: [RoleGuard],
        canDeactivate: [UnsavedChangesGuard],
    },
    {
        path: ':id/template-update',
        component: EntryTemplateUpdateComponent,
        canActivate: [RoleGuard],
        canDeactivate: [UnsavedChangesGuard],
    },
    {
        path: ':id',
        component: EntryDetailFormComponent,
    },
    // Routing for no entry found
    {
        path: '',
        component: EntryDetailFormComponent,
    },
];
