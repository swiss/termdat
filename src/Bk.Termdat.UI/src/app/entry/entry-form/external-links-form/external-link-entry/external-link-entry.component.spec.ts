import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {ActivatedRoute, RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ExternalLinkType} from 'api/ExternalLinkType';
import {MockModule} from 'ng-mocks';
import {Subject} from 'rxjs';
import {ErrorService} from '../../../../shared/error.service';
import {EntryService} from '../../../entry.service';
import {ReferenceEntryMode} from '../../references-form/reference-entry/ReferenceEntryMode';
import {ExternalLinkEntryComponent} from './external-link-entry.component';

describe('ExternalLinkEntryComponent', () => {
    let component: ExternalLinkEntryComponent;
    let fixture: ComponentFixture<ExternalLinkEntryComponent>;

    const activatedRouteMock = {
        snapshot: {
            queryParams: {},
        },
    };

    const errorServiceMock = {
        getControlError: jest.fn(),
    };

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        onLangChange: onLangChangeSubject.asObservable(),
    };

    const entryServiceMock = {
        getExternalLinkTypes: jest.fn(() => new Subject().asObservable()),
    };

    let formBuilder: FormBuilder;
    let form: FormGroup;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ExternalLinkEntryComponent,
                RouterModule,
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(TranslateModule),
                MockModule(MatSelectModule),
                MockModule(MatInputModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
            ],
            providers: [
                {provide: ActivatedRoute, useValue: activatedRouteMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: EntryService, useValue: entryServiceMock},
            ],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        form = formBuilder.group({
            id: formBuilder.control(null),
            type: formBuilder.control(ExternalLinkType.None, [Validators.required]),
            url: formBuilder.control(null, [Validators.required]),
        });

        fixture = TestBed.createComponent(ExternalLinkEntryComponent);
        component = fixture.componentInstance;

        component.form = form;

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should use defaults on init', () => {
        expect(component.mode).toEqual(ReferenceEntryMode.Create);
    });
});
