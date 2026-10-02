import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {ReferenceType} from 'api/ReferenceType';
import {MockModule} from 'ng-mocks';
import {of} from 'rxjs';
import {ErrorService} from '../../../shared/error.service';
import {LanguageService} from '../../../shared/language.service';
import {EntryService} from '../../entry.service';
import {ReferenceEntryComponent} from './reference-entry/reference-entry.component';
import {ReferenceEntryMode} from './reference-entry/ReferenceEntryMode';
import {ReferencesFormComponent} from './references-form.component';

describe('ReferencesFormComponent with ReferenceEntryComponent', () => {
    let component: ReferencesFormComponent;
    let fixture: ComponentFixture<ReferencesFormComponent>;
    let parentForm: FormGroup;

    const reference = (id: number, entryId: number, type: ReferenceType): EntryReferenceDetail => ({
        id,
        entryId,
        type,
        languages: [{languageId: 1, text: `Text ${entryId}`}],
    });

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatIconModule),
                MockModule(MatInputModule),
                MockModule(MatButtonModule),
                TranslateModule.forRoot(),
                MockModule(MatTooltipModule),
                ReactiveFormsModule,
                RouterModule,
                ReferencesFormComponent,
            ],
            providers: [
                {provide: ActivatedRoute, useValue: {snapshot: {queryParams: {}}}},
                {provide: LanguageService, useValue: {getCurrentLanguageId: () => 1}},
                {provide: ErrorService, useValue: {getControlError: jest.fn()}},
                {provide: EntryService, useValue: {getReferenceTypes: () => of([]), getReferenceEntry: jest.fn()}},
                {provide: ObHttpApiInterceptorEvents, useValue: {deactivateOnNextAPICalls: jest.fn()}},
            ],
        }).compileComponents();

        const formBuilder = TestBed.inject(FormBuilder);
        parentForm = formBuilder.group({references: formBuilder.array([])});

        fixture = TestBed.createComponent(ReferencesFormComponent);
        component = fixture.componentInstance;
        component.formArray = parentForm.controls.references as FormArray;
        component.references = [
            reference(1, 100, ReferenceType.General),
            reference(2, 200, ReferenceType.Specific),
            reference(3, 300, ReferenceType.Antonym),
            reference(4, 400, ReferenceType.Antonym),
        ];

        fixture.detectChanges();
        await fixture.whenStable();
    });

    const editEntries = () =>
        fixture.debugElement
            .queryAll(By.directive(ReferenceEntryComponent))
            .map(de => de.componentInstance as ReferenceEntryComponent)
            .filter(c => c.mode === ReferenceEntryMode.Edit);

    it('should keep form values, texts and dirty state consistent after removing a reference in the middle', async () => {
        editEntries()[1].remove();
        fixture.detectChanges();
        await fixture.whenStable();

        const entries = editEntries();
        expect(entries.map(e => e.form.value.entryId)).toEqual([100, 300, 400]);
        expect(entries.map(e => e.text)).toEqual(['Text 100', 'Text 300', 'Text 400']);
        expect(component.formArray.value.map((v: EntryReferenceDetail) => v.entryId)).toEqual([100, 300, 400]);
        expect(component.formArray.value.map((v: EntryReferenceDetail) => v.id)).toEqual([1, 3, 4]);
        expect(parentForm.dirty).toBe(true);
    });
});
