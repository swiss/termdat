import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {Router, RouterLink} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObInputClearModule} from '@oblique/oblique';
import {LanguageDetail} from 'api/LanguageDetail';
import {MockDirective, MockModule} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {LanguageService} from '../../shared/language.service';
import {LanguagesComponent} from './languages.component';

describe('LanguagesComponent', () => {
    let component: LanguagesComponent;
    let fixture: ComponentFixture<LanguagesComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    let languages: LanguageDetail[] = [];
    const languageServiceMock = {
        getLanguageDetails: jest.fn(),
    };

    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MatPaginatorModule,
                MatFormFieldModule,
                MatTableModule,
                MatInputModule,
                MatSortModule,
                MatTooltipModule,
                MatButtonModule,
                MockModule(MatIconModule),
                MockModule(TranslateModule),
                MockModule(ObInputClearModule),
                LanguagesComponent,
                MockDirective(RouterLink),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        languages = [
            {
                id: 10000,
                name: 'Deutsch',
                isoCode: 'de',
                sortIndex: 1,
                isUiLanguage: true,
                createdBy: 'test@test.ch',
                modifiedBy: 'test2@test.ch',
                modified: new Date('2013-02-22T08:05:05.78'),
                created: new Date('2015-04-24T08:05:05.78'),
            },
            {
                id: 20000,
                name: 'Französisch',
                isoCode: 'fr',
                sortIndex: 2,
                isUiLanguage: true,
                createdBy: 'test3@test.ch',
                modifiedBy: 'test4@test.ch',
                modified: new Date('2014-02-22T14:05:05.78'),
                created: new Date('2016-02-22T15:05:05.78'),
            },
            {
                id: 30000,
                name: 'Englisch',
                isoCode: 'en',
                sortIndex: 5,
                isUiLanguage: true,
                createdBy: 'test@test.ch',
                modifiedBy: 'test@test.ch',
                modified: new Date('2014-02-22T11:05:05.78'),
                created: new Date('2014-02-22T11:05:05.78'),
            },
        ];

        languageServiceMock.getLanguageDetails.mockReturnValue(of(languages));

        fixture = TestBed.createComponent(LanguagesComponent);
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

    it('should set paginator and sort', () => {
        expect(component.paginator).toBeDefined();
        expect(component.dataSource.paginator).toEqual(component.paginator);

        expect(component.sort).toBeDefined();
        expect(component.dataSource.sort).toEqual(component.sort);
    });

    it('should load languages on init', () => {
        expect(languageServiceMock.getLanguageDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(languages);
    });

    it('should reload languages on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(languageServiceMock.getLanguageDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(languages);
    });

    it.each([
        {filter: 'DE'},
        {filter: '20000'},
        {filter: 'franzö'},
        {filter: 'sisch'},
        {filter: 'Englisch'},
        {filter: 'test3@test.ch'},
        {filter: 'test4@test.ch'},
        {filter: '22.02.2013'},
        {filter: '22.02.2016'},
        {filter: '2016'},
    ])('should apply filter for $filter and clear filter', async ({filter}) => {
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('can apply empty filter', async () => {
        component.applyFilter({target: {value: null} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: undefined} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('can apply filter and clear filter without paginator', async () => {
        // @ts-ignore
        component.paginator = null;
        component.dataSource.paginator = null;

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);

        component.applyFilter({target: {value: 'Deutsch'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData.length).toBe(3);
    });

    it('should navigate on update', () => {
        component.update({} as LanguageDetail);

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
    });
});
