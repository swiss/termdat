import {Role} from '../auth/Role';
import {RoleGuard} from '../auth/role.guard';
import {DuplicateCheckComponent} from './duplicate-check.component';

export const duplicateCheckRoutes = [
    {
        path: '',
        component: DuplicateCheckComponent,
        data: {title: 'navigation.duplicateCheck', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
];
