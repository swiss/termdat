import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {MultiEditAction} from 'api/MultiEditAction';
import {MockComponent} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {CollectionService} from '../../../administration/collection/collection.service';
import {EntryService} from '../../../entry/entry.service';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipComponent} from '../multi-edit-chip.component';
import {MultiEditChipCollectionsComponent} from './multi-edit-chip-collections.component';

describe('MultiEditChipCollectionsComponent', () => {
    let component: MultiEditChipCollectionsComponent;
    let fixture: ComponentFixture<MultiEditChipCollectionsComponent>;
    let collections: string[] = [];

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const entryServiceMock = {
        updateMultipleRelatedCollections: jest.fn(),
    };
    const collectionServiceMock = {
        getModificationCollections: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        warning: jest.fn(),
        error: jest.fn(),
    };

    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MultiEditChipCollectionsComponent, MockComponent(MultiEditChipComponent)],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
            ],
        }).compileComponents();
        collections = ['A', 'B', 'C'];
        entryServiceMock.updateMultipleRelatedCollections.mockReturnValue(of({entryCount: 12, affectedOccurrences: 1, dataWasChanged: true, lockInfos: []}));

        collectionServiceMock.getModificationCollections.mockReturnValue(of(collections));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditChipCollectionsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call update service for remove', async () => {
        await fixture.whenStable();

        component.ids = [1, 2, 3];
        component.remove();

        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: [],
            action: MultiEditAction.Remove,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for add', async () => {
        await fixture.whenStable();

        component.ids = [1, 2, 3];
        component.add(['4, 5']);

        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: ['4, 5'],
            action: MultiEditAction.Add,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for set', async () => {
        await fixture.whenStable();

        component.ids = [1, 2, 3];
        component.set(['4, 5']);

        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleRelatedCollections).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: ['4, 5'],
            action: MultiEditAction.Set,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show an error when service throws error', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleRelatedCollections.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.set(['4, 5']);

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleRelatedCollections.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.remove();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });
});
