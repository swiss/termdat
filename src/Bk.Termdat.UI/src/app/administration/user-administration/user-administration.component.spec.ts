import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatChipsModule} from '@angular/material/chips';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObInputClearModule} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {UserAccountDetail} from 'api/UserAccountDetail';
import {MockModule} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {UserAccountService} from './user-account.service';
import {UserAdministrationComponent} from './user-administration.component';

describe('UserAdministrationComponent', () => {
    let component: UserAdministrationComponent;
    let fixture: ComponentFixture<UserAdministrationComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    const userAccountServiceMock = {
        getUserAccountDetails: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    let userAccountDetails: UserAccountDetail[] = [];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MatPaginatorModule,
                MatInputModule,
                MatTooltipModule,
                MatSortModule,
                MatFormFieldModule,
                MockModule(MatChipsModule),
                MockModule(MatTableModule),
                MockModule(MatIconModule),
                MockModule(MatButtonModule),
                MockModule(ObInputClearModule),
                MockModule(TranslateModule),
                UserAdministrationComponent,
            ],
            providers: [
                {provide: UserAccountService, useValue: userAccountServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        userAccountDetails = [
            {
                firstName: 'Peter',
                lastName: 'Mann',
                email: 'p.m@bit.ch',
                hasRole: true,
                role: 'Admin',
                lastAction: new Date('2023-04-24 06:39:49.101275+00'),
                collections: [{id: 1, code: 'code1', deleted: false, isObsolete: false, name: 'collection one'} as CollectionSummary],
                glossaries: [{id: 2, code: 'code2', name: 'glossary two'}],
                hasFullAccess: true,
            } as UserAccountDetail,
            {
                firstName: 'Vreni',
                lastName: 'Frau',
                email: 'v.f@bit.ch',
                hasRole: true,
                role: 'Super-User',
                lastAction: new Date('2022-04-24 06:39:49.101275+00'),
                collections: [{id: 1, code: 'code1', deleted: false, isObsolete: false, name: 'collection one'} as CollectionSummary],
                glossaries: [{id: 3, code: 'code3', name: 'glossary three'}],
                hasFullAccess: true,
            } as UserAccountDetail,
            {
                firstName: 'Pete',
                lastName: 'Kind',
                email: 'p.k@bit.ch',
                hasRole: true,
                role: 'Writer',
                lastAction: new Date('2021-04-24 06:39:49.101275+00'),
                collections: [{id: 1, code: 'code1', deleted: false, isObsolete: false, name: 'collection one'} as CollectionSummary],
                glossaries: [{id: 4, code: 'code2', name: 'glossary four'}],
                hasFullAccess: false,
            } as UserAccountDetail,
            {
                firstName: 'Norman',
                lastName: 'Nobody',
                email: 'n.p@bit.ch',
                hasRole: false,
                hasFullAccess: false,
            } as UserAccountDetail,
        ];

        userAccountServiceMock.getUserAccountDetails.mockReturnValue(of(userAccountDetails));
    });

    beforeEach(async () => {
        fixture = TestBed.createComponent(UserAdministrationComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load user account details on init', () => {
        expect(userAccountServiceMock.getUserAccountDetails).toHaveBeenCalledTimes(1);

        expect(component.dataSource.data).toHaveLength(3); // Users without a role are excluded
    });

    it('should set pagination and sort', () => {
        expect(component.paginator).toBeDefined();
        expect(component.dataSource.paginator).toEqual(component.paginator);

        expect(component.sort).toBeDefined();
        expect(component.dataSource.sort).toEqual(component.sort);
    });

    it('should call updateUserAccount', () => {
        const userAccountDetail = {
            firstName: 'Vreni',
            lastName: 'Frau',
            email: 'v.f@bit.ch',
            role: 'Super-User',
            lastAction: new Date('2022-04-24 06:39:49.101275+00'),
            collections: [{id: 1, code: 'code1', deleted: false, isObsolete: false, name: 'collection one'} as CollectionSummary],
            glossaries: [{id: 3, code: 'code3', name: 'glossary three'}],
            hasFullAccess: true,
        } as UserAccountDetail;

        component.updateUserAccount(userAccountDetail);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });

    it('should reload user account details on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(userAccountServiceMock.getUserAccountDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toHaveLength(3);
    });

    it.each([
        {filter: 'Vreni', length: 1},
        {filter: 'Peter', length: 1},
        {filter: 'Pete', length: 2},
        {filter: 'p.k', length: 1},
        {filter: 'Writer', length: 1},
        {filter: '2023', length: 1},
        {filter: 'code3', length: 1},
        {filter: 'code1', length: 3},
    ])('should apply filter and clear filter', async ({filter, length}) => {
        expect(component.dataSource.filteredData.length).toEqual(3);

        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(length);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toEqual(3);
    });
});
