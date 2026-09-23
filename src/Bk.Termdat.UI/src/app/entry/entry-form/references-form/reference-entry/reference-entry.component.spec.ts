import {ComponentFixture, fakeAsync, TestBed, tick} from '@angular/core/testing';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {ActivatedRoute, RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {ReferenceType} from 'api/ReferenceType';
import {MockModule} from 'ng-mocks';
import {of, Subject, throwError} from 'rxjs';
import {ErrorService} from '../../../../shared/error.service';
import {LanguageService} from '../../../../shared/language.service';
import {EntryService} from '../../../entry.service';
import {ReferenceEntryComponent} from './reference-entry.component';
import {ReferenceEntryMode} from './ReferenceEntryMode';

describe('ReferenceEntryComponent', () => {
    let component: ReferenceEntryComponent;
    let fixture: ComponentFixture<ReferenceEntryComponent>;

    const activatedRouteMock = {
        snapshot: {
            queryParams: {},
        },
    };

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        onLangChange: onLangChangeSubject.asObservable(),
    };

    const languageServiceMock = {
        getCurrentLanguageId: jest.fn(),
    };

    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    const entryServiceMock = {
        getReferenceTypes: jest.fn(),
        getReferenceEntry: jest.fn(),
    };

    const httpApiInterceptorEventsMock = {
        deactivateOnNextAPICalls: jest.fn(),
    };

    let formBuilder: FormBuilder;
    let form: FormGroup;

    const referenceTypes = [
        {id: ReferenceType.None, name: 'field.reference.None'},
        {id: ReferenceType.Specific, name: 'field.reference.Spe'},
    ];

    let entryReferenceDetail: EntryReferenceDetail;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatIconModule),
                MockModule(MatInputModule),
                MockModule(MatButtonModule),
                MockModule(TranslateModule),
                MockModule(MatTooltipModule),
                ReactiveFormsModule,
                RouterModule,
                ReferenceEntryComponent,
            ],
            providers: [
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
                {provide: ObHttpApiInterceptorEvents, useValue: httpApiInterceptorEventsMock},
            ],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        form = formBuilder.group({});

        entryReferenceDetail = {
            id: 1,
            entryId: 10,
            type: ReferenceType.Specific,
            languages: [
                {languageId: 1, text: 'Language Id 1 Text'},
                {languageId: 2, text: 'Language Id 2 Text'},
                {languageId: 3, text: 'Language Id 3 Text'},
                {languageId: 4, text: 'Language Id 4 Text'},
            ],
        };

        entryServiceMock.getReferenceTypes.mockReturnValue(of(referenceTypes));
        entryServiceMock.getReferenceEntry.mockReturnValue(of(entryReferenceDetail));
        languageServiceMock.getCurrentLanguageId.mockReturnValue(2);

        fixture = TestBed.createComponent(ReferenceEntryComponent);
        component = fixture.componentInstance;
        component.form = form;

        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should use defaults on init', () => {
        expect(component.loading).toEqual(false);
        expect(component.mode).toEqual(ReferenceEntryMode.Create);
        expect(component.text).toEqual('');
    });

    it('should load reference types on init and lang change', () => {
        expect(entryServiceMock.getReferenceTypes).toHaveBeenCalledTimes(1);
        expect(component.referenceTypes).toEqual(referenceTypes);

        entryServiceMock.getReferenceTypes.mockReturnValue(of([]));

        onLangChangeSubject.next(true);

        expect(entryServiceMock.getReferenceTypes).toHaveBeenCalledTimes(2);
        expect(component.referenceTypes).toEqual([]);
    });

    it('should reset text and languages on entryId changes', () => {
        component.text = 'test';
        component.languages = entryReferenceDetail.languages;

        component.form.controls.entryId.setValue(123, {emitEvent: true});

        expect(component.text).toEqual('');
        expect(component.languages).toEqual([]);
    });

    it('should load reference entry on entryId value changes', fakeAsync(() => {
        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        component.form.controls.entryId.setValue('123', {emitEvent: true});

        tick(500);

        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledWith(1);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledWith(123);

        expect(component.form.controls.entryId.errors).toBeNull();
        expect(component.form.controls.entryId.touched).toEqual(false);
        expect(component.languages).toEqual(entryReferenceDetail.languages);
        expect(component.text).toEqual(entryReferenceDetail.languages[1].text);
    }));

    it('should not load reference entry on invalid entryId', fakeAsync(() => {
        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        component.form.controls.entryId.setValue('abc', {emitEvent: true});

        tick(500);

        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        expect(component.form.controls.entryId.errors).toEqual({pattern: {actualValue: 'abc', requiredPattern: '/[0-9]+/'}});
        expect(component.form.controls.entryId.touched).toEqual(false);
        expect(component.languages).toEqual([]);
        expect(component.text).toEqual('');
    }));

    it('should not load reference entry on empty entryId', fakeAsync(() => {
        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        component.form.controls.entryId.setValue('', {emitEvent: true});

        tick(500);

        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        expect(component.form.controls.entryId.errors).toEqual({required: true});
        expect(component.form.controls.entryId.touched).toEqual(false);
        expect(component.languages).toEqual([]);
        expect(component.text).toEqual('');
    }));

    it('should catch error while loading reference entry', fakeAsync(() => {
        entryServiceMock.getReferenceEntry.mockReturnValue(throwError(() => new Error('SHOULD NOT BE THROWN')));

        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(0);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(0);

        component.form.controls.entryId.setValue(123, {emitEvent: true});

        tick(500);

        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledTimes(1);
        expect(httpApiInterceptorEventsMock.deactivateOnNextAPICalls).toHaveBeenCalledWith(1);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledTimes(1);
        expect(entryServiceMock.getReferenceEntry).toHaveBeenCalledWith(123);

        expect(component.form.controls.entryId.errors).toEqual({unknownEntryId: true});
        expect(component.form.controls.entryId.touched).toEqual(true);
        expect(component.languages).toEqual([]);
        expect(component.text).toEqual('');
    }));

    describe('edit mode', () => {
        let entryReferenceUpdate: EntryReferenceDetail;

        beforeEach(async () => {
            entryReferenceUpdate = {
                id: 2,
                entryId: 20,
                type: ReferenceType.General,
                languages: [
                    {languageId: 1, text: 'Updated Language Id 1 Text'},
                    {languageId: 2, text: 'Updated Language Id 2 Text'},
                ],
            };

            component.mode = ReferenceEntryMode.Edit;
            component.entryReference = entryReferenceDetail;

            fixture.detectChanges();
            await fixture.whenStable();
        });

        it('should setup correctly for edit mode', () => {
            expect(component.form.getRawValue()).toEqual({
                entryId: 10,
                id: 1,
                type: ReferenceType.Specific,
            });
            expect(component.languages).toEqual(entryReferenceDetail.languages);
            expect(component.text).toEqual(entryReferenceDetail.languages[1].text);
        });

        it('should update text when onLangChange is emitted', () => {
            expect(component.text).toEqual(entryReferenceDetail.languages[1].text);

            languageServiceMock.getCurrentLanguageId.mockReturnValue(3);
            onLangChangeSubject.next(true);
            expect(component.text).toEqual(entryReferenceDetail.languages[2].text);

            languageServiceMock.getCurrentLanguageId.mockReturnValue(5);
            onLangChangeSubject.next(true);
            expect(component.text).toEqual(entryReferenceDetail.languages[0].text);

            component.languages = [];
            onLangChangeSubject.next(true);
            expect(component.text).toEqual('');
        });

        it('should emit remove on button click for edit mode', () => {
            let removed = 0;
            let created: EntryReferenceDetail;
            component.referenceRemoved.subscribe(() => removed++);
            component.referenceCreated.subscribe(ref => (created = ref));

            const button = fixture.debugElement.query(By.css('button'));
            button.triggerEventHandler('click', {});

            expect(removed).toEqual(1);
            expect(created!).toBeUndefined();
        });

        it('should update text, languages and form when entry reference is set', () => {
            component.form.markAsTouched();
            component.entryReference = entryReferenceUpdate;

            expect(component.languages).toEqual(entryReferenceUpdate.languages);
            expect(component.text).toEqual(entryReferenceUpdate.languages[1].text);
            expect(component.form.touched).toEqual(false);
            expect(component.form.getRawValue()).toEqual({
                entryId: 20,
                id: 2,
                type: ReferenceType.General,
            });
        });

        it('should handle reference detail updates when form is still undefined (angular lifecycle update problem)', () => {
            component.form = null as unknown as FormGroup;

            expect(() => {
                component.entryReference = entryReferenceUpdate;
            }).not.toThrow();

            expect(component.text).toEqual(entryReferenceUpdate.languages[1].text);
            expect(component.languages).toEqual(entryReferenceUpdate.languages);
        });

        it('should handle reference detail update being null', () => {
            component.entryReference = null as unknown as EntryReferenceDetail;

            expect(component.text).toEqual('');
            expect(component.languages).toEqual([]);
            expect(component.form.getRawValue()).toEqual({
                entryId: null,
                id: null,
                type: ReferenceType.None,
            });
        });
    });

    describe('create mode', () => {
        beforeEach(async () => {
            component.mode = ReferenceEntryMode.Create;

            fixture.detectChanges();
            await fixture.whenStable();
        });

        it('should setup correctly for create mode', () => {
            expect(component.form.getRawValue()).toEqual({
                entryId: null,
                id: null,
                type: ReferenceType.None,
            });
        });

        it('should emit create on button click for create mode', () => {
            let removed = 0;
            let created: EntryReferenceDetail;
            component.referenceRemoved.subscribe(() => removed++);
            component.referenceCreated.subscribe(ref => (created = ref));

            component.form.patchValue({
                type: ReferenceType.Specific,
                entryId: 123,
            });
            component.languages = [{text: 'created', languageId: 2}];

            const button = fixture.debugElement.query(By.css('button'));
            button.triggerEventHandler('click', {});

            expect(removed).toEqual(0);
            expect(created!).toEqual({
                entryId: 123,
                id: null,
                languages: [
                    {
                        languageId: 2,
                        text: 'created',
                    },
                ],
                type: ReferenceType.Specific,
            });
            expect(component.form.touched).toEqual(false);
            expect(component.form.getRawValue()).toEqual({
                entryId: null,
                id: null,
                type: ReferenceType.None,
            });
            expect(component.text).toEqual('');
            expect(component.languages).toEqual([]);
        });

        it('should not emit create on button click for create mode if form is invalid or no entryId is provided', () => {
            let removed = 0;
            let created: EntryReferenceDetail;
            component.referenceRemoved.subscribe(() => removed++);
            component.referenceCreated.subscribe(ref => (created = ref));

            const button = fixture.debugElement.query(By.css('button'));
            button.triggerEventHandler('click', {});

            expect(removed).toEqual(0);
            expect(created!).toBeUndefined();
            expect(component.form.touched).toEqual(true);

            component.form.controls.entryId.setValue(0, {emitEvent: true});

            button.triggerEventHandler('click', {});
            expect(removed).toEqual(0);
            expect(created!).toBeUndefined();
            expect(component.form.touched).toEqual(true);
        });
    });
});
