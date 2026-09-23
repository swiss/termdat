import {Location} from '@angular/common';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {Router, RouterLink} from '@angular/router';
import {ObButtonModule} from '@oblique/oblique';
import {MockComponent, MockDirective, MockModule} from 'ng-mocks';
import {MultiEditChipCollectionsComponent} from '../multi-edit-chip/multi-edit-chip-collections/multi-edit-chip-collections.component';
import {MultiEditChipGlossariesComponent} from '../multi-edit-chip/multi-edit-chip-glossaries/multi-edit-chip-glossaries.component';
import {MultiEditAdminInfosComponent} from '../multi-edit-chip-input/multi-edit-admininfos/multi-edit-admininfos.component';
import {MultiEditHeaderDataComponent} from './multi-edit-header-data.component';

describe('MultiEditHeaderDataComponent', () => {
    let component: MultiEditHeaderDataComponent;
    let fixture: ComponentFixture<MultiEditHeaderDataComponent>;

    const locationMock = {
        getState: jest.fn(),
    };
    const routerMock = {
        navigate: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MultiEditHeaderDataComponent,
                MockComponent(MultiEditChipGlossariesComponent),
                MockComponent(MultiEditAdminInfosComponent),
                MockComponent(MultiEditChipCollectionsComponent),
                MockModule(ObButtonModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockDirective(RouterLink),
            ],
            providers: [
                {provide: Location, useValue: locationMock},
                {provide: Router, useValue: routerMock},
            ],
        }).compileComponents();

        locationMock.getState.mockReturnValue({ids: [1, 2]});
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MultiEditHeaderDataComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should read ids on init', () => {
        expect(component.ids).toEqual([1, 2]);
    });

    it('should navigate back to watchlist if ids are empty', () => {
        locationMock.getState.mockReturnValue({});

        component.ngOnInit();

        expect(routerMock.navigate).toHaveBeenCalledTimes(1);
        expect(routerMock.navigate).toHaveBeenCalledWith(['watchlist']);
    });
});
