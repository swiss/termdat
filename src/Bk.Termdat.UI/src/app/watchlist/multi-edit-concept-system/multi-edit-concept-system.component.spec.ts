import {Location} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonModule, ObNotificationService} from '@oblique/oblique';
import {MockComponent, MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {MultiEditAction} from '../../../api/MultiEditAction';
import {EntryService} from '../../entry/entry.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {WatchlistService} from '../watchlist.service';
import {MultiEditConceptSystemComponent} from './multi-edit-concept-system.component';

describe('MultiEditConceptSystemComponent', () => {
    let component: MultiEditConceptSystemComponent;
    let fixture: ComponentFixture<MultiEditConceptSystemComponent>;

    const entryServiceMock = {
        getConceptSystemPositions: jest.fn(),
        updateMultipleConceptSystemPosition: jest.fn(),
    };

    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
        instant: jest.fn(),
    };

    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    const locationMock = {
        getState: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MultiEditConceptSystemComponent,
                MockModule(MatInputModule),
                MockModule(MatButtonModule),
                MockModule(MatFormFieldModule),
                MockModule(ObButtonModule),
                MockModule(MatIconModule),
                ReactiveFormsModule,
                MockPipe(TranslatePipe),
                MockDirective(RouterLink),
                MockComponent(AutocompleteComponent),
            ],
            providers: [
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: Location, useValue: locationMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(MultiEditConceptSystemComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        const conceptSystemPositions = [
            {id: 1, value: 'ConceptSystem1'},
            {id: 2, value: 'ConceptSystem2'},
        ];
        entryServiceMock.getConceptSystemPositions.mockReturnValue(of(conceptSystemPositions));
        entryServiceMock.updateMultipleConceptSystemPosition.mockReturnValue(of({entryCount: 12, affectedOccurrences: 1, dataWasChanged: true, lockInfos: []}));
        locationMock.getState.mockReturnValue({ids: [1, 2, 3], collectionId: 4});
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set Ids and collection Id', () => {
        expect(component.idsToModify).toStrictEqual([1, 2, 3]);
        expect(component.collectionId).toBe(4);
    });

    it('should load concept system position list', () => {
        expect(component.conceptSystemPositions.length).toBe(2);
    });

    it('should call update service for remove', async () => {
        await fixture.whenStable();

        component.ngOnInit();
        component.remove();

        expect(entryServiceMock.updateMultipleConceptSystemPosition).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleConceptSystemPosition).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            conceptSystemPositionId: null,
            action: MultiEditAction.Remove,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for set', async () => {
        await fixture.whenStable();
        component.ngOnInit();

        component.form.controls.conceptSystemPosition.setValue(4);

        component.set();

        expect(entryServiceMock.updateMultipleConceptSystemPosition).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleConceptSystemPosition).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            conceptSystemPositionId: 4,
            action: MultiEditAction.Set,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleConceptSystemPosition.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.remove();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });

    it('should error when in case of other exception', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleConceptSystemPosition.mockReturnValueOnce(throwError(() => 'Error'));

        component.remove();

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });
});
