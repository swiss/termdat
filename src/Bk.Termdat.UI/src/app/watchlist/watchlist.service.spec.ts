import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {EntryMultipleUpdateInfo} from 'api/EntryMultipleUpdateInfo';
import {skip} from 'rxjs/operators';
import * as downloadHelper from '../shared/download.helper';
import {LOCAL_STORAGE} from '../shared/injection.tokens';
import {WatchlistService} from './watchlist.service';

describe('WatchlistService', () => {
    let service: WatchlistService;
    let httpMock: HttpTestingController;

    const watchlistLocalStorageName = 'Watchlist';
    const selectedWatchlistItemsLocalStorageName = 'SelectedWatchlistItems';

    describe('WatchlistService.HasValues', () => {
        const localStorageMock = {
            getItem: jest.fn(() => '1,2,3'),
            setItem: jest.fn(),
            removeItem: jest.fn(),
        };

        const notificationServiceMock = {
            warning: jest.fn(),
        };

        const translateServiceMock = {
            instant: jest.fn(),
            getCurrentLang: jest.fn(() => 'de'),
        };

        beforeEach(() => {
            TestBed.configureTestingModule({
                imports: [],
                providers: [
                    {provide: LOCAL_STORAGE, useValue: localStorageMock},
                    {provide: ObNotificationService, useValue: notificationServiceMock},
                    {provide: TranslateService, useValue: translateServiceMock},
                    provideHttpClient(withInterceptorsFromDi()),
                    provideHttpClientTesting(),
                ],
            });

            service = TestBed.inject(WatchlistService);
            httpMock = TestBed.inject(HttpTestingController);
        });

        afterEach(() => {
            jest.clearAllMocks();
        });

        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should get entries', () => {
            const entryIds = [1, 2, 3];

            service.getEntries(entryIds).subscribe();

            const req = httpMock.expectOne(`/api/watchlist`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toMatchObject({entryIds});

            req.flush(['test']);
            httpMock.verify();
        });

        it('should return entries', done => {
            service.watchlist$.subscribe(watchlist => {
                expect(watchlist).toMatchObject([1, 2, 3]);

                expect(localStorageMock.getItem).toHaveBeenCalledTimes(1);
                expect(localStorageMock.getItem).toHaveBeenCalledWith(watchlistLocalStorageName);

                done();
            });
        });

        it.each([
            [1, true],
            [3, true],
            [4, false],
            [NaN, false],
        ])('should return if id is in watchlist', (id: number, expected: boolean) => {
            let isWatchlistItem;
            service.isWatchlistItem(id).subscribe(value => (isWatchlistItem = value));

            expect(isWatchlistItem).toBe(expected);
        });

        it.each([
            [[1], '1'],
            [[1, 2, 5], '1,2,5'],
            [[9999, 1234567890], '9999,1234567890'],
        ])('should add to watchlist', (idsToAdd: number[], expected: string) => {
            localStorageMock.getItem.mockReturnValue('');

            service.watchlist$
                .pipe(skip(1)) // Skip the watchlist emmited by constructor()
                .subscribe(watchlist => {
                    idsToAdd.every(id => expect(watchlist).toContain(id));
                });

            service.add(...idsToAdd);

            expect(localStorageMock.setItem).toHaveBeenCalledTimes(1);
            expect(localStorageMock.setItem).toHaveBeenCalledWith(watchlistLocalStorageName, expected);
        });

        it.each([
            [[1], '2,3'],
            [[1, 2, 2, 5], '3'],
            [[9999, 1234567890], '1,2,3,1'],
            [[1, 2, 3], ''],
        ])('should remove from watchlist', (idsToRemove: number[], expected: string) => {
            localStorageMock.getItem.mockReturnValue('1,2,3,1, non');

            service.watchlist$
                .pipe(skip(1)) // Skip the watchlist emmited by constructor()
                .subscribe(watchlist => {
                    idsToRemove.every(id => expect(watchlist).not.toContain(id));
                });

            service.remove(...idsToRemove);

            expect(localStorageMock.setItem).toHaveBeenCalledTimes(1);
            expect(localStorageMock.setItem).toHaveBeenCalledWith(watchlistLocalStorageName, expected);
            expect(localStorageMock.removeItem).toHaveBeenCalledTimes(1);
            expect(localStorageMock.removeItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
        });

        it('should remove all items from watchlist', () => {
            service.watchlist$
                .pipe(skip(1)) // Skip the watchlist emmited by constructor()
                .subscribe(watchlist => {
                    expect(watchlist.length).toBe(0);
                });

            service.removeAll();

            expect(localStorageMock.removeItem).toHaveBeenCalledTimes(1);
            expect(localStorageMock.removeItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
        });

        it('should save selected items to localStorage on updateSelectedItems', () => {
            const ids = [8523, 2386, 8249];
            const idsAsString = ids.join(',');

            service.updateSelectedItems(ids);

            expect(localStorageMock.setItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName, idsAsString);
            expect(localStorageMock.setItem).toHaveBeenCalledTimes(1);
        });

        it('should return ids on readSelectedItems', () => {
            const ids = [8523, 2386, 8249];
            const idsAsString = ids.join(',');

            localStorageMock.getItem.mockReturnValue(idsAsString);

            const receivedIds = service.readSelectedItems();

            expect(localStorageMock.getItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
            expect(receivedIds).toEqual(ids);
        });

        it('should return integer ids when entering decimal ids on readSelectedItems', () => {
            const decimalIds = [8523.789, 2386.123, 8249.456];
            const integerIds = [8523, 2386, 8249];
            const decimalIdsAsString = decimalIds.join(',');

            localStorageMock.getItem.mockReturnValue(decimalIdsAsString);

            const receivedIds = service.readSelectedItems();

            expect(localStorageMock.getItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
            expect(receivedIds).toEqual(integerIds);
        });

        it('should return empty array when entering string instead of ids on readSelectedItems', () => {
            const ids = 'HALLO';
            const expected: number[] = [];

            localStorageMock.getItem.mockReturnValue(ids);

            const receivedIds = service.readSelectedItems();

            expect(localStorageMock.getItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
            expect(receivedIds).toEqual(expected);
        });

        it('should return empty array when entering undefined instead of ids on readSelectedItems', () => {
            const ids = undefined;
            const expected: number[] = [];

            localStorageMock.getItem.mockReturnValue(ids!);

            const receivedIds = service.readSelectedItems();

            expect(localStorageMock.getItem).toHaveBeenCalledWith(selectedWatchlistItemsLocalStorageName);
            expect(receivedIds).toEqual(expected);
        });
    });

    describe('WatchlistService.HasNoValues', () => {
        const localStorageMock = {
            getItem: jest.fn(() => undefined as string | undefined),
            setItem: jest.fn(),
        };

        const notificationServiceMock = {
            warning: jest.fn(),
        };

        const translateServiceMock = {
            instant: jest.fn(),
            getCurrentLang: jest.fn(() => 'de'),
        };

        beforeEach(() => {
            TestBed.configureTestingModule({
                imports: [],
                providers: [
                    {provide: LOCAL_STORAGE, useValue: localStorageMock},
                    {provide: ObNotificationService, useValue: notificationServiceMock},
                    {provide: TranslateService, useValue: translateServiceMock},
                    provideHttpClient(withInterceptorsFromDi()),
                    provideHttpClientTesting(),
                ],
            });

            service = TestBed.inject(WatchlistService);
        });

        afterEach(() => {
            jest.clearAllMocks();
        });

        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should return empty array on missing watchlist', done => {
            service.watchlist$.subscribe(watchlist => {
                expect(watchlist).toMatchObject([]);

                expect(localStorageMock.getItem).toHaveBeenCalledTimes(1);
                expect(localStorageMock.getItem).toHaveBeenCalledWith(watchlistLocalStorageName);

                done();
            });
        });
    });

    describe('WatchlistService.Export', () => {
        const localStorageMock = {
            getItem: jest.fn(),
            setItem: jest.fn(),
        };

        const notificationServiceMock = {
            warning: jest.fn(),
        };

        const translateServiceMock = {
            instant: jest.fn(),
            getCurrentLang: jest.fn(() => 'de'),
        };

        beforeEach(() => {
            TestBed.configureTestingModule({
                imports: [],
                providers: [
                    {provide: LOCAL_STORAGE, useValue: localStorageMock},
                    {provide: ObNotificationService, useValue: notificationServiceMock},
                    {provide: TranslateService, useValue: translateServiceMock},
                    provideHttpClient(withInterceptorsFromDi()),
                    provideHttpClientTesting(),
                ],
            });

            service = TestBed.inject(WatchlistService);
            httpMock = TestBed.inject(HttpTestingController);
        });

        afterEach(() => {
            httpMock.verify();
            jest.clearAllMocks();
        });

        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should request pdf export', done => {
            service.createExportPdf([1, 2, 3]).subscribe(() => {
                done();
            });

            const req = httpMock.expectOne({method: 'POST', url: '/api/watchlist/export:pdf'});
            expect(req.request.body).toEqual({entryIds: [1, 2, 3]});

            req.flush(new Blob());
        });

        it('should request xlsx export', done => {
            service.createExportXlsx([1, 2, 3]).subscribe(() => {
                done();
            });

            const req = httpMock.expectOne({method: 'POST', url: '/api/watchlist/export:xlsx'});
            expect(req.request.body).toEqual({entryIds: [1, 2, 3]});

            req.flush(new Blob());
        });

        it('should export entry ids to txt', () => {
            const downloadTxtFileSpy = jest.spyOn(downloadHelper, 'downloadTxtFile').mockImplementation(() => undefined);
            translateServiceMock.instant.mockReturnValue('Eintrags-IDs_20260702.txt');

            service.createExportTxt([1, 2, 3]);

            expect(translateServiceMock.instant).toHaveBeenCalledTimes(1);
            expect(translateServiceMock.instant).toHaveBeenCalledWith(
                'watchList.exportEntryIdsTxtFilename',
                expect.objectContaining({date: expect.stringMatching(/^\d{8}$/)})
            );
            expect(downloadTxtFileSpy).toHaveBeenCalledTimes(1);
            expect(downloadTxtFileSpy).toHaveBeenCalledWith('1,2,3', 'Eintrags-IDs_20260702.txt');
        });
    });

    describe('WatchlistService.DisplayUpdateInfoWarning', () => {
        const localStorageMock = {
            getItem: jest.fn(() => '1,2,3'),
        };

        const notificationServiceMock = {
            warning: jest.fn(),
        };

        const translateServiceMock = {
            instant: jest.fn(),
            getCurrentLang: jest.fn(() => 'de'),
        };

        beforeEach(() => {
            TestBed.configureTestingModule({
                imports: [],
                providers: [
                    {provide: LOCAL_STORAGE, useValue: localStorageMock},
                    {provide: ObNotificationService, useValue: notificationServiceMock},
                    {provide: TranslateService, useValue: translateServiceMock},
                    provideHttpClient(withInterceptorsFromDi()),
                    provideHttpClientTesting(),
                ],
            });

            service = TestBed.inject(WatchlistService);
            httpMock = TestBed.inject(HttpTestingController);
        });

        afterEach(() => {
            jest.clearAllMocks();
        });

        it('should show warning for locked entries', () => {
            const infos: EntryMultipleUpdateInfo[] = [
                {
                    lockedEntryId: 7,
                    lockedBy: 'test_user',
                },
                {
                    lockedEntryId: 42,
                    lockedBy: 'test_user',
                },
            ];
            const lockedEntriesMessage = 'test-locked-entries-message';
            translateServiceMock.instant.mockReturnValue(lockedEntriesMessage);

            service.displayUpdateInfoWarning(infos, 'some-key');

            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.warning).toHaveBeenCalledWith({
                message: `${lockedEntriesMessage}\n\n7: test_user;\n42: test_user;`,
                timeout: 15000,
            });
        });

        it('should show warning for not-soft-deleted entries', () => {
            const infos: EntryMultipleUpdateInfo[] = [
                {
                    notSoftDeletedEntryId: 12,
                },
            ];
            const notSoftDeletedEntriesMessage = 'test-not-soft-deleted-entries-message';
            translateServiceMock.instant.mockReturnValue(notSoftDeletedEntriesMessage);

            service.displayUpdateInfoWarning(infos, 'some-key', 'some-other-key');

            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
            expect(notificationServiceMock.warning).toHaveBeenCalledWith({
                message: `${notSoftDeletedEntriesMessage}\n\n12;`,
                timeout: 15000,
            });
        });

        it('should show warning for locked entries and not-soft-deleted entries', () => {
            const infos: EntryMultipleUpdateInfo[] = [
                {
                    lockedEntryId: 42,
                    lockedBy: 'test-user',
                },
                {
                    notSoftDeletedEntryId: 12,
                },
            ];
            translateServiceMock.instant.mockReturnValue('some-text');

            service.displayUpdateInfoWarning(infos, 'some-key', 'some-other-key');

            expect(notificationServiceMock.warning).toHaveBeenCalledTimes(2);
        });

        it('should not show warning when not-soft-deleted-message is missing', () => {
            const infos: EntryMultipleUpdateInfo[] = [
                {
                    notSoftDeletedEntryId: 12,
                },
            ];
            const notSoftDeletedEntriesMessage = 'test-not-soft-deleted-entries-message';
            translateServiceMock.instant.mockReturnValue(notSoftDeletedEntriesMessage);

            service.displayUpdateInfoWarning(infos, 'some-key');

            expect(notificationServiceMock.warning).not.toHaveBeenCalled();
        });
    });
});
