import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatTooltipModule} from '@angular/material/tooltip';
import {ActivatedRoute, RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {EntryReferenceSummary} from 'api/EntryReferenceSummary';
import {ReferenceType} from 'api/ReferenceType';
import {MockModule} from 'ng-mocks';
import {ReferenceListComponent} from './reference-list.component';

describe('ReferenceListComponent', () => {
    let component: ReferenceListComponent;
    let fixture: ComponentFixture<ReferenceListComponent>;

    const activatedRouteMock = {
        snapshot: {
            queryParams: {},
        },
    };

    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [RouterModule, MockModule(TranslateModule), MockModule(MatTooltipModule), ReferenceListComponent],
            providers: [
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(ReferenceListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should update display of same references', async () => {
        expect(component.references).toEqual([]);
        expect(component.displayReferences).toEqual([]);

        component.references = [
            {
                type: ReferenceType.General,
                entryId: 1,
                text: 'Reference 1',
            },
            {
                type: ReferenceType.General,
                entryId: 2,
                text: 'Reference 2',
            },
            {
                type: ReferenceType.General,
                entryId: 3,
                text: 'Reference 3',
            },
        ];

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.displayReferences).toEqual([
            {
                entryId: 1,
                separator: ',',
                text: 'Reference 1',
                type: ReferenceType.General,
            },
            {entryId: 2, separator: ',', text: 'Reference 2', type: ReferenceType.None},
            {entryId: 3, text: 'Reference 3', type: ReferenceType.None},
        ]);
    });

    it('should update display references', async () => {
        expect(component.references).toEqual([]);
        expect(component.displayReferences).toEqual([]);

        component.references = [
            {
                type: ReferenceType.None,
                entryId: 1,
                text: 'Reference 1',
            },
            {
                type: ReferenceType.Antonym,
                entryId: 2,
                text: 'Reference 2',
            },
            {
                type: ReferenceType.General,
                entryId: 5,
                text: 'Reference 5',
            },
            {
                type: ReferenceType.Specific,
                entryId: 3,
                text: 'Reference 3',
            },
            {
                type: ReferenceType.None,
                entryId: 4,
                text: 'Reference 4',
            },
            {
                type: ReferenceType.General,
                entryId: 6,
                text: 'Reference 6',
            },
        ];

        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.displayReferences).toEqual([
            {
                entryId: 1,
                separator: ';',
                text: 'Reference 1',
                type: ReferenceType.None,
            },
            {
                entryId: 2,
                separator: ';',
                text: 'Reference 2',
                type: ReferenceType.Antonym,
            },
            {
                entryId: 5,
                separator: ';',
                text: 'Reference 5',
                type: ReferenceType.General,
            },
            {
                entryId: 3,
                separator: ';',
                text: 'Reference 3',
                type: ReferenceType.Specific,
            },
            {
                entryId: 4,
                separator: ';',
                text: 'Reference 4',
                type: ReferenceType.None,
            },
            {
                entryId: 6,
                text: 'Reference 6',
                type: ReferenceType.General,
            },
        ]);
    });

    it('should handle empty or null for references', () => {
        expect(async () => {
            component.references = [];

            fixture.detectChanges();
            await fixture.whenStable();
        }).not.toThrow();

        expect(async () => {
            component.references = null as unknown as EntryReferenceSummary[];

            fixture.detectChanges();
            await fixture.whenStable();
        }).not.toThrow();
    });
});
