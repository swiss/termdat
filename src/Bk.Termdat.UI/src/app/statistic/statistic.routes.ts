import {Role} from '../auth/Role';
import {RoleGuard} from '../auth/role.guard';
import {StatisticComponent} from './statistic/statistic.component';

export const statisticRoutes = [
    {
        path: 'termdat',
        component: StatisticComponent,
        data: {title: 'navigation.statistic.termdat', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
];
