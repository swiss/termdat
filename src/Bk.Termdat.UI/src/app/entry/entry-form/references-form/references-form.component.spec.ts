import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {ReferenceType} from 'api/ReferenceType';
import {MockComponent, MockModule} from 'ng-mocks';
import {ReferenceEntryComponent} from './reference-entry/reference-entry.component';
import {ReferenceEntryMode} from './reference-entry/ReferenceEntryMode';
import {ReferencesFormComponent} from './references-form.component';

describe('ReferencesFormComponent', () => {
    let component: ReferencesFormComponent;
    let fixture: ComponentFixture<ReferencesFormComponent>;

    let formArray: FormArray;
    let formBuilder: FormBuilder;
    let references: EntryReferenceDetail[];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MockModule(TranslateModule), ReactiveFormsModule, ReferencesFormComponent, MockComponent(ReferenceEntryComponent)],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        formArray = formBuilder.array([]);
        references = [
            {
                id: 1,
                entryId: 10,
                type: ReferenceType.Specific,
                languages: [
                    {languageId: 1, text: 'Language Id 1 EntryId 10'},
                    {languageId: 2, text: 'Language Id 2 EntryId 10'},
                    {languageId: 3, text: 'Language Id 3 EntryId 10'},
                    {languageId: 4, text: 'Language Id 4 EntryId 10'},
                ],
            },
            {
                id: 2,
                entryId: 20,
                type: ReferenceType.General,
                languages: [
                    {languageId: 2, text: 'Language Id 2 EntryId 20'},
                    {languageId: 3, text: 'Language Id 3 EntryId 20'},
                ],
            },
        ];

        fixture = TestBed.createComponent(ReferencesFormComponent);
        component = fixture.componentInstance;
        component.formArray = formArray;

        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should setup correctly', () => {
        expect(component.createForm).toBeDefined();
        expect(component.createForm.pristine).toEqual(true);
        expect(component.createForm.touched).toEqual(false);

        expect(component.formArray.length).toEqual(0);
        expect(component.formArray.pristine).toEqual(true);
        expect(component.formArray.touched).toEqual(false);
    });

    describe('empty references', () => {
        it('should add reference when referenceCreated is emitted', () => {
            const referenceEntryCreate = fixture.debugElement.query(By.directive(ReferenceEntryComponent));
            const referenceEntryCreateComponent = referenceEntryCreate.componentInstance as ReferenceEntryComponent;
            expect(referenceEntryCreateComponent.mode).toEqual(ReferenceEntryMode.Create);
            expect(referenceEntryCreateComponent.form).toEqual(component.createForm);

            referenceEntryCreate.triggerEventHandler('referenceCreated', references[0]);

            expect(component._references).toEqual([references[0]]);
            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.dirty).toEqual(true);
            expect(component.formArray.touched).toEqual(true);
        });
    });

    describe('provided references', () => {
        beforeEach(async () => {
            component.references = references;

            fixture.detectChanges();
            await fixture.whenStable();
        });

        it('should setup correctly with references', () => {
            expect(component.createForm).toBeDefined();
            expect(component.createForm.pristine).toEqual(true);
            expect(component.createForm.touched).toEqual(false);

            expect(component.formArray.length).toEqual(2);
            expect(component.formArray.pristine).toEqual(true);
            expect(component.formArray.touched).toEqual(false);
        });

        it('should remove reference when referenceRemoved is emitted', () => {
            const referenceEntryEdit = fixture.debugElement.query(By.directive(ReferenceEntryComponent));
            const referenceEntryEditComponent = referenceEntryEdit.componentInstance as ReferenceEntryComponent;
            expect(referenceEntryEditComponent.mode).toEqual(ReferenceEntryMode.Edit);
            expect(referenceEntryEditComponent.form).toEqual(component.formArray.controls[0]);
            expect(referenceEntryEditComponent.entryReference).toEqual(component._references[0]);

            referenceEntryEdit.triggerEventHandler('referenceRemoved', {});

            expect(component._references).toEqual([references[0]]);
            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.dirty).toEqual(true);
            expect(component.formArray.touched).toEqual(true);
        });
    });
});
