import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {Inject, Injectable, OnDestroy, computed, signal} from '@angular/core';
import {toObservable} from '@angular/core/rxjs-interop';
import {ObNotificationService} from '@oblique/oblique';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {Observable, ReplaySubject, Subject, catchError, finalize, map, mergeMap, of, tap} from 'rxjs';
import {UserAccountService} from '../administration/user-administration/user-account.service';
import {ConfigsService} from '../config.service';
import {ExtendedSearchFiltersService} from '../search/extended-search-filters.service';
import {LOCATION, SESSION_STORAGE} from '../shared/injection.tokens';
import {AuthResponse} from './AuthResponse';
import {Role} from './Role';
import {UserInfo} from './UserInfo';

@Injectable({
    providedIn: 'root',
})
export class AuthService implements OnDestroy {
    readonly isAuthenticated$: Observable<boolean>;
    readonly userInfo$: Observable<UserInfo>;

    readonly isWriter$: Observable<boolean>;
    readonly isSuperUser$: Observable<boolean>;
    readonly isSuperReader$: Observable<boolean>;
    readonly isAdmin$: Observable<boolean>;
    // Emits once checkAuth() has completed (successfully or with error). Used to gate logic that must wait
    // until the initial auth check against the backend (/auth/user) has finished.
    readonly authCompleted$: Observable<boolean>;
    readonly roles$: Observable<Role[]>;
    readonly userAccountDetail$ = new ReplaySubject<UserAccountDetail>();

    readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
    readonly userInfo = computed(() => (this.authState().isAuthenticated ? this.extractUserInfoFromClaims(this.authState().claims) : {}));
    readonly roles = computed(() => {
        const userInfo = this.userInfo();

        return userInfo.roles?.includes(this.configsService.frontendConfig.roles.allow)
            ? userInfo.roles?.map(role => this.roleMap[role]).filter(role => role != null) || []
            : [];
    });

    private static readonly CLAIM_NAME_GIVEN_NAME = 'given_name';
    private static readonly CLAIM_NAME_FAMILY_NAME = 'family_name';
    private static readonly CLAIM_NAME_EMAIL = 'email';
    private static readonly CLAIM_NAME_ROLE = 'role';

    private readonly unsubscribe = new Subject<void>();
    private readonly authCompletedSubject = new ReplaySubject<boolean>(1);
    private readonly roleMap: Record<string, Role>;

    // Internal auth state populated from the Gateway's /auth/user endpoint.
    private readonly authState = signal<AuthResponse>({
        isAuthenticated: false,
        claims: [],
    });

    constructor(
        private readonly http: HttpClient,
        private readonly userAccountService: UserAccountService,
        private readonly extendedSearchFiltersService: ExtendedSearchFiltersService,
        private readonly notificationService: ObNotificationService,
        private readonly configsService: ConfigsService,
        @Inject(SESSION_STORAGE) private readonly sessionStorage: Storage,
        @Inject(LOCATION) private readonly location: Location
    ) {
        this.roleMap = {
            [configsService.frontendConfig.roles.admin]: Role.Admin,
            [configsService.frontendConfig.roles.writer]: Role.Writer,
            [configsService.frontendConfig.roles.superUser]: Role.SuperUser,
            [configsService.frontendConfig.roles.superReader]: Role.SuperReader,
        };

        // Convert signals to observables for backward compatibility with existing consumers.
        this.isAuthenticated$ = toObservable(this.isAuthenticated);
        this.userInfo$ = toObservable(this.userInfo);
        this.roles$ = toObservable(this.roles);

        this.isAdmin$ = this.roles$.pipe(map(roles => roles.includes(Role.Admin)));
        this.isWriter$ = this.roles$.pipe(map(roles => roles.includes(Role.Writer)));
        this.isSuperUser$ = this.roles$.pipe(map(roles => roles.includes(Role.SuperUser)));
        this.isSuperReader$ = this.roles$.pipe(map(roles => roles.includes(Role.SuperReader)));
        this.authCompleted$ = this.authCompletedSubject.asObservable();
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
        this.userAccountDetail$.complete();
        this.authCompletedSubject.complete();
    }

    checkAuth(): Observable<boolean> {
        return this.fetchAuthState().pipe(
            mergeMap(isAuthenticated =>
                isAuthenticated
                    ? this.userAccountService.createOrUpdate().pipe(
                          tap(details => this.userAccountDetail$.next(details)),
                          tap(() => this.extendedSearchFiltersService.clear()),
                          catchError(() => {
                              this.notificationService.error('userAccount.notification.createOrUpdateError');
                              return of(null);
                          }),
                          map(() => isAuthenticated)
                      )
                    : of(isAuthenticated)
            ),
            finalize(() => this.authCompletedSubject.next(true))
        );
    }

    logout(): void {
        this.sessionStorage.clear();
        // Redirect to the Gateway's logout endpoint (cookie + OIDC sign-out).
        this.location.href = '/auth/logout';
    }

    login(): void {
        this.sessionStorage.clear();
        // Redirect to the Gateway's login endpoint with the current location as the return URL.
        const returnUrl = encodeURIComponent(this.location.pathname + this.location.search);
        this.location.href = `/auth/login?returnUrl=${returnUrl}`;
    }

    private fetchAuthState(): Observable<boolean> {
        return this.http.get<AuthResponse>('/auth/user').pipe(
            map(response => {
                this.authState.set(response);
                return response.isAuthenticated;
            }),
            catchError((_: HttpErrorResponse) => {
                // Any error verifying auth state should be treated as unauthenticated.
                // This includes 401 (unauthorized), 500 (server error), network issues, etc.
                this.authState.set({
                    isAuthenticated: false,
                    claims: [],
                });
                return of(false);
            })
        );
    }

    private extractUserInfoFromClaims(claims: {type: string; value: string}[]): UserInfo {
        const givenName = claims.find(c => c.type === AuthService.CLAIM_NAME_GIVEN_NAME)?.value || '';
        const familyName = claims.find(c => c.type === AuthService.CLAIM_NAME_FAMILY_NAME)?.value || '';
        const email = claims.find(c => c.type === AuthService.CLAIM_NAME_EMAIL)?.value || '';
        const roles = claims.filter(c => c.type === AuthService.CLAIM_NAME_ROLE).map(claim => claim.value);

        return {
            name: `${givenName} ${familyName}`.trim(),
            email,
            roles,
        };
    }
}
