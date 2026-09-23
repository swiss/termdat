import {ChangeDetectorRef} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSelectChange} from '@angular/material/select';
import {ActivatedRoute, convertToParamMap} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryDetail} from 'api/EntryDetail';
import {Status} from 'api/Status';
import {MockComponent, MockModule} from 'ng-mocks';
import {BehaviorSubject, of, Subject, throwError} from 'rxjs';
import {AuthService} from '../../auth/auth.service';
import {Role} from '../../auth/Role';
import {SearchService} from '../../search/search.service';
import {EntryButtonListComponent} from '../entry-button-list/entry-button-list.component';
import {EntryHeaderComponent} from '../entry-header/entry-header.component';
import {EntryService} from '../entry.service';
import {DisplayService} from './entry-detail/display.service';
import {EntryDetailComponent} from './entry-detail/entry-detail.component';
import {EntryDetailFormComponent} from './entry-detail-form.component';
import {ReferenceListComponent} from './reference-list/reference-list.component';

describe('EntryDetailFormComponent', () => {
    let component: EntryDetailFormComponent;
    let fixture: ComponentFixture<EntryDetailFormComponent>;

    let entryDetail: EntryDetail;

    const entrySelectedSubject = new Subject();
    const entryServiceMock = {
        get: jest.fn(),
        delete: jest.fn(),
        entrySelected$: entrySelectedSubject.asObservable(),
    };
    const roles: Role[] = [];
    const roleSubject = new BehaviorSubject(roles);
    const authServiceMock = {
        roles$: roleSubject.asObservable(),
    };
    const displayServiceMock = {
        setDisplayMode: jest.fn(),
        getDisplayMode: jest.fn(),
    };

    const paramMapSubject = new Subject();
    const activatedRouteMock = {
        paramMap: paramMapSubject.asObservable(),
        snapshot: {
            queryParamMap: convertToParamMap({p: '1', s: 'term'}),
        },
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        instant: jest.fn(),
    };

    const searchServiceMock = {
        getSourceLanguages: jest.fn(),
        getTargetLanguages: jest.fn(),
    };
    const changeDetectorRefMock = {
        detectChanges: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatButtonModule),
                MockModule(MatCheckboxModule),
                MockModule(TranslateModule),
                MockModule(MatOptionModule),
                MockModule(MatFormFieldModule),
                EntryDetailFormComponent,
                MockComponent(EntryHeaderComponent),
                MockComponent(EntryDetailComponent),
                MockComponent(EntryButtonListComponent),
                MockComponent(ReferenceListComponent),
            ],
            providers: [
                {provide: ChangeDetectorRef, useValue: changeDetectorRefMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: AuthService, useValue: authServiceMock},
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: SearchService, useValue: searchServiceMock},
                {provide: DisplayService, useValue: displayServiceMock},
            ],
        }).compileComponents();

        entryDetail = {
            id: 1,
            status: Status.Validated,
            collection: {
                id: 10,
                code: 'VAC1',
                name: 'Value added collection',
            } as CollectionSummary,
            languageDetails: [
                {
                    abbreviation: 'short',
                    definition: 'def',
                    languageId: 1,
                    name: 'Search Detail',
                    phraseology: 'phraseology',
                    sequence: 0,
                    terminus: 'term',
                },
            ],
        } as EntryDetail;

        roleSubject.next(roles);
        entryServiceMock.get.mockReturnValue(of(entryDetail));
        entryServiceMock.delete.mockReturnValue(of({}));
        searchServiceMock.getSourceLanguages.mockReturnValue([]);
        searchServiceMock.getTargetLanguages.mockReturnValue([]);
        displayServiceMock.getDisplayMode.mockReturnValue('complete');
        displayServiceMock.setDisplayMode.mockReturnValue('complete');

        fixture = TestBed.createComponent(EntryDetailFormComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();

        paramMapSubject.next(convertToParamMap({id: '1'}));
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load entry from activated route param', () => {
        expect(component.currentEntry).toBeTruthy();
        expect(component.currentEntry).toEqual(entryDetail);
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);
    });

    it('should not load same entry multiple times', () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        paramMapSubject.next(convertToParamMap({id: '1'}));

        paramMapSubject.next(convertToParamMap({id: '2'}));
        paramMapSubject.next(convertToParamMap({id: '2'}));
        paramMapSubject.next(convertToParamMap({id: '2'}));

        expect(entryServiceMock.get).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.get).toHaveBeenCalledWith(2, [], []);
    });

    it('should only load entries where id is number and greater than 0', () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        paramMapSubject.next(convertToParamMap({id: '0'}));

        paramMapSubject.next(convertToParamMap({id: -1}));

        paramMapSubject.next(convertToParamMap({id: 'abc'}));

        paramMapSubject.next(convertToParamMap({id: ''}));

        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        paramMapSubject.next(convertToParamMap({id: '   2    '}));

        paramMapSubject.next(convertToParamMap({id: '123efg'}));

        paramMapSubject.next(convertToParamMap({}));

        expect(entryServiceMock.get).toHaveBeenCalledTimes(3);
        expect(entryServiceMock.get).toHaveBeenCalledWith(2, [], []);
        expect(entryServiceMock.get).toHaveBeenCalledWith(123, [], []);
    });

    it('should reload entry on language change', () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        langChangeSubject.next(true);

        expect(entryServiceMock.get).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);
    });

    it('should reset entry on error', async () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(component.currentEntry).toBeTruthy();

        entryServiceMock.get.mockReturnValue(throwError(() => ({})));

        langChangeSubject.next(true);
        await fixture.whenStable();

        expect(component.currentEntry).toBeUndefined();
    });

    it('should set canCreateAndDeleteAndUpdate correctly for isAdmin', () => {
        expect(component.canCreateAndDeleteAndUpdate).toEqual(false);

        roleSubject.next([Role.Admin]);
        fixture = TestBed.createComponent(EntryDetailFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        expect(component.canCreateAndDeleteAndUpdate).toEqual(true);
    });

    it('should set canCreateAndDeleteAndUpdate correctly for isWriter', () => {
        expect(component.canCreateAndDeleteAndUpdate).toEqual(false);

        roleSubject.next([Role.Writer]);
        fixture = TestBed.createComponent(EntryDetailFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        expect(component.canCreateAndDeleteAndUpdate).toEqual(true);
    });

    it('should set canCreateAndDeleteAndUpdate correctly for isSuperUser', () => {
        expect(component.canCreateAndDeleteAndUpdate).toEqual(false);

        roleSubject.next([Role.SuperUser]);
        fixture = TestBed.createComponent(EntryDetailFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        expect(component.canCreateAndDeleteAndUpdate).toEqual(true);
    });

    it('should get source & target languages from search service when getting detail', () => {
        searchServiceMock.getTargetLanguages.mockReturnValue([1, 2]);
        searchServiceMock.getSourceLanguages.mockReturnValue([3, 4]);

        langChangeSubject.next(true);

        expect(entryServiceMock.get).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [3, 4], [1, 2]);

        expect(searchServiceMock.getTargetLanguages).toHaveBeenCalledTimes(2);
        expect(searchServiceMock.getSourceLanguages).toHaveBeenCalledTimes(2);
    });

    it('should read displayMode from storage', () => {
        component.ngOnInit();
        expect(displayServiceMock.getDisplayMode).toHaveBeenCalledTimes(2);
    });

    it('should write displayMode to storage', () => {
        const eventMock = {
            value: 'string',
        } as MatSelectChange;

        component.changeDisplayMode(eventMock);
        expect(displayServiceMock.setDisplayMode).toHaveBeenCalledTimes(1);
    });

    it('should get entry on entry selected', () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        entrySelectedSubject.next(1);

        expect(entryServiceMock.get).toHaveBeenCalledTimes(2);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);
    });

    it('should not get entry on selected when selected entry is different to current entry', () => {
        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.get).toHaveBeenCalledWith(1, [], []);

        entrySelectedSubject.next(2);

        expect(entryServiceMock.get).toHaveBeenCalledTimes(1);
    });

    it.each([
        [new Date('2023-05-25T12:20:00Z'), 5],
        [new Date('2023-05-25T11:20:00Z'), 65],
        [new Date('2023-05-24T11:20:00Z'), 1505],
        [new Date('2023-05-25T12:25:00Z'), 1],
        [new Date('2023-05-25T12:24:00Z'), 1],
        [new Date('2023-05-25T12:23:29Z'), 2],
        [new Date('2023-05-25T12:23:30Z'), 2],
        [new Date('2023-05-25T12:23:31Z'), 1],
        [new Date('2023-05-25T12:26:00Z'), 1],
    ])('should get minutes since entry was locked for %s', (date, expectedMinutes) => {
        jest.useFakeTimers();
        jest.setSystemTime(new Date('2023-05-25T12:25:00Z'));

        const minutes = component.getLockedMinutes(date);
        expect(minutes).toEqual(expectedMinutes);

        jest.useRealTimers();
    });
});
