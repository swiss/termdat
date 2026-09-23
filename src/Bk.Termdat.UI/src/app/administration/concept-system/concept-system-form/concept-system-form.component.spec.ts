import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockComponent, MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {AutocompleteComponent} from '../../../shared/autocomplete/autocomplete.component';
import {CollectionService} from '../../collection/collection.service';
import {ConceptSystemFormComponent} from './concept-system-form.component';

describe('ConceptSystemFormComponent', () => {
    let component: ConceptSystemFormComponent;
    let fixture: ComponentFixture<ConceptSystemFormComponent>;

    const onLangChangeSubject = new Subject<void>();
    const translateServiceMock = {
        onLangChange: onLangChangeSubject.asObservable(),
    };

    const collectionServiceMock = {
        getCollectionDetails: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatTooltipModule),
                MockModule(MatOptionModule),
                MockPipe(TranslatePipe),
                ConceptSystemFormComponent,
                MockDirective(ObUnsavedChangesDirective),
                MockDirective(RouterLink),
                MockComponent(AutocompleteComponent),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: CollectionService, useValue: collectionServiceMock},
            ],
        }).compileComponents();

        collectionServiceMock.getCollectionDetails.mockReturnValue(of([]));

        fixture = TestBed.createComponent(ConceptSystemFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(collectionServiceMock.getCollectionDetails).toHaveBeenCalledTimes(1);
        expect(component.positions).toHaveLength(1);
    });

    it('should reload collections after language change', () => {
        collectionServiceMock.getCollectionDetails.mockClear();

        onLangChangeSubject.next();

        expect(collectionServiceMock.getCollectionDetails).toHaveBeenCalledTimes(1);
    });

    it('should fill form and positions', () => {
        component.conceptSystem = {
            id: 42,
            title: 'Test Concept System Title',
            collectionId: 11,
            positions: [
                {
                    id: 7,
                    value: 'test07',
                },
                {
                    id: 1,
                    value: 'test05',
                },
                {
                    id: 6,
                    value: 'test0',
                },
            ],
        };

        const values = component.form.getRawValue();
        expect(values.id).toBe(42);
        expect(values.title).toEqual('Test Concept System Title');
        expect(values.collectionId).toBe(11);
        expect(values.positions).toHaveLength(3);
        expect(values.positions[0].value).toEqual('test07');
        expect(values.positions[1].value).toEqual('test05');
        expect(values.positions[2].value).toEqual('test0');
    });

    it('should set initial position when none is give', () => {
        component.conceptSystem = {
            id: 11,
            title: 'Test Concept System Title',
            collectionId: 7,
        };

        expect(component.positions).toHaveLength(1);
        expect(component.positions.at(0).value.value).toEqual('');
    });

    it('should call save on click', () => {
        jest.spyOn(component.saved, 'emit');

        component.form.patchValue({
            title: 'Test Concept System Title',
            collectionId: 42,
        });

        component.save();

        expect(component.saved.emit).toHaveBeenCalledTimes(1);
    });

    it('should not save when invalid', () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });

    it('should add new position', () => {
        component.positions.clear();
        component.positions.push(new FormControl('test0'));
        component.positions.push(new FormControl('test5'));

        component.addPosition(0);

        expect(component.positions).toHaveLength(3);
        expect(component.positions.at(0).value).toEqual('test0');
        expect(component.positions.at(1).value.value).toEqual('');
        expect(component.positions.at(2).value).toEqual('test5');
    });

    it('should remove position', () => {
        component.positions.clear();
        component.positions.push(new FormControl('test5'));
        component.positions.push(new FormControl('test42'));
        component.positions.push(new FormControl('test0'));

        component.removePosition(1);

        expect(component.positions).toHaveLength(2);
        expect(component.positions.at(0).value).toEqual('test5');
        expect(component.positions.at(1).value).toEqual('test0');
    });

    it('should have empty default position after last is removed', () => {
        component.positions.clear();
        component.positions.push(new FormControl(''));

        component.removePosition(0);

        expect(component.positions).toHaveLength(1);
        expect(component.positions.at(0).value.value).toEqual('');
    });
});
