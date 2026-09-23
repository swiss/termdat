import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatInputModule} from '@angular/material/input';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {AdminInfoSummary} from 'api/AdminInfoSummary';
import {MultiEditAction} from 'api/MultiEditAction';
import {MockComponent, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {EntryService} from '../../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../../search/extended-search-filters.service';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipInputComponent} from '../multi-edit-chip-input.component';
import {MultiEditAdminInfosComponent} from './multi-edit-admininfos.component';

describe('MultiEditAdminInfosComponent', () => {
    let component: MultiEditAdminInfosComponent;
    let fixture: ComponentFixture<MultiEditAdminInfosComponent>;
    let adminInfos: AdminInfoSummary[] = [];

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const entryServiceMock = {
        updateMultipleAdminInfos: jest.fn(),
    };
    const extendedSearchFiltersServiceMock = {
        getAdminInfos: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
    };

    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
            ],
            imports: [ReactiveFormsModule, MockModule(MatInputModule), MultiEditAdminInfosComponent, MockComponent(MultiEditChipInputComponent)],
        }).compileComponents();
        adminInfos = [{id: 1, code: 'info1'} as AdminInfoSummary, {id: 2, code: 'info2'} as AdminInfoSummary];
        entryServiceMock.updateMultipleAdminInfos.mockReturnValue(of({entryCount: 12, affectedOccurrences: 1, dataWasChanged: true, lockInfos: []}));
        extendedSearchFiltersServiceMock.getAdminInfos.mockReturnValue(of(adminInfos));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditAdminInfosComponent);
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

        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: [],
            action: MultiEditAction.Remove,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for add', async () => {
        await fixture.whenStable();

        component.ids = [1, 2, 3];
        component.add(adminInfos);

        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: [
                {
                    code: 'info1',
                    id: 1,
                },
                {
                    code: 'info2',
                    id: 2,
                },
            ],
            action: MultiEditAction.Add,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for set', async () => {
        await fixture.whenStable();

        component.ids = [1, 2, 3];
        component.set(adminInfos);

        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleAdminInfos).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: [
                {
                    code: 'info1',
                    id: 1,
                },
                {
                    code: 'info2',
                    id: 2,
                },
            ],
            action: MultiEditAction.Set,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show an error when service throws error', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleAdminInfos.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.set(adminInfos);

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleAdminInfos.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.remove();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });
});
