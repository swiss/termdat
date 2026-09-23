import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatMenuModule} from '@angular/material/menu';
import {MatTooltipModule} from '@angular/material/tooltip';
import {TranslateModule} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {of} from 'rxjs';
import {SourceTemplatesComponent} from './source-templates.component';
import {SourceTemplatesService} from './source-templates.service';
import {SourceTemplateDefault} from './SourceTemplateDefault';
import {SourceTemplateType} from './SourceTemplateType';

describe('SourceTemplatesComponent', () => {
    let component: SourceTemplatesComponent;
    let fixture: ComponentFixture<SourceTemplatesComponent>;

    const sourceTemplateDefaults = {
        de: {
            federalLawAs: 'test',
        } as SourceTemplateDefault,
    };

    const sourceTemplateTexts = ['TextDe1', 'TextDe2', 'TextDe3', 'TextDe4'] as string[];

    const sourceTemplatesServiceMock = {
        getSourceTemplateDefaults: jest.fn().mockReturnValue(of(sourceTemplateDefaults)),
        getSourceTemplateTexts: jest.fn().mockReturnValue(of(sourceTemplateTexts)),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatIconModule),
                MockModule(MatMenuModule),
                MockModule(MatButtonModule),
                MockModule(MatTooltipModule),
                MockModule(TranslateModule),
                SourceTemplatesComponent,
            ],
            providers: [{provide: SourceTemplatesService, useValue: sourceTemplatesServiceMock}],
        }).compileComponents();

        fixture = TestBed.createComponent(SourceTemplatesComponent);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('language', 'de');

        fixture.detectChanges();
        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('loads source templates by user for the selected language', () => {
        expect(sourceTemplatesServiceMock.getSourceTemplateTexts).toHaveBeenCalledWith('de');
        expect(component.sourceTemplatesByUser()).toEqual(sourceTemplateTexts);
    });

    it('reloads source templates by user when the selected language changes', async () => {
        const sourceTemplateTextsFr = ['TextFr1', 'TextFr2'];
        sourceTemplatesServiceMock.getSourceTemplateTexts.mockReturnValueOnce(of(sourceTemplateTextsFr));

        fixture.componentRef.setInput('language', 'fr');
        fixture.detectChanges();
        await fixture.whenStable();

        expect(sourceTemplatesServiceMock.getSourceTemplateTexts).toHaveBeenCalledWith('fr');
        expect(component.sourceTemplatesByUser()).toEqual(sourceTemplateTextsFr);
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(component['templateSelected'], 'complete');
        component.ngOnDestroy();
        expect(component['templateSelected'].complete).toHaveBeenCalledTimes(1);
    });

    it('emits templateSelected when default template is selected', () => {
        let selected: string;
        component.templateSelected.subscribe(value => (selected = value));

        component.selectTemplate(SourceTemplateType.FederalLawAs);

        expect(selected!).toEqual('test');
    });

    it('emits templateSelected when user template is selected', () => {
        let selected: string;
        component.templateSelected.subscribe(value => (selected = value));

        component.selectTemplateByUser('test1');

        expect(selected!).toEqual('test1');
    });
});
