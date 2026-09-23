import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatChip} from '@angular/material/chips';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelect, MatSelectChange, MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {SubjectSummary} from 'api/SubjectSummary';
import {MockComponent, MockModule} from 'ng-mocks';
import {AutocompleteComponent} from '../../autocomplete/autocomplete.component';
import {ChiplistAutocompleteComponent} from '../../chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../../error.service';
import {WINDOW} from '../../injection.tokens';
import {SubjectEntryComponent} from './subject-entry.component';

describe('SubjectEntryComponent', () => {
    let component: SubjectEntryComponent;
    let fixture: ComponentFixture<SubjectEntryComponent>;

    const errorServiceMock = {
        getControlError: jest.fn(),
    };
    const windowMock = {
        setTimeout: jest.fn(),
    };

    let formBuilder: FormBuilder;
    let formGroup: FormGroup;
    let subjects: SubjectSummary[];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(TranslateModule),
                MockModule(MatIconModule),
                MockModule(MatSelectModule),
                MockModule(MatTooltipModule),
                ReactiveFormsModule,
                SubjectEntryComponent,
                MockComponent(AutocompleteComponent),
                MockComponent(ChiplistAutocompleteComponent),
                MockComponent(MatChip),
            ],
            providers: [
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: WINDOW, useValue: windowMock},
            ],
        }).compileComponents();

        subjects = [
            {
                id: 1,
                name: 'name1',
                domains: [
                    {id: 11, name: 'name11', subjectId: 1},
                    {id: 12, name: 'name12', subjectId: 1},
                ],
            },
            {
                id: 2,
                name: 'name2',
                domains: [
                    {id: 21, name: 'name21', subjectId: 2},
                    {id: 22, name: 'name22', subjectId: 2},
                    {id: 23, name: 'name23', subjectId: 2},
                ],
            },
        ];

        formBuilder = TestBed.inject(FormBuilder);
        formGroup = formBuilder.group({subject: formBuilder.control(''), domains: formBuilder.control('')});

        fixture = TestBed.createComponent(SubjectEntryComponent);
        component = fixture.componentInstance;
        component.formGroup = formGroup;
        component.subjects = subjects;
        component.entrySubject = subjects[0];

        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load domains according to subjectId', async () => {
        component.ngOnInit();

        await fixture.whenStable();

        expect(component.domains.length).toEqual(2);
        expect(component.domainChips.length).toEqual(2);
    });

    it('should change domains when subject changes', () => {
        const event: MatSelectChange = {value: 2, source: {} as MatSelect};
        component.selectionChanged(event);

        expect(component.domains.length).toEqual(3);
        expect(component.domainChips.length).toEqual(3);
    });

    it('should emit remove when remove button is clicked', () => {
        let removeCounter = 0;
        component.subjectRemoved.subscribe(() => removeCounter++);

        const removeButton = fixture.debugElement.query(By.css('button'));
        removeButton.triggerEventHandler('click', {});

        expect(removeCounter).toEqual(1);
    });
});
