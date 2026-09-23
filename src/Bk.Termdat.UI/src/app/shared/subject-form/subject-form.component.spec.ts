import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {provideHttpClientTesting} from '@angular/common/http/testing';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormArray, FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {MockComponent, MockModule} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {SubjectSummary} from '../../../api/SubjectSummary';
import {TermdatClassificationService} from '../../administration/termdat-classification/termdat-classification.service';
import {SubjectEntryComponent} from './subject-entry/subject-entry.component';
import {SubjectFormComponent} from './subject-form.component';

describe('SubjectFormComponent', () => {
    let component: SubjectFormComponent;
    let fixture: ComponentFixture<SubjectFormComponent>;

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        onLangChange: onLangChangeSubject.asObservable(),
    };

    const termdatClassificationServiceMock = {
        getSubjects: jest.fn(),
    };

    let formBuilder: FormBuilder;
    let formArray: FormArray;

    let subjects: SubjectSummary[];

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatTooltipModule),
                MockModule(TranslateModule),
                MockModule(MatIconModule),
                ReactiveFormsModule,
                SubjectFormComponent,
                MockComponent(SubjectEntryComponent),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: TermdatClassificationService, useValue: termdatClassificationServiceMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
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

        termdatClassificationServiceMock.getSubjects.mockReturnValue(of(subjects));

        formBuilder = TestBed.inject(FormBuilder);
        formArray = formBuilder.array([]);

        fixture = TestBed.createComponent(SubjectFormComponent);
        component = fixture.componentInstance;
        component.formArray = formArray;

        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('empty subjects', () => {
        it('should set new subject on empty init', () => {
            component.entrySubjects = [];
            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.dirty).toEqual(false);
            expect(component.formArray.touched).toEqual(false);
        });
    });

    describe('provieded subjects', () => {
        beforeEach(async () => {
            fixture = TestBed.createComponent(SubjectFormComponent);
            component = fixture.componentInstance;
            component.formArray = formArray;
            component.entrySubjects = subjects;

            fixture.detectChanges();
            await fixture.whenStable();
        });

        it('should set subject on init', () => {
            expect(component.formArray.length).toEqual(2);
        });

        it('should remove subject', () => {
            component.removeSubject(0);

            expect(component.formArray.length).toEqual(1);
            expect(component.formArray.dirty).toEqual(true);
            expect(component.formArray.touched).toEqual(true);
        });

        it('should add subject', () => {
            component.addSubject();

            expect(component.formArray.length).toEqual(3);
            expect(component.formArray.dirty).toEqual(true);
            expect(component.formArray.touched).toEqual(true);
        });
    });
});
