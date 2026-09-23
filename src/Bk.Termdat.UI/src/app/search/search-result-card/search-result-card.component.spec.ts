import {ComponentFixture, fakeAsync, TestBed, tick} from '@angular/core/testing';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {RouterModule} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObIconModule} from '@oblique/oblique';
import {EntrySummary} from 'api/EntrySummary';
import {Status} from 'api/Status';
import {MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject} from 'rxjs';
import {first} from 'rxjs/operators';
import {AuthService} from '../../auth/auth.service';
import {WatchlistService} from '../../watchlist/watchlist.service';
import {SearchResultCardComponent} from './search-result-card.component';

/* eslint-disable dot-notation */
describe('SearchResultCardComponent', () => {
    let component: SearchResultCardComponent;
    let fixture: ComponentFixture<SearchResultCardComponent>;

    const isWatchlistItemMockSubject = new BehaviorSubject<boolean>(true);

    const watchlistServiceMock = {
        isWatchlistItem: jest.fn(() => isWatchlistItemMockSubject.asObservable()),
        add: jest.fn(),
        remove: jest.fn(),
    };

    const isAuthSubject = new BehaviorSubject(true);
    const authServiceMock = {
        isAuthenticated$: isAuthSubject.asObservable(),
    };

    const searchResultAch: EntrySummary = {
        id: 1,
        status: Status.Validated,
        deleted: false,
        isMainOffice: true,
        canBeAddedToWatchList: true,
        collection: {
            id: 1,
            code: 'COL',
            name: 'Collection',
            isObsolete: false,
        },
        terms: [
            {
                languageId: 1,
                terminus: 'Wert',
                sequence: 1,
            },
            {
                languageId: 2,
                terminus: 'Value',
                sequence: 2,
            },
        ],
    } as EntrySummary;

    const searchResultNotAch: EntrySummary = {
        id: 2,
        status: Status.InProgress,
        deleted: false,
        isMainOffice: false,
        canBeAddedToWatchList: false,
        collection: {
            id: 1,
            code: 'COL',
            name: 'Collection',
            isObsolete: false,
        },
        terms: [
            {
                languageId: 1,
                terminus: 'Wert',
                sequence: 1,
            },
            {
                languageId: 2,
                terminus: 'Value',
                sequence: 2,
            },
        ],
    } as EntrySummary;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                RouterModule,
                MockModule(ObIconModule),
                MockModule(MatIconModule),
                MockModule(MatTooltipModule),
                MockModule(MatCheckboxModule),
                SearchResultCardComponent,
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: AuthService, useValue: authServiceMock},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(SearchResultCardComponent);
        component = fixture.componentInstance;
        component.result = searchResultAch;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should mark main office ACH correctly', () => {
        expect(component.isMainOffice).toBe(true);
    });

    it('should mark entry as validated', () => {
        expect(component.isValidated).toBe(true);
    });

    it('should enable permission to add to watchlist correctly', () => {
        expect(component.canBeAddedToWatchList).toBe(true);
    });

    it('should mark other office as not ACH', () => {
        component.result = searchResultNotAch;
        expect(component.isMainOffice).toBe(false);
    });

    it('should mark entry as non validated', () => {
        component.result = searchResultNotAch;
        expect(component.isValidated).toBe(false);
    });

    it('should enable permission to add to watchlist correctly', () => {
        component.result = searchResultNotAch;
        expect(component.canBeAddedToWatchList).toBe(false);
    });

    it('should have correct routerLinkActiveOptions', () => {
        expect(component.routerLinkActiveOptions).toEqual({
            queryParams: 'ignored',
            fragment: 'ignored',
            paths: 'subset',
            matrixParams: 'subset',
        });
    });

    it('should unsubscribe on destroy', () => {
        jest.spyOn(component['unsubscribe'], 'next');
        jest.spyOn(component['unsubscribe'], 'complete');

        component.ngOnDestroy();

        expect(component['unsubscribe'].next).toHaveBeenCalled();
        expect(component['unsubscribe'].complete).toHaveBeenCalled();
    });

    it('should set authentication status', done => {
        component.isAuthenticated$.pipe(first()).subscribe(isAuth => {
            expect(isAuth).toBe(true);
        });

        isAuthSubject.next(false);

        component.isAuthenticated$.pipe(first()).subscribe(isAuth => {
            expect(isAuth).toBe(false);
            done();
        });
    });

    it('should unsubscribe and resubscribe when changing the entry', fakeAsync(() => {
        jest.spyOn(component['unsubscribe'], 'next');

        component.result = searchResultAch;

        expect(component['unsubscribe'].next).toHaveBeenCalled();

        // isAuth and isWatchlistItem
        isAuthSubject.next(true);
        isWatchlistItemMockSubject.next(true);

        tick(300);
        fixture.whenStable();
        expect(watchlistServiceMock.isWatchlistItem).toHaveBeenCalled();
        expect(component.isWatchlistItem).toBe(true);

        // isAuth and not isWatchlistItem
        isWatchlistItemMockSubject.next(false);
        tick(300);
        fixture.whenStable();
        expect(component.isWatchlistItem).toBe(false);

        // not isAuth
        watchlistServiceMock.isWatchlistItem.mockClear();
        isAuthSubject.next(false);
        isWatchlistItemMockSubject.next(true);

        tick(300);
        fixture.whenStable();
        expect(watchlistServiceMock.isWatchlistItem).not.toHaveBeenCalled();
        expect(component.isWatchlistItem).toBe(false);
    }));

    it('should add to watchlist on toggle', () => {
        component.isWatchlistItem = false;

        component.toggleWatchlistItem();

        expect(watchlistServiceMock.add).toHaveBeenCalled();
        expect(watchlistServiceMock.remove).not.toHaveBeenCalled();
    });

    it('should remove from watchlist on toggle', () => {
        component.isWatchlistItem = true;

        component.toggleWatchlistItem();

        expect(watchlistServiceMock.add).not.toHaveBeenCalled();
        expect(watchlistServiceMock.remove).toHaveBeenCalled();
    });
});
