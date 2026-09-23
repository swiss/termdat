import {SearchFormComponent} from './search-form/search-form.component';

export const searchRoutes = [
    {
        path: '',
        component: SearchFormComponent,
        children: [
            {
                path: 'entry',
                loadChildren: () => import('../entry/entry.routes').then(m => m.entryRoutes),
            },
        ],
    },
];
