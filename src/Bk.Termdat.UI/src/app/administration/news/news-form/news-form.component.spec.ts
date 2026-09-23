import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatOptionModule, provideNativeDateAdapter} from '@angular/material/core';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective, WINDOW} from '@oblique/oblique';
import {NewsItemModification} from 'api/NewsItemModification';
import {MockDirectives, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {ConfigsService} from '../../../config.service';
import {NewsFormComponent} from './news-form.component';

const translateService = {
    getCurrentLang: jest.fn().mockReturnValue('de'),
    onLangChange: new Subject(),
    get: jest.fn((key: string) => of(key)),
    instant: jest.fn((key: string) => key),
};

const routeMock = {
    snapshot: {
        paramMap: {
            get: jest.fn(),
        },
    },
    paramMap: of({
        get: jest.fn(),
    }),
};

const configServiceMock = {
    frontendConfig: {
        froalaKey: 'test-key',
    },
};

describe('NewsFormComponent', () => {
    let component: NewsFormComponent;
    let fixture: ComponentFixture<NewsFormComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                NewsFormComponent,
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSlideToggleModule),
                MockModule(MatSelectModule),
                MockModule(MatOptionModule),
                MockModule(MatDatepickerModule),
                MockPipe(TranslatePipe),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
            providers: [
                {
                    provide: TranslateService,
                    useValue: translateService,
                },
                {
                    provide: ActivatedRoute,
                    useValue: routeMock,
                },
                {
                    provide: WINDOW,
                    useValue: window,
                },
                {
                    provide: ConfigsService,
                    useValue: configServiceMock,
                },
                provideNativeDateAdapter(),
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(NewsFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should patch form values when initialData is provided', () => {
        const newsItem: NewsItemModification = {
            isEnabled: true,
            validFrom: new Date('2025-01-01'),
            validTo: new Date('2025-01-02'),
            titleDe: 'Titel DE',
            titleFr: 'Titre FR',
            titleIt: 'Titolo IT',
            titleRm: 'Titlu RM',
            titleEn: 'Title EN',
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textRm: 'Text RM',
            textEn: 'Text EN',
        };

        fixture.componentRef.setInput('initialData', newsItem);
        fixture.detectChanges();

        expect(component.form.controls.titleDe.value).toBe('Titel DE');
        expect(component.form.controls.textDe.value).toBe('Text DE');
        expect(component.form.controls.validFrom.value).toEqual(newsItem.validFrom);
        expect(component.form.controls.validTo.value).toEqual(newsItem.validTo);
        expect(component.form.controls.isEnabled.value).toBe(true);
    });
});
