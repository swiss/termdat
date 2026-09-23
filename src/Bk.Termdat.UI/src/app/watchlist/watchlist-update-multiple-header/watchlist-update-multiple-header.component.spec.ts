import {Location} from '@angular/common';
import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, FormsModule, ReactiveFormsModule, ValidationErrors} from '@angular/forms';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatRadioModule} from '@angular/material/radio';
import {MatSelectModule} from '@angular/material/select';
import {BrowserModule} from '@angular/platform-browser';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObNotificationService, ObUnsavedChangesDirective} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryHeaderModification} from 'api/EntryHeaderModification';
import {OfficeSummary} from 'api/OfficeSummary';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {MockComponents, MockDirectives, MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {CollectionService} from '../../administration/collection/collection.service';
import {EntryService} from '../../entry/entry.service';
import {ExtendedSearchFiltersService} from '../../search/extended-search-filters.service';
import {AutocompleteComponent} from '../../shared/autocomplete/autocomplete.component';
import {ErrorService} from '../../shared/error.service';
import {Option} from '../../shared/Option';
import {SubjectFormComponent} from '../../shared/subject-form/subject-form.component';
import {WatchlistService} from '../watchlist.service';
import {WatchlistUpdateMultipleHeaderComponent} from './watchlist-update-multiple-header.component';

describe('WatchlistUpdateMultipleHeaderComponent', () => {
    let formBuilder: FormBuilder;
    let component: WatchlistUpdateMultipleHeaderComponent;
    let fixture: ComponentFixture<WatchlistUpdateMultipleHeaderComponent>;
    let entry: EntryHeaderModification;
    let collections: CollectionSummary[] = [];
    let offices: OfficeSummary[] = [];
    let states: Option[] = [];
    let reliabilities: Option[] = [];

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: onLangChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const extendedSearchFiltersServiceMock = {
        getOffices: jest.fn(),
        getStates: jest.fn(),
    };
    const httpApiInterceptorEventsMock = {
        deactivateNotificationOnNextAPICalls: jest.fn(),
    };
    const entryServiceMock = {
        getModification: jest.fn(),
        getReliabilities: jest.fn(),
        updateMandatoryHeaderFields: jest.fn(),
    };
    const collectionServiceMock = {
        getModificationCollections: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };
    const routeMock = {
        paramMap: of({get: jest.fn()}),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
        getControlError: jest.fn(),
    };
    const notificationServiceMock = {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
    };
    const locationMock = {
        getState: jest.fn(),
    };
    const watchlistServiceMock = {
        displayUpdateInfoWarning: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                FormsModule,
                BrowserModule,
                MockModule(TranslateModule),
                MockModule(MatIconModule),
                MockModule(MatOptionModule),
                MockModule(MatFormFieldModule),
                MockModule(MatInputModule),
                MockModule(MatSelectModule),
                MockModule(MatRadioModule),
                WatchlistUpdateMultipleHeaderComponent,
                MockComponents(AutocompleteComponent, SubjectFormComponent),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFiltersServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: Router, useValue: routerMock},
                {provide: ActivatedRoute, useValue: routeMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: WatchlistService, useValue: watchlistServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
                {provide: Location, useValue: locationMock},
                provideHttpClient(withInterceptorsFromDi()),
            ],
        }).compileComponents();
        collectionServiceMock.getModificationCollections.mockReturnValue(of({}));
        translateServiceMock.instant.mockImplementation(input => input);
        entryServiceMock.updateMandatoryHeaderFields.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: true, lockInfos: []}));
        collections = [{id: 1, name: 'Collection1', code: 'COL1'} as CollectionSummary, {id: 2, name: 'Collection2', code: 'COL2'} as CollectionSummary];
        offices = [
            {id: 1, name: 'Office1', code: 'OFF1'},
            {id: 2, name: 'Office2', code: 'OFF2'},
        ];
        states = [{id: 1, name: 'State1', code: 'ST1'} as Option, {id: 2, name: 'State2', code: 'ST2'} as Option];
        reliabilities = [{id: 1, name: 'Reliability1', code: 'R1'} as Option, {id: 2, name: 'Reliability2', code: 'R2'} as Option];
        entry = {
            ids: [1, 2, 3],
            status: Status.Validated,
            reliability: Reliability.Approved,
            officeId: 1,
            collectionId: 2,
        };
        errorServiceMock.getControlError.mockImplementation((errors: ValidationErrors | null, label: string, labelPrefix: string) => {
            if (errors === null) {
                return '';
            }

            const errorKeys = Object.keys(errors);
            return `${labelPrefix}.${errorKeys[0]}`;
        });
        extendedSearchFiltersServiceMock.getOffices.mockReturnValue(of(offices));
        extendedSearchFiltersServiceMock.getStates.mockReturnValue(of(states));
        collectionServiceMock.getModificationCollections.mockReturnValue(of(collections));
        entryServiceMock.getReliabilities.mockReturnValue(of(reliabilities));
        locationMock.getState.mockReturnValue({ids: [1, 2, 3, 4, 5]});

        entryServiceMock.getModification.mockReturnValue(of(entry));
    });

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [WatchlistUpdateMultipleHeaderComponent],
        }).compileComponents();
    });

    beforeEach(() => {
        formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(WatchlistUpdateMultipleHeaderComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should navigate back to watchlist if ids are empty', () => {
        locationMock.getState.mockReturnValue({});

        component.ngOnInit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist']);
    });

    it('can update an entry', async () => {
        await fixture.whenStable();

        const subjectDomainsData = [
            {subject: 1, domains: [11, 12]},
            {subject: 2, domains: [21, 22]},
        ];

        component.form.controls.state.setValue('0');
        component.form.controls.office.setValue('12');
        component.form.controls.reliability.setValue('3');
        component.form.controls.collection.setValue('87');

        const subjectDomainsArray = component.form.controls.subjectDomains as FormArray;
        subjectDomainsArray.clear();

        subjectDomainsData.forEach(item => {
            subjectDomainsArray.push(
                formBuilder.group({
                    subject: item.subject,
                    domains: formBuilder.control(item.domains),
                })
            );
        });

        component.save();

        expect(httpApiInterceptorEventsMock.deactivateNotificationOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMandatoryHeaderFields).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.updateMandatoryHeaderFields).toHaveBeenCalledWith({
            ids: [1, 2, 3, 4, 5],
            status: '0',
            reliability: '3',
            officeId: '12',
            collectionId: '87',
            subjectIds: [1, 2],
            domainIds: [11, 12, 21, 22],
            replaceSubjects: false,
        });
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should show an error when error is thrown', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMandatoryHeaderFields.mockReturnValue(throwError(() => ({error: {modelState: {'': ['just an error']}}})));

        component.save();

        expect(notificationServiceMock.error).toHaveBeenCalledTimes(1);
    });

    it('should show warning when no records were edited', async () => {
        await fixture.whenStable();

        entryServiceMock.updateMandatoryHeaderFields.mockReturnValue(of({entryCount: 0, affectedOccurrences: 0, dataWasChanged: false, lockInfos: []}));

        component.save();

        expect(notificationServiceMock.warning).toHaveBeenCalledTimes(1);
    });
});
