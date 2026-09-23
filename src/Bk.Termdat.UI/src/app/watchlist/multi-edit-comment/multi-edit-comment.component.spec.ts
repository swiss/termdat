import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {Router, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonModule, ObNotificationService} from '@oblique/oblique';
import {MultiEditAction} from 'api/MultiEditAction';
import {MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of} from 'rxjs';
import {EntryService} from '../../entry/entry.service';
import {WatchlistService} from '../watchlist.service';
import {MultiEditCommentComponent} from './multi-edit-comment.component';

describe('MultiEditCommentComponent', () => {
    let component: MultiEditCommentComponent;
    let fixture: ComponentFixture<MultiEditCommentComponent>;

    const entryServiceMock = {
        updateMultipleComment: jest.fn(),
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

    const translateServiceMock = {
        instant: jest.fn(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            providers: [
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: Location, useValue: locationMock},
            ],
            imports: [
                MockModule(MatInputModule),
                MockModule(MatButtonModule),
                MockModule(MatFormFieldModule),
                MockModule(ObButtonModule),
                MockModule(MatIconModule),
                ReactiveFormsModule,
                MultiEditCommentComponent,
                MockPipe(TranslatePipe),
                MockDirective(RouterLink),
            ],
        }).compileComponents();
        entryServiceMock.updateMultipleComment.mockReturnValue(of({entryCount: 12, affectedOccurrences: 1, dataWasChanged: true, lockInfos: []}));
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditCommentComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should call update service for remove', async () => {
        await fixture.whenStable();

        component.idsToModify = [1, 2, 3];
        component.remove();

        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            comment: null,
            action: MultiEditAction.Remove,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for add', async () => {
        await fixture.whenStable();

        component.form.controls.comment.setValue('I am digging in the dirt');
        component.idsToModify = [2, 3];
        component.add();

        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledWith({
            ids: [2, 3],
            comment: 'I am digging in the dirt',
            action: MultiEditAction.Add,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should call update service for set', async () => {
        await fixture.whenStable();

        component.form.controls.comment.setValue('Digging in the dirt');
        component.idsToModify = [1, 2, 3];
        component.set();

        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMultipleComment).toHaveBeenCalledWith({
            ids: [1, 2, 3],
            comment: 'Digging in the dirt',
            action: MultiEditAction.Set,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMultipleComment.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.remove();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });
});
