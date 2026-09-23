import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {ExternalLinkDetail} from 'api/ExternalLinkDetail';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {MockComponent, MockModule} from 'ng-mocks';
import {ExternalLinkEntryComponent} from './external-link-entry/external-link-entry.component';
import {ExternalLinkMode} from './external-link-entry/ExternalLinkMode';
import {ExternalLinksFormComponent} from './external-links-form.component';

describe('ExternalLinksFormComponent', () => {
    let component: ExternalLinksFormComponent;
    let fixture: ComponentFixture<ExternalLinksFormComponent>;

    let formArray: FormArray;
    let formBuilder: FormBuilder;
    let externalLinks: ExternalLinkDetail[];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ExternalLinksFormComponent, MockModule(TranslateModule), ReactiveFormsModule, MockComponent(ExternalLinkEntryComponent)],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        formArray = formBuilder.array([]);
        externalLinks = [
            {
                id: 1,
                url: 'https://example.com',
                type: ExternalLinkType.Fedlex,
            },
        ];

        fixture = TestBed.createComponent(ExternalLinksFormComponent);
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

    describe('empty external links', () => {
        it('should add external link when externalLinkCreated is emitted', () => {
            const externalLinkEntryCreate = fixture.debugElement.query(By.directive(ExternalLinkEntryComponent));
            const externalLinkEntryCreateComponent = externalLinkEntryCreate.componentInstance as ExternalLinkEntryComponent;
            expect(externalLinkEntryCreateComponent.mode).toEqual(ExternalLinkMode.Create);
            expect(externalLinkEntryCreateComponent.form).toEqual(component.createForm);

            externalLinkEntryCreate.triggerEventHandler('externalLinkCreated', externalLinks[0]);

            expect(component._externalLinks).toEqual([externalLinks[0]]);
            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.dirty).toEqual(true);
            expect(component.formArray.touched).toEqual(true);
        });
    });

    describe('provided external links', () => {
        beforeEach(async () => {
            component.externalLinks = externalLinks;

            fixture.detectChanges();
            await fixture.whenStable();
        });

        it('should setup correctly with external links', () => {
            expect(component.createForm).toBeDefined();
            expect(component.createForm.pristine).toEqual(true);
            expect(component.createForm.touched).toEqual(false);

            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.pristine).toEqual(true);
            expect(component.formArray.touched).toEqual(false);
        });
    });
});
