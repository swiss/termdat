import {Routes} from '@angular/router';
import {Role} from './auth/Role';
import {RoleGuard} from './auth/role.guard';
import {RedirectGuard} from './redirect.guard';

export const routes: Routes = [
    {path: 'unauthorized', loadChildren: () => import('./auth/auth.routes').then(m => m.authRoutes)},
    {path: 'Entry/EntryDetail/:id', children: [], canActivate: [RedirectGuard]},
    {path: 'entry/:id', redirectTo: 'search/entry/:id'},
    {path: 'Entry/:id', redirectTo: 'search/entry/:id'},
    {path: '', redirectTo: 'search', pathMatch: 'full'},
    {
        path: 'search',
        loadChildren: () => import('./search/search.routes').then(m => m.searchRoutes),
    },
    {
        path: 'watchlist',
        loadChildren: () => import('./watchlist/watchlist.routes').then(m => m.watchlistRoutes),
        canActivate: [RoleGuard],
    },
    {
        path: 'user',
        loadChildren: () => import('./user/user.routes').then(m => m.userRoutes),
    },
    {
        path: 'statistic',
        loadChildren: () => import('./statistic/statistic.routes').then(m => m.statisticRoutes),
        data: {roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'administration',
        loadChildren: () => import('./administration/administration.routes').then(m => m.adminRoutes),
        data: {roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'import',
        loadChildren: () => import('./import/import.routes').then(m => m.importRoutes),
        data: {roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'duplicate-check',
        loadChildren: () => import('./duplicate-check/duplicate-check.routes').then(m => m.duplicateCheckRoutes),
        data: {roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {path: '**', redirectTo: 'search'},
];
