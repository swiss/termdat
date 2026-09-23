import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {ActivatedRoute} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {ClassificationSummary} from 'api/ClassificationSummary';
import {MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {ClassificationService} from '../classification.service';
import {CollectionFormComponent} from './collection-form.component';

describe('CollectionFormComponent', () => {
    let component: CollectionFormComponent;
    let fixture: ComponentFixture<CollectionFormComponent>;

    const onLangChangeSubject = new Subject<void>();
    const translateServiceMock = {
        onLangChange: onLangChangeSubject.asObservable(),
        instant: jest.fn(),
    };

    const classifications: ClassificationSummary[] = [
        {
            id: 1,
            code: 'No1',
            name: 'Erste Klasse',
        },
        {
            id: 2,
            code: 'No2',
            name: 'Zweite Klasse',
        },
        {
            id: 3,
            code: 'No3',
            name: 'Dritte Klasse',
        },
    ];
    const classificationServiceMock = {
        getClassifications: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSlideToggleModule),
                MockModule(MatSelectModule),
                MockModule(MatOptionModule),
                MockPipe(TranslatePipe),
                CollectionFormComponent,
                MockDirective(ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ClassificationService, useValue: classificationServiceMock},
                {provide: ActivatedRoute, useValue: {}},
            ],
        }).compileComponents();

        classificationServiceMock.getClassifications.mockReturnValue(of(classifications));

        fixture = TestBed.createComponent(CollectionFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();

        expect(classificationServiceMock.getClassifications).toHaveBeenCalledTimes(1);

        expect(component.classifications).toEqual(classifications);
    });

    it('should reload master data after language change', () => {
        jest.clearAllMocks();

        onLangChangeSubject.next();

        expect(classificationServiceMock.getClassifications).toHaveBeenCalledTimes(1);
    });

    it('should call save on click', () => {
        jest.spyOn(component.saved, 'emit');

        component.form.patchValue({
            code: '123',
            descriptionDe: 'de',
            descriptionFr: 'fr',
            descriptionIt: 'it',
            descriptionRm: 'rm',
            descriptionEn: 'en',
        });

        component.save();

        expect(component.saved.emit).toHaveBeenCalledTimes(1);
    });

    it('cannot save when invalid', async () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });
});
