import {RoleGuard} from '../auth/role.guard';
import {ProfileComponent} from './profile/profile.component';

export const userRoutes = [
    {
        path: '',
        component: ProfileComponent,
        canActivate: [RoleGuard],
    },
];
