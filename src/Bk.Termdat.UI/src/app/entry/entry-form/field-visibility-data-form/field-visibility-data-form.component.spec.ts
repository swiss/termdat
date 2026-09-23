import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatIconModule} from '@angular/material/icon';
import {MatMenuItem, MatMenuModule} from '@angular/material/menu';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonModule} from '@oblique/oblique';
import {MockModule, MockPipe} from 'ng-mocks';
import {FieldVisibilityDataFormComponent} from './field-visibility-data-form.component';
import {FieldVisibility} from './FieldVisibility';

describe('FieldVisibilityDataFormComponent', () => {
    let component: FieldVisibilityDataFormComponent;
    let fixture: ComponentFixture<FieldVisibilityDataFormComponent>;

    let visibility: FieldVisibility;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatMenuModule),
                MockModule(MatCheckboxModule),
                MockModule(ObButtonModule),
                MockModule(MatTooltipModule),
                MockModule(MatIconModule),
                FieldVisibilityDataFormComponent,
                MockPipe(TranslatePipe),
            ],
            providers: [],
        }).compileComponents();

        visibility = {
            abbreviation: true,
            context: true,
            country: true,
            definition: true,
            metadata: true,
            name: true,
            note: true,
            phraseology: true,
            terminus: true,
        };

        fixture = TestBed.createComponent(FieldVisibilityDataFormComponent);
        component = fixture.componentInstance;
        component.fieldVisibility = visibility;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it.each([
        [
            true,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: false,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: false,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: false,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: false,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: false,
                name: true,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: false,
                note: true,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: false,
                phraseology: true,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: false,
                terminus: true,
            },
        ],
        [
            false,
            {
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: true,
                note: true,
                phraseology: true,
                terminus: false,
            },
        ],
    ])('should set %s to showAll when %s fields are set', (expected: boolean, fieldVisibility: FieldVisibility) => {
        component.fieldVisibility = fieldVisibility;

        expect(component.showAll).toBe(expected);
    });

    it.each([
        ['terminus', true],
        ['terminus', false],
        ['name', true],
        ['name', false],
        ['abbreviation', true],
        ['abbreviation', false],
        ['phraseology', true],
        ['phraseology', false],
        ['definition', true],
        ['definition', false],
        ['note', true],
        ['note', false],
        ['context', true],
        ['context', false],
        ['metadata', true],
        ['metadata', false],
        ['country', true],
        ['country', false],
    ])('should set %s in fieldVisibility to %s', (name: string, show: boolean) => {
        const fieldVisibility: FieldVisibility = {} as FieldVisibility;
        const key = name as keyof FieldVisibility;
        fieldVisibility[key] = !show;
        component.fieldVisibility = fieldVisibility;

        let emitted;
        component.visibilityChanged.subscribe(value => (emitted = value));

        component.show(name, show);
        expect(component.fieldVisibility[key]).toEqual(show);
        expect(emitted).toEqual(component.fieldVisibility);
    });

    it('should update showAll when field is shown or hidden', () => {
        expect(component.showAll).toEqual(true);

        component.show('name', false);
        expect(component.showAll).toEqual(false);

        component.show('name', true);
        expect(component.showAll).toEqual(true);
    });

    it.each([[true], [false]])('should select all with %s', (show: boolean) => {
        let emitted;
        component.visibilityChanged.subscribe(value => (emitted = value));

        component.selectAll(show);

        expect(component.showAll).toEqual(show);
        expect(emitted).toEqual(component.fieldVisibility);
    });

    it('should always show terminus and name for selectAll', () => {
        let emitted;
        component.visibilityChanged.subscribe(value => (emitted = value));

        component.selectAll(false);

        expect(component.showAll).toEqual(false);
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
        expect(emitted).toEqual(component.fieldVisibility);
    });

    it('should set visibility according to enabled status', () => {
        let emittedCount = 0;
        component.visibilityChanged.subscribe(_ => emittedCount++);

        component.enableDefinition = {enabled: true};
        expect(component.fieldNames).toEqual(expect.arrayContaining(['definition']));
        expect(component.fieldVisibility.definition).toBe(true);

        component.enableDefinition = {enabled: false};
        expect(component.fieldNames).toEqual(expect.not.arrayContaining(['definition']));
        expect(component.fieldVisibility.definition).toBe(true);

        expect(emittedCount).toBe(2);
    });

    describe('render tests', () => {
        let element: DebugElement;
        let menuButton: DebugElement;
        const clickEvent = new MouseEvent('click');
        const keydownEvent = new KeyboardEvent('keydown');

        beforeEach(() => {
            element = fixture.debugElement;
            menuButton = element.query(By.css('button'));
            menuButton.triggerEventHandler('click', clickEvent);
        });

        it('should select all on menu interaction', () => {
            const menuItems = element.queryAll(By.directive(MatMenuItem));

            expect(component.showAll).toEqual(true);

            menuItems[0].triggerEventHandler('click', clickEvent);
            expect(component.showAll).toEqual(false);

            menuItems[0].triggerEventHandler('keydown.enter', keydownEvent);
            expect(component.showAll).toEqual(true);
        });

        it('should set specific visibility on menu interaction', () => {
            const menuItems = element.queryAll(By.directive(MatMenuItem));

            expect(component.showAll).toEqual(true);

            menuItems[1].triggerEventHandler('click', clickEvent);
            expect(component.showAll).toEqual(false);
            expect(component.fieldVisibility).toEqual({
                abbreviation: true,
                context: true,
                country: true,
                definition: true,
                metadata: true,
                name: false,
                note: true,
                phraseology: true,
                terminus: true,
            });

            menuItems[1].triggerEventHandler('keydown.enter', keydownEvent);
            expect(component.showAll).toEqual(true);
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
    });
});
