import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatMenuModule} from '@angular/material/menu';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslateModule} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {NoteTemplatesDataService} from './note-templates-data.service';
import {NoteTemplatesComponent} from './note-templates.component';
import {TemplateDataContainer} from './TemplateDataContainer';
import {TemplateMenu} from './TemplateMenu';

describe('NoteTemplatesComponent', () => {
    let component: NoteTemplatesComponent;
    let fixture: ComponentFixture<NoteTemplatesComponent>;

    const mockTemplateData: TemplateDataContainer = {
        de: [
            {
                label: 'A-DE',
                options: ['A1-DE', 'A2-DE'],
            },
            {
                label: 'B-DE',
                options: ['B1-DE', 'B2-DE'],
            },
        ],
        fr: [
            {
                label: 'A-FR',
                options: ['A1-FR', 'A2-FR'],
            },
        ],
        it: [
            {
                label: 'A-IT',
                options: [],
            },
        ],
        en: [
            {
                label: 'A-EN',
                options: [],
            },
        ],
        rm: [
            {
                label: 'A-RM',
                options: [],
            },
        ],
    };

    const noteTemplatesDataServiceMock = {
        getTemplateDataContainer: jest.fn().mockReturnValue(mockTemplateData),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatIconModule),
                MockModule(MatMenuModule),
                MockModule(MatButtonModule),
                MockModule(MatTooltipModule),
                MockModule(TranslateModule),
                NoteTemplatesComponent,
            ],
            providers: [{provide: NoteTemplatesDataService, useValue: noteTemplatesDataServiceMock}],
        }).compileComponents();

        fixture = TestBed.createComponent(NoteTemplatesComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Initialization', () => {
        it('should initialize with default language "de"', () => {
            expect(component.language).toBe('de');
        });
    });

    describe('Language Changes', () => {
        it('should update templateMenuItems when language changes with a supported language', () => {
            component.language = 'fr';
            component.ngOnChanges();

            expect(component.templateMenuItems).toEqual(mockTemplateData.fr);
        });

        it('should fallback to "de" when language is not supported', () => {
            component.language = 'es'; // unsupported language
            component.ngOnChanges();

            expect(component.templateMenuItems).toEqual(mockTemplateData.de);
        });

        it('should update templateMenuItems for all supported languages', () => {
            const supportedLanguages = ['de', 'fr', 'it', 'en', 'rm'];

            supportedLanguages.forEach(lang => {
                component.language = lang;
                component.ngOnChanges();
                expect(component.templateMenuItems).toEqual(mockTemplateData[lang as keyof TemplateDataContainer]);
            });
        });
    });

    describe('Template Menu Selection', () => {
        it('should set selectedTemplateMenu when setSelectedMenu is called', () => {
            const testMenu: TemplateMenu = mockTemplateData.de[0];

            component.setSelectedMenu(testMenu);

            expect(component.selectedTemplateMenu).toEqual(testMenu);
        });
    });

    describe('Template Selection', () => {
        it('should emit templateSelected when selectTemplate is called', () => {
            let emittedTemplate: string | undefined;
            component.templateSelected.subscribe(template => {
                emittedTemplate = template;
            });

            const testTemplate = 'XYZ';
            component.selectTemplate(testTemplate);

            expect(emittedTemplate).toBe(testTemplate);
        });
    });

    describe('NoteTemplatesDataService Integration', () => {
        it('should call getTemplateDataContainer when ngOnChanges triggers updateTemplateMenuItems', () => {
            component.ngOnChanges();

            expect(noteTemplatesDataServiceMock.getTemplateDataContainer).toHaveBeenCalledTimes(1);
        });
    });
});
