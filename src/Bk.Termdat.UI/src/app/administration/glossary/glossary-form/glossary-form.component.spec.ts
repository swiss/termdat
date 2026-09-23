import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatOptionModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {RouterLink} from '@angular/router';
import {TranslatePipe} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockDirectives, MockModule, MockPipe} from 'ng-mocks';
import {GlossaryFormComponent} from './glossary-form.component';

describe('GlossaryFormComponent', () => {
    let component: GlossaryFormComponent;
    let fixture: ComponentFixture<GlossaryFormComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSlideToggleModule),
                MockModule(MatSelectModule),
                MockModule(MatOptionModule),
                GlossaryFormComponent,
                MockPipe(TranslatePipe),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(GlossaryFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('cannot save when invalid', async () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });
});
