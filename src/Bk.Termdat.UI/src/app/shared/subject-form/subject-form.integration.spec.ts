import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {MockComponent, MockModule} from 'ng-mocks';
import {of} from 'rxjs';
import {SubjectSummary} from '../../../api/SubjectSummary';
import {TermdatClassificationService} from '../../administration/termdat-classification/termdat-classification.service';
import {ChiplistAutocompleteComponent} from '../chiplist-autocomplete/chiplist-autocomplete.component';
import {ErrorService} from '../error.service';
import {WINDOW} from '../injection.tokens';
import {SubjectEntryComponent} from './subject-entry/subject-entry.component';
import {SubjectFormComponent} from './subject-form.component';

describe('SubjectFormComponent with SubjectEntryComponent', () => {
    let component: SubjectFormComponent;
    let fixture: ComponentFixture<SubjectFormComponent>;

    const subject = (id: number): SubjectSummary => ({
        id,
        name: `name${id}`,
        domains: [{id: id * 10 + 1, name: `name${id}1`, subjectId: id}],
    });
    const subjects = [subject(1), subject(2), subject(3)];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatIconModule),
                MockModule(MatButtonModule),
                MockModule(MatTooltipModule),
                TranslateModule.forRoot(),
                ReactiveFormsModule,
                SubjectFormComponent,
            ],
            providers: [
                {provide: TermdatClassificationService, useValue: {getSubjects: () => of(subjects)}},
                {provide: ErrorService, useValue: {getControlError: jest.fn()}},
                {provide: WINDOW, useValue: window},
            ],
        })
            .overrideComponent(SubjectEntryComponent, {
                remove: {imports: [ChiplistAutocompleteComponent]},
                add: {imports: [MockComponent(ChiplistAutocompleteComponent)]},
            })
            .compileComponents();

        const formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(SubjectFormComponent);
        component = fixture.componentInstance;
        component.formArray = formBuilder.array([]) as FormArray;
        component.entrySubjects = subjects;
        component.entryDomains = subjects.flatMap(s => s.domains);

        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();
    });

    const entries = () => fixture.debugElement.queryAll(By.directive(SubjectEntryComponent)).map(de => de.componentInstance as SubjectEntryComponent);

    it('should keep domain chips matching the subject of each row after removing a subject in the middle', async () => {
        expect(entries().map(e => e.domainChips.map(c => c.value))).toEqual([[11], [21], [31]]);

        component.removeSubject(1);
        fixture.detectChanges();
        await fixture.whenStable();

        expect(entries().map(e => e.formGroup.value.subject)).toEqual([1, 3]);
        expect(entries().map(e => e.domainChips.map(c => c.value))).toEqual([[11], [31]]);
    });
});
