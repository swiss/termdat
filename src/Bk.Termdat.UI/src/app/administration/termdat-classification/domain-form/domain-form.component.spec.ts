import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {MockModule, MockPipe} from 'ng-mocks';
import {skip} from 'rxjs/operators';
import {TermdatClassificationDomainFormComponent} from './domain-form.component';

describe('TermdatClassificationDomainFormComponent', () => {
    let component: TermdatClassificationDomainFormComponent;
    let fixture: ComponentFixture<TermdatClassificationDomainFormComponent>;

    let formBuilder: FormBuilder;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatAutocompleteModule),
                MockModule(MatIconModule),
                MockModule(MatTooltipModule),
                TermdatClassificationDomainFormComponent,
                MockPipe(TranslatePipe),
            ],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(TermdatClassificationDomainFormComponent);
        component = fixture.componentInstance;
        component.form = formBuilder.group({
            id: {value: undefined, disabled: true},
            descriptionDe: formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionFr: formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionEn: formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionRm: formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
            descriptionIt: formBuilder.control('', [Validators.maxLength(1000), Validators.required]),
        });
        component.descriptions = [
            {
                descriptionDe: 'de_b',
                descriptionFr: 'fr_b',
                descriptionEn: 'en_b',
                descriptionRm: 'rm_b',
                descriptionIt: 'it_b',
            },
            {
                descriptionDe: 'de_c',
                descriptionFr: 'fr_c',
                descriptionEn: 'en_c',
                descriptionRm: 'rm_c',
                descriptionIt: 'it_c',
            },
            {
                descriptionDe: 'de_a',
                descriptionFr: 'fr_a',
                descriptionEn: 'en_a',
                descriptionRm: 'rm_a',
                descriptionIt: 'it_a',
            },
            {
                descriptionDe: 'de_b',
                descriptionFr: 'fr_b',
                descriptionEn: 'en_b',
                descriptionRm: 'rm_b',
                descriptionIt: 'it_b',
            },
            {
                descriptionDe: 'de_a',
                descriptionFr: 'fr_a',
                descriptionEn: 'en_a',
                descriptionRm: 'rm_a',
                descriptionIt: 'it_a',
            },
            {
                descriptionDe: 'de_c',
                descriptionFr: 'fr_c',
                descriptionEn: 'en_c',
                descriptionRm: 'rm_c',
                descriptionIt: 'it_c',
            },
            {
                descriptionDe: 'de_c',
                descriptionFr: 'fr_c',
                descriptionEn: 'en_c',
                descriptionRm: 'rm_c',
                descriptionIt: 'it_c',
            },
        ];

        component.ngOnInit();
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(component.form).toBeDefined();
    });

    it('should have all unique de-desctriptions sorted with empty value', done => {
        component.filteredDescriptionsDe$.subscribe(values => {
            expect(values).toEqual(['de_a', 'de_b', 'de_c']);
            done();
        });
    });

    it('should filter de-descriptions', done => {
        component.filteredDescriptionsDe$.pipe(skip(1)).subscribe(values => {
            expect(values).toEqual(['de_a']);
            done();
        });

        component.form.controls.descriptionDe.patchValue('de_a');
    });

    it('should filter fr-descriptions', done => {
        component.filteredDescriptionsFr$.pipe(skip(1)).subscribe(values => {
            expect(values).toEqual(['fr_a', 'fr_b', 'fr_c']);
            done();
        });

        component.form.controls.descriptionFr.patchValue('');
    });

    it('should filter en-descriptions', done => {
        component.filteredDescriptionsEn$.pipe(skip(1)).subscribe(values => {
            expect(values).toEqual(['en_a']);
            done();
        });

        component.form.controls.descriptionEn.patchValue('en_a');
    });

    it('should filter rm-descriptions', done => {
        component.filteredDescriptionsRm$.pipe(skip(1)).subscribe(values => {
            expect(values).toEqual(['rm_a', 'rm_b', 'rm_c']);
            done();
        });

        component.form.controls.descriptionRm.patchValue('rm_');
    });

    it('should filter it-descriptions', done => {
        component.filteredDescriptionsIt$.pipe(skip(1)).subscribe(values => {
            expect(values).toEqual(['it_b']);
            done();
        });

        component.form.controls.descriptionIt.patchValue('it_b');
    });
});
