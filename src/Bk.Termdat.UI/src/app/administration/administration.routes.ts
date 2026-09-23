import {Role} from '../auth/Role';
import {RoleGuard} from '../auth/role.guard';
import {AdminInfoCreateComponent} from './admin-info/admin-info-create/admin-info-create.component';
import {AdminInfoUpdateComponent} from './admin-info/admin-info-update/admin-info-update.component';
import {AdminInfoComponent} from './admin-info/admin-info.component';
import {CollectionCreateComponent} from './collection/collection-create/collection-create.component';
import {CollectionUpdateComponent} from './collection/collection-update/collection-update.component';
import {CollectionsComponent} from './collection/collections.component';
import {ConceptSystemCreateComponent} from './concept-system/concept-system-create/concept-system-create.component';
import {ConceptSystemUpdateComponent} from './concept-system/concept-system-update/concept-system-update.component';
import {ConceptSystemsComponent} from './concept-system/concept-systems.component';
import {GlossariesComponent} from './glossary/glossaries.component';
import {GlossaryCreateComponent} from './glossary/glossary-create/glossary-create.component';
import {GlossaryUpdateComponent} from './glossary/glossary-update/glossary-update.component';
import {LanguageCreateComponent} from './language/language-create/language-create.component';
import {LanguageUpdateComponent} from './language/language-update/language-update.component';
import {LanguagesComponent} from './language/languages.component';
import {LenochClassificationComponent} from './lenoch-classification/lenoch-classification.component';
import {MailboxComponent} from './mailbox/mailbox.component';
import {NewsCreateComponent} from './news/news-create/news-create.component';
import {NewsUpdateComponent} from './news/news-update/news-update.component';
import {NewsComponent} from './news/news.component';
import {OfficeCreateComponent} from './office/office-create/office-create.component';
import {OfficeUpdateComponent} from './office/office-update/office-update.component';
import {OfficesComponent} from './office/offices.component';
import {TermdatClassificationCreateComponent} from './termdat-classification/classification-create/classification-create.component';
import {TermdatClassificationDomainUpdateComponent} from './termdat-classification/domain-update/domain-update.component';
import {TermdatClassificationSubjectUpdateComponent} from './termdat-classification/subject-update/subject-update.component';
import {TermdatClassificationComponent} from './termdat-classification/termdat-classification.component';
import {UserAdministrationUpdateComponent} from './user-administration/user-administration-update/user-administration-update.component';
import {UserAdministrationComponent} from './user-administration/user-administration.component';

export const adminRoutes = [
    {
        path: 'mailbox',
        component: MailboxComponent,
        data: {title: 'navigation.administration.mailbox', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'collection',
        component: CollectionsComponent,
        data: {title: 'navigation.administration.collections', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'collection/create',
        component: CollectionCreateComponent,
        data: {title: 'navigation.administration.collections.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'collection/:id',
        component: CollectionUpdateComponent,
        data: {title: 'navigation.administration.collections.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'glossary',
        component: GlossariesComponent,
        data: {title: 'navigation.administration.glossaries', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'glossary/create',
        component: GlossaryCreateComponent,
        data: {title: 'navigation.administration.glossaries.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'glossary/:id',
        component: GlossaryUpdateComponent,
        data: {title: 'navigation.administration.glossaries.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'termdatClassification',
        component: TermdatClassificationComponent,
        data: {title: 'navigation.administration.termdatClassifications', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'termdatClassification/create',
        component: TermdatClassificationCreateComponent,
        data: {title: 'navigation.administration.termdatClassification.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'termdatClassification/subject/:id',
        component: TermdatClassificationSubjectUpdateComponent,
        data: {title: 'navigation.administration.termdatClassification.subject.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'termdatClassification/subject/:subjectId/domain/:domainId',
        component: TermdatClassificationDomainUpdateComponent,
        data: {title: 'navigation.administration.termdatClassification.domain.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'lenochClassification',
        component: LenochClassificationComponent,
        data: {title: 'navigation.administration.lenochClassifications', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'office',
        component: OfficesComponent,
        data: {title: 'navigation.administration.offices', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'office/create',
        component: OfficeCreateComponent,
        data: {title: 'navigation.administration.offices.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'office/:id',
        component: OfficeUpdateComponent,
        data: {title: 'navigation.administration.offices.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'language',
        component: LanguagesComponent,
        data: {title: 'navigation.administration.languages', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'language/create',
        component: LanguageCreateComponent,
        data: {title: 'navigation.administration.language.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'language/:id',
        component: LanguageUpdateComponent,
        data: {title: 'navigation.administration.language.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'conceptSystem',
        component: ConceptSystemsComponent,
        data: {title: 'navigation.administration.conceptSystems', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'conceptSystem/create',
        component: ConceptSystemCreateComponent,
        data: {title: 'navigation.administration.conceptSystems.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'conceptSystem/:id',
        component: ConceptSystemUpdateComponent,
        data: {title: 'navigation.administration.conceptSystems.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'userAdministration',
        component: UserAdministrationComponent,
        data: {title: 'navigation.administration.userAdministration', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'userAdministration/:id',
        component: UserAdministrationUpdateComponent,
        data: {title: 'navigation.administration.userAdministration', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'news',
        component: NewsComponent,
        data: {title: 'navigation.administration.news', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'news/create',
        component: NewsCreateComponent,
        data: {title: 'navigation.administration.news.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'news/:id',
        component: NewsUpdateComponent,
        data: {title: 'navigation.administration.news.edit', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'adminInfo',
        component: AdminInfoComponent,
        data: {title: 'navigation.administration.adminInfoAdministration', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'adminInfo/create',
        component: AdminInfoCreateComponent,
        data: {title: 'navigation.administration.adminInfos.create', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
    {
        path: 'adminInfo/:id',
        component: AdminInfoUpdateComponent,
        data: {title: 'navigation.administration.adminInfos.update', roles: [Role.Admin]},
        canActivate: [RoleGuard],
    },
];
