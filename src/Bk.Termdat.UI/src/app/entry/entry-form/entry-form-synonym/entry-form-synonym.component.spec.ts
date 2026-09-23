import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule, ValidationErrors} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {EntryLanguageDetail} from 'api/EntryLanguageDetail';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {ErrorService} from '../../../shared/error.service';
import {EntryFormService} from '../entry-form.service';
import {FieldVisibilityDataFormComponent} from '../field-visibility-data-form/field-visibility-data-form.component';
import {FieldVisibility} from '../field-visibility-data-form/FieldVisibility';
import {NoteTemplatesComponent} from '../note-templates/note-templates.component';
import {SourceTemplatesComponent} from '../source-templates/source-templates.component';
import {EntryFormSynonymComponent} from './entry-form-synonym.component';
import {SynonymMoveDirection} from './SynonymMoveDirection';

describe('EntryFormSynonymComponent', () => {
    let component: EntryFormSynonymComponent;
    let fixture: ComponentFixture<EntryFormSynonymComponent>;

    const translateServiceMock = {
        instant: jest.fn(),
    };
    const errorServiceMock = {
        getControlError: jest.fn(),
    };
    let entryFormService: EntryFormService;
    let detail: EntryLanguageDetail;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatTooltipModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                EntryFormSynonymComponent,
                MockComponent(SourceTemplatesComponent),
                MockComponent(NoteTemplatesComponent),
                MockComponent(FieldVisibilityDataFormComponent),
                MockPipe(TranslatePipe),
            ],
            providers: [{provide: TranslateService, useValue: translateServiceMock}, {provide: ErrorService, useValue: errorServiceMock}, EntryFormService],
        }).compileComponents();

        detail = {
            abbreviation: 'abbreviation',
            abbreviationSource: 'abbreviationSource',
            context: 'context',
            contextSource: 'contextSource',
            country: 'country',
            definition: 'definition',
            definitionSource: 'definitionSource',
            id: 1,
            sequence: 1,
            languageId: 1,
            metadata: 'metadata',
            metadataSource: 'metadataSource',
            name: 'name',
            nameSource: 'nameSource',
            note: 'note',
            noteSource: 'noteSource',
            phraseology: 'phraseology',
            phraseologySource: 'phraseologySource',
            terminus: 'terminus',
            terminusSource: 'terminusSource',
        };

        entryFormService = TestBed.inject(EntryFormService);

        translateServiceMock.instant.mockImplementation(value => value);
        errorServiceMock.getControlError.mockImplementation((errors: ValidationErrors | null, label: string, labelPrefix: string) => {
            if (errors === null) {
                return '';
            }

            const errorKeys = Object.keys(errors);
            return `${labelPrefix}.${errorKeys[0]}`;
        });

        fixture = TestBed.createComponent(EntryFormSynonymComponent);
        component = fixture.componentInstance;

        component.form = entryFormService.createLanguageDetailFormGroup(detail);
        component.title = 'test';

        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default language to de', () => {
        expect(component.language).toEqual('de');
    });

    it('should create field visibility', () => {
        expect(component.fieldVisibility).toEqual({
            abbreviation: true,
            context: true,
            country: true,
            definition: true,
            metadata: true,
            name: true,
            note: true,
            phraseology: true,
            terminus: true,
        });
    });

    it('should update field visibility when form changes', async () => {
        component.form = entryFormService.createLanguageDetailFormGroup();
        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.fieldVisibility).toEqual({
            abbreviation: false,
            context: false,
            country: false,
            definition: false,
            metadata: false,
            name: true,
            note: false,
            phraseology: false,
            terminus: true,
        });
    });

    it('should emit synonymDeleted when delete button is clicked', () => {
        const element = fixture.debugElement;
        const deleteButton = element.query(By.css('.delete-button'));

        let deleted = false;
        component.synonymDeleted.subscribe(() => (deleted = true));

        deleteButton.triggerEventHandler('click', {});

        expect(deleted).toEqual(true);
    });

    it.each([{direction: SynonymMoveDirection.Down}, {direction: SynonymMoveDirection.Up}])(
        'should emit synonymSorted correctly for direction: $direction',
        ({direction}) => {
            let synonymSorted;
            component.synonymSorted.subscribe(value => (synonymSorted = value));

            component.changeSort(direction);

            expect(synonymSorted).toEqual(direction);
        }
    );

    it('should select source template by control name', () => {
        component._form.get('terminusSource')?.setValue(null);
        expect(component._form.dirty).toEqual(false);
        expect(component._form.touched).toEqual(false);

        component.sourceTemplateSelected('MY TEMPLATE', 'terminusSource');

        expect(component._form.value.terminusSource).toEqual('MY TEMPLATE');
        expect(component._form.dirty).toEqual(true);
        expect(component._form.touched).toEqual(true);
    });

    it('should append source template by control name if control already has value', () => {
        expect(component._form.value.terminusSource).toEqual('terminusSource');

        component.sourceTemplateSelected('MY TEMPLATE', 'terminusSource');

        expect(component._form.value.terminusSource).toEqual('terminusSource; MY TEMPLATE');
    });

    it('should only add selected source template if control exists', () => {
        expect(component._form.value?.superSource).toBeUndefined();

        component.sourceTemplateSelected('MY TEMPLATE', 'superSource');

        expect(component._form.value?.superSource).toBeUndefined();
    });

    it('should update fieldVisibility when visibilityChanged is emitted', () => {
        const element = fixture.debugElement;
        const visibilityComponent = element.query(By.directive(FieldVisibilityDataFormComponent));
        const newVisibility = {terminus: true} as FieldVisibility;

        expect(component.fieldVisibility).not.toEqual(newVisibility);

        visibilityComponent.triggerEventHandler('visibilityChanged', newVisibility);

        expect(component.fieldVisibility).toEqual(newVisibility);
    });

    it('should get invisible control errors without duplicates', async () => {
        component.form = entryFormService.createLanguageDetailFormGroup();
        component._form.markAllAsTouched();

        component._form.get('noteSource')?.setValue('test');

        fixture.detectChanges();
        await fixture.whenStable();

        const errors = component.getInvisibleControlsErrors();
        expect(errors).toEqual(['entry.error.atLeastOne', 'entry.error.dependency.required']);
    });

    it('should only get invisible controls, that are touched', async () => {
        component._form = entryFormService.createLanguageDetailFormGroup();
        fixture.detectChanges();
        await fixture.whenStable();

        const errors = component.getInvisibleControlsErrors();
        expect(errors).toEqual([]);
    });
});
