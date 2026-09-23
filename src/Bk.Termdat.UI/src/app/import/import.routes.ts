import {Role} from '../auth/Role';
import {RoleGuard} from '../auth/role.guard';
import {FileImportComponent} from './file-import/file-import.component';

export const importRoutes = [
    {
        path: 'fileimport',
        component: FileImportComponent,
        data: {title: 'navigation.import.fileimport', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
];
