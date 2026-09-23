import {HttpClient} from '@angular/common/http';
import {Injectable, OnDestroy} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {CollectionSummary} from 'api/CollectionSummary';
import {CommentStatus} from 'api/CommentStatus';
import {CommentStatusOption} from 'api/CommentStatusOption';
import {GlossarySummary} from 'api/GlossarySummary';
import {OfficeSummary} from 'api/OfficeSummary';
import {Status} from 'api/Status';
import {SubjectSummary} from 'api/SubjectSummary';
import {UserAccountSummary} from 'api/UserAccountSummary';
import {Observable, of, Subject} from 'rxjs';
import {takeUntil, tap} from 'rxjs/operators';
import {CollectionService} from '../administration/collection/collection.service';
import {GlossaryService} from '../administration/glossary/glossary.service';
import {OfficeService} from '../administration/office/office.service';
import {TermdatClassificationService} from '../administration/termdat-classification/termdat-classification.service';
import {UserAccountService} from '../administration/user-administration/user-account.service';
import {Option} from '../shared/Option';

@Injectable({
    providedIn: 'root',
})
export class ExtendedSearchFiltersService implements OnDestroy {
    readonly resetExtendedSearch$ = new Subject<void>();
    readonly reloadExtendedSearchData$ = new Subject<void>();
    readonly cleared$ = new Subject<void>();

    private userAccounts: UserAccountSummary[] = [];
    private adminInfos: AdminInfoSummary[] = [];
    private collections: CollectionSummary[] = [];
    private offices: OfficeSummary[] = [];
    private glossaries: GlossarySummary[] = [];
    private subjects: SubjectSummary[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly httpClient: HttpClient,
        private readonly translateService: TranslateService,
        private readonly collectionService: CollectionService,
        private readonly officeService: OfficeService,
        private readonly termdatClassificationService: TermdatClassificationService,
        private readonly userAccountService: UserAccountService,
        private readonly glossaryService: GlossaryService
    ) {
        this.translateService.onLangChange.pipe(takeUntil(this.unsubscribe)).subscribe(() => {
            this.clear();
        });
    }

    clear() {
        this.userAccounts = [];
        this.collections = [];
        this.offices = [];
        this.glossaries = [];
        this.subjects = [];
        this.cleared$.next();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    getAdminInfos(): Observable<AdminInfoSummary[]> {
        return this.httpClient.get<AdminInfoSummary[]>('/api/admin-infos').pipe(tap(infos => (this.adminInfos = infos)));
    }

    getCollections(): Observable<CollectionSummary[]> {
        if (this.collections.length > 0) {
            return of(this.collections);
        }
        return this.collectionService.getAllCollections().pipe(tap(collections => (this.collections = collections)));
    }

    getOffices(): Observable<OfficeSummary[]> {
        if (this.offices.length > 0) {
            return of(this.offices);
        }

        return this.officeService.getOffices().pipe(tap(offices => (this.offices = offices)));
    }

    getGlossaries(): Observable<GlossarySummary[]> {
        if (this.glossaries.length > 0) {
            return of(this.glossaries);
        }

        return this.glossaryService.getGlossarySummaries().pipe(tap(glossaries => (this.glossaries = glossaries)));
    }

    getUserAccounts(): Observable<UserAccountSummary[]> {
        if (this.userAccounts.length > 0) {
            return of(this.userAccounts);
        }
        return this.userAccountService.getUserAccountSummaries().pipe(tap(userAccounts => (this.userAccounts = userAccounts)));
    }

    getStates(): Observable<Option[]> {
        return of([
            {
                id: Status.Validated,
                name: this.translateService.instant(`field.status.${Status[Status.Validated]}`),
            },
            {
                id: Status.InProgress,
                name: this.translateService.instant(`field.status.${Status[Status.InProgress]}`),
            },
        ]);
    }

    getCommentStatusOptions(): Observable<CommentStatusOption[]> {
        return of([
            {
                id: CommentStatus.Empty,
                name: this.translateService.instant(`field.commentStatus.${CommentStatus[CommentStatus.Empty]}`),
            },
            {
                id: CommentStatus.Filled,
                name: this.translateService.instant(`field.commentStatus.${CommentStatus[CommentStatus.Filled]}`),
            },
        ]);
    }

    getSubjects(): Observable<SubjectSummary[]> {
        if (this.subjects.length > 0) {
            return of(this.subjects);
        }
        return this.termdatClassificationService.getSubjects().pipe(tap(subjects => (this.subjects = subjects)));
    }
}
