import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatDividerModule} from '@angular/material/divider';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockComponent, MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of} from 'rxjs';
import {TermdatClassificationDomainFormComponent} from '../domain-form/domain-form.component';
import {TermdatClassificationService} from '../termdat-classification.service';
import {TermdatClassificationSubjectFormComponent} from './subject-form.component';

describe('TermdatClassificationSubjectFormComponent', () => {
    let component: TermdatClassificationSubjectFormComponent;
    let fixture: ComponentFixture<TermdatClassificationSubjectFormComponent>;

    const classificationServiceMock = {
        getDomainLanguageDescriptions: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatDividerModule),
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                MockModule(MatTooltipModule),
                TermdatClassificationSubjectFormComponent,
                MockComponent(TermdatClassificationDomainFormComponent),
                MockPipe(TranslatePipe),
                MockDirective(ObUnsavedChangesDirective),
            ],
            providers: [{provide: TermdatClassificationService, useValue: classificationServiceMock}],
        }).compileComponents();

        classificationServiceMock.getDomainLanguageDescriptions.mockReturnValue(of([]));

        fixture = TestBed.createComponent(TermdatClassificationSubjectFormComponent);
        component = fixture.componentInstance;

        component.subject = {
            id: 42,
            descriptionDe: 'Test Name DE',
            descriptionFr: 'Test Name FR',
            descriptionEn: 'Test Name EN',
            descriptionRm: 'Test Name RM',
            descriptionIt: 'Test Name IT',
            domains: [
                {
                    id: 1,
                    subjectId: 42,
                    descriptionDe: 'Test Domain Name DE 1',
                    descriptionFr: 'Test Domain Name FR 1',
                    descriptionEn: 'Test Domain Name EN 1',
                    descriptionRm: 'Test Domain Name RM 1',
                    descriptionIt: 'Test Domain Name IT 1',
                },
                {
                    id: 11,
                    subjectId: 42,
                    descriptionDe: 'Test Domain Name DE 11',
                    descriptionFr: 'Test Domain Name FR 11',
                    descriptionEn: 'Test Domain Name EN 11',
                    descriptionRm: 'Test Domain Name RM 11',
                    descriptionIt: 'Test Domain Name IT 11',
                },
                {
                    id: 111,
                    subjectId: 42,
                    descriptionDe: 'Test Domain Name DE 111',
                    descriptionFr: 'Test Domain Name FR 111',
                    descriptionEn: 'Test Domain Name EN 111',
                    descriptionRm: 'Test Domain Name RM 111',
                    descriptionIt: 'Test Domain Name IT 111',
                },
            ],
        };

        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(classificationServiceMock.getDomainLanguageDescriptions).toHaveBeenCalledTimes(1);
    });

    it('should fill form and domains', () => {
        const values = component.form.getRawValue();
        expect(values.id).toBe(42);
        expect(values.descriptionDe).toEqual('Test Name DE');
        expect(values.descriptionFr).toEqual('Test Name FR');
        expect(values.descriptionEn).toEqual('Test Name EN');
        expect(values.descriptionRm).toEqual('Test Name RM');
        expect(values.descriptionIt).toEqual('Test Name IT');
        expect(values.domains).toHaveLength(3);
        expect(values.domains[0].id).toEqual(1);
        expect(values.domains[1].id).toEqual(11);
        expect(values.domains[2].id).toEqual(111);
        expect(values.domains[0].subjectId).toEqual(42);
        expect(values.domains[1].subjectId).toEqual(42);
        expect(values.domains[2].subjectId).toEqual(42);
    });

    it('should set uppercase text', () => {
        component.toUppercase({target: {value: 'some text for descriptionDe'}} as unknown as Event, 'descriptionDe');
        component.toUppercase({target: {value: 'some text for descriptionFr'}} as unknown as Event, 'descriptionFr');
        component.toUppercase({target: {value: 'some text for descriptionEn'}} as unknown as Event, 'descriptionEn');
        component.toUppercase({target: {value: 'some text for descriptionRm'}} as unknown as Event, 'descriptionRm');
        component.toUppercase({target: {value: 'some text for descriptionIt'}} as unknown as Event, 'descriptionIt');

        expect(component.form.controls.descriptionDe.value).toEqual('SOME TEXT FOR DESCRIPTIONDE');
        expect(component.form.controls.descriptionFr.value).toEqual('SOME TEXT FOR DESCRIPTIONFR');
        expect(component.form.controls.descriptionEn.value).toEqual('SOME TEXT FOR DESCRIPTIONEN');
        expect(component.form.controls.descriptionRm.value).toEqual('SOME TEXT FOR DESCRIPTIONRM');
        expect(component.form.controls.descriptionIt.value).toEqual('SOME TEXT FOR DESCRIPTIONIT');
    });

    it('should call save on save', () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).toHaveBeenCalledTimes(1);
    });

    it('should not call save when invalid', () => {
        jest.spyOn(component.saved, 'emit');
        component.form.patchValue({
            descriptionDe: '',
        });

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });

    it('should disable subject form only', () => {
        component.disabledSubjectForm = true;

        component.ngOnChanges();

        expect(component.form.controls.id.disabled).toBe(true);
        expect(component.form.controls.descriptionDe.disabled).toBe(true);
        expect(component.form.controls.descriptionFr.disabled).toBe(true);
        expect(component.form.controls.descriptionEn.disabled).toBe(true);
        expect(component.form.controls.descriptionRm.disabled).toBe(true);
        expect(component.form.controls.descriptionIt.disabled).toBe(true);
        component.domains.controls.forEach(item => {
            expect((item as FormGroup).controls.id.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionDe.disabled).toBe(false);
            expect((item as FormGroup).controls.descriptionFr.disabled).toBe(false);
            expect((item as FormGroup).controls.descriptionEn.disabled).toBe(false);
            expect((item as FormGroup).controls.descriptionRm.disabled).toBe(false);
            expect((item as FormGroup).controls.descriptionIt.disabled).toBe(false);
        });
    });

    it('should disable subject and domain form', () => {
        component.disabledSubjectForm = true;
        component.disabledDomainForm = true;

        component.ngOnChanges();

        expect(component.form.controls.id.disabled).toBe(true);
        expect(component.form.controls.descriptionDe.disabled).toBe(true);
        expect(component.form.controls.descriptionFr.disabled).toBe(true);
        expect(component.form.controls.descriptionEn.disabled).toBe(true);
        expect(component.form.controls.descriptionRm.disabled).toBe(true);
        expect(component.form.controls.descriptionIt.disabled).toBe(true);
        component.domains.controls.forEach(item => {
            expect((item as FormGroup).controls.id.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionDe.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionFr.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionEn.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionRm.disabled).toBe(true);
            expect((item as FormGroup).controls.descriptionIt.disabled).toBe(true);
        });
    });

    it('should add disabled subject controls if subject form is disabled', () => {
        component.disabledSubjectForm = true;
        component.subject = {
            id: 700,
            descriptionDe: 'Test Name DE',
            descriptionFr: 'Test Name FR',
            descriptionEn: 'Test Name EN',
            descriptionRm: 'Test Name RM',
            descriptionIt: 'Test Name IT',
            domains: [],
        };

        expect(component.form.controls.id.disabled).toBe(true);
        expect(component.form.controls.descriptionDe.disabled).toBe(true);
        expect(component.form.controls.descriptionFr.disabled).toBe(true);
        expect(component.form.controls.descriptionEn.disabled).toBe(true);
        expect(component.form.controls.descriptionRm.disabled).toBe(true);
        expect(component.form.controls.descriptionIt.disabled).toBe(true);
    });

    it('should add enabled domain controls even if domain form is disabled', () => {
        component.disabledSubjectForm = true;
        component.disabledDomainForm = true;

        component.addDomain();

        expect(component.form.controls.id.disabled).toBe(true);
        expect(component.form.controls.descriptionDe.disabled).toBe(true);
        expect(component.form.controls.descriptionFr.disabled).toBe(true);
        expect(component.form.controls.descriptionEn.disabled).toBe(true);
        expect(component.form.controls.descriptionRm.disabled).toBe(true);
        expect(component.form.controls.descriptionIt.disabled).toBe(true);
        expect((component.domains.controls[3] as FormGroup).controls.id.disabled).toBe(true);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionDe.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionFr.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionEn.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionRm.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionIt.disabled).toBe(false);
    });

    it('should add new domain', () => {
        component.addDomain();

        expect(component.domains).toHaveLength(4);
        expect((component.domains.controls[3] as FormGroup).controls.id.value).toBeUndefined();
        expect((component.domains.controls[3] as FormGroup).controls.id.disabled).toBe(true);
        expect((component.domains.controls[3] as FormGroup).controls.subjectId.value).toBeUndefined();
        expect((component.domains.controls[3] as FormGroup).controls.subjectId.disabled).toBe(true);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionDe.value).toEqual('');
        expect((component.domains.controls[3] as FormGroup).controls.descriptionDe.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionFr.value).toEqual('');
        expect((component.domains.controls[3] as FormGroup).controls.descriptionFr.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionEn.value).toEqual('');
        expect((component.domains.controls[3] as FormGroup).controls.descriptionEn.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionRm.value).toEqual('');
        expect((component.domains.controls[3] as FormGroup).controls.descriptionRm.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionIt.disabled).toBe(false);
        expect((component.domains.controls[3] as FormGroup).controls.descriptionIt.value).toEqual('');
    });

    it('should remove domain entry', () => {
        component.addDomain();
        component.removeDomain(3);

        expect(component.domains).toHaveLength(3);
    });

    it('should only remove temporary domains', () => {
        component.removeDomain(0);
        component.removeDomain(1);
        component.removeDomain(2);

        expect(component.domains).toHaveLength(3);
    });
});
