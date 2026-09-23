import {Injectable, OnDestroy} from '@angular/core';
import {BehaviorSubject, Observable, Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {AuthService} from '../auth/auth.service';
import {Role} from '../auth/Role';
import {authenticatedPageSize, defaultPageSize, superReaderPageSize, writerPageSize} from './PaginationMapper';

@Injectable({
    providedIn: 'root',
})
export class PageSizeService implements OnDestroy {
    public pageSize$: Observable<number[]>;

    private readonly pageSizeSubject = new BehaviorSubject<number[]>(defaultPageSize);
    private readonly unsubscribe = new Subject<void>();

    constructor(private readonly authService: AuthService) {
        this.pageSize$ = this.pageSizeSubject.asObservable();

        this.authService.roles$.pipe(takeUntil(this.unsubscribe)).subscribe(roles => {
            if (roles.includes(Role.Writer)) {
                this.pageSizeSubject.next(writerPageSize);
            } else if (roles.includes(Role.SuperReader)) {
                this.pageSizeSubject.next(superReaderPageSize);
            } else if (roles.includes(Role.Admin) || roles.includes(Role.SuperUser)) {
                this.pageSizeSubject.next(authenticatedPageSize);
            } else {
                this.pageSizeSubject.next(defaultPageSize);
            }
        });
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.pageSizeSubject.complete();
    }
}
