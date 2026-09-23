import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortable, MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObInputClearModule} from '@oblique/oblique';
import {LenochClassificationDetail} from 'api/LenochClassificationDetail';
import {MockModule} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {LenochClassificationComponent} from './lenoch-classification.component';
import {LenochClassificationService} from './lenoch-classification.service';

describe('LenochClassificationComponent', () => {
    let component: LenochClassificationComponent;
    let fixture: ComponentFixture<LenochClassificationComponent>;

    let details: LenochClassificationDetail[] = [];
    const lenochClassificationServiceMock = {
        getLenochClassificationDetails: jest.fn(),
    };

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
        instant: jest.fn(),
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
                LenochClassificationComponent,
            ],
            providers: [
                {provide: LenochClassificationService, useValue: lenochClassificationServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        }).compileComponents();

        details = [
            {
                id: 8007,
                code: 'AD5',
                name: 'ARBEITEN',
                entryCount: 324,
                subject: 'VERWALTUNG',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'GEWALT',
                entryCount: 1417,
                subject: 'SICHERHEIT',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'GEWALT',
                entryCount: 1417,
                subject: 'SICHERHEIT',
                domain: 'Berufe',
            },
            {
                id: 8008,
                code: 'AD6',
                name: 'GEWALT',
                entryCount: 1417,
                subject: 'VERWALTUNG',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'SICHERHEIT',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'SICHERHEIT',
                domain: 'Bevölkerungsschutz',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'SICHERHEIT',
                domain: 'Cybersicherheit',
            },
            {
                id: 8009,
                code: 'AD7',
                name: 'SICHERHEIT (auch: ME8)',
                entryCount: 3893,
                subject: 'VERWALTUNG',
            },
            {
                id: 8010,
                code: 'AD8',
                name: 'OLD',
                entryCount: 2,
            },
        ];
        lenochClassificationServiceMock.getLenochClassificationDetails.mockReturnValue(of(details));
        translateServiceMock.instant.mockImplementation(value => value);

        fixture = TestBed.createComponent(LenochClassificationComponent);
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

    it('should load lenoch classifications on init', () => {
        expect(lenochClassificationServiceMock.getLenochClassificationDetails).toHaveBeenCalledTimes(1);
        expect(component.dataSource.data).toEqual(details);
    });

    it('should reload lenoch classifications on language change', () => {
        component.dataSource.data = [];

        langChangeSubject.next(true);

        expect(lenochClassificationServiceMock.getLenochClassificationDetails).toHaveBeenCalledTimes(2);
        expect(component.dataSource.data).toEqual(details);
    });

    it('should set row spans on init', () => {
        expect(component.rowSpans).toBeDefined();
        expect(component.rowSpans.id).toBeDefined();
        expect(component.rowSpans.id).toHaveLength(details.length);
        expect(component.rowSpans.subject).toBeDefined();
        expect(component.rowSpans.subject).toHaveLength(details.length);

        expect(component.rowSpans).toEqual({
            id: [1, 3, 0, 0, 4, 0, 0, 0, 1],
            subject: [1, 2, 0, 1, 3, 0, 0, 1, 1],
        });
    });

    it('updates row spans on filtering', async () => {
        component.applyFilter({target: {value: 'GEWALT'} as unknown} as Event);
        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(3);
        expect(component.rowSpans).toEqual({
            id: [3, 0, 0],
            subject: [2, 0, 1],
        });
    });

    it('updates row spans on paging', async () => {
        component.paginator._changePageSize(5);
        component.paginator.lastPage();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(9);
        expect(component.rowSpans).toEqual({
            id: [3, 0, 0, 1],
            subject: [2, 0, 1, 1],
        });
    });

    it('updates row spans on sorting (relevant when sorting while paging)', async () => {
        component.paginator._changePageSize(5);
        component.paginator.lastPage();
        component.sort.sort({id: 'code', start: 'desc'} as MatSortable);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(9);
        expect(component.rowSpans).toEqual({
            id: [3, 0, 0, 1],
            subject: [2, 0, 1, 1],
        });

        component.sort.sort({id: 'name', start: 'asc'} as MatSortable);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(9);
        // TODO: Von Michael interpretieren lassen ob hier was kaputt ist.
        expect(component.rowSpans).toEqual({
            id: [4, 0, 0, 0],
            subject: [3, 0, 0, 1],
        });
    });

    it.each([
        {filter: 'GEWALT', expectedHits: 3},
        {filter: 'AD5', expectedHits: 1},
        {filter: 'SICHERHEIT', expectedHits: 6},
        {filter: 'Cybersicherheit', expectedHits: 1},
        {filter: 'Bevölkerungs', expectedHits: 1},
        {filter: '324', expectedHits: 1},
        {filter: '8007', expectedHits: 1},
        {filter: null, expectedHits: 9},
    ])('should apply filter $filter with results and clear filter', async ({filter, expectedHits}) => {
        expect(component.dataSource.filteredData).toHaveLength(9);
        component.applyFilter({target: {value: filter} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(expectedHits);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.dataSource.filteredData).toHaveLength(9);
    });

    it('can apply filter and clear filter without paginator', async () => {
        // @ts-ignore
        component.paginator = null;
        component.dataSource.paginator = null;

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData).toHaveLength(9);

        component.applyFilter({target: {value: 'AD5'} as unknown} as Event);

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData).toHaveLength(1);

        component.clearFilter();

        fixture.detectChanges();
        await fixture.whenStable();
        expect(component.dataSource.filteredData).toHaveLength(9);
    });

    it('should add null substitute for entries without subject', () => {
        expect(component.dataSource.data[8]).toBeDefined();
        expect(component.dataSource.data[8].subject).toEqual('administration.lenochClassifications.unknownSubject');
        expect(component.dataSource.data[0].subject).toEqual('VERWALTUNG');
        expect(component.dataSource.data[3].subject).toEqual('VERWALTUNG');
        expect(component.dataSource.data[6].subject).toEqual('SICHERHEIT');

        expect(translateServiceMock.instant).toHaveBeenCalledTimes(1);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('administration.lenochClassifications.unknownSubject');
    });
});
