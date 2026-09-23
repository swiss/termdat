import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {MultiEditAction} from 'api/MultiEditAction';
import {MockComponent} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {GlossaryService} from '../../../administration/glossary/glossary.service';
import {EntryService} from '../../../entry/entry.service';
import {WatchlistService} from '../../watchlist.service';
import {MultiEditChipComponent} from '../multi-edit-chip.component';
import {MultiEditChipGlossariesComponent} from './multi-edit-chip-glossaries.component';

describe('MultiEditChipGlossariesComponent', () => {
    let component: MultiEditChipGlossariesComponent;
    let fixture: ComponentFixture<MultiEditChipGlossariesComponent>;
    let glossaries: string[] = [];

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const entryServiceMock = {
        updateMultipleGlossaries: jest.fn(),
    };
    const glossaryServiceMock = {
        getModificationGlossaries: jest.fn(),
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
            imports: [MultiEditChipGlossariesComponent, MockComponent(MultiEditChipComponent)],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: GlossaryService, useValue: glossaryServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
            ],
        }).compileComponents();
        glossaries = ['A', 'B', 'C'];
        entryServiceMock.updateMultipleGlossaries.mockReturnValue(of({entryCount: 12, affectedOccurrences: 1, dataWasChanged: true, lockInfos: []}));
        glossaryServiceMock.getModificationGlossaries.mockReturnValue(of(glossaries));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditChipGlossariesComponent);
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

        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledWith({
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

        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledWith({
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

        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleGlossaries).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            codes: ['4, 5'],
            action: MultiEditAction.Set,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show an error when service throws error', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleGlossaries.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.set(['4, 5']);

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleGlossaries.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.remove();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });
});
