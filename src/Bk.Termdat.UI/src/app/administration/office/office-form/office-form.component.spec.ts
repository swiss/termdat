import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockDirectives, MockModule, MockPipe} from 'ng-mocks';
import {OfficeFormComponent} from './office-form.component';

describe('OfficeFormComponent', () => {
    let component: OfficeFormComponent;
    let fixture: ComponentFixture<OfficeFormComponent>;

    const translateServiceMock = {
        instant: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                ReactiveFormsModule,
                OfficeFormComponent,
                MockPipe(TranslatePipe),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
            providers: [{provide: TranslateService, useValue: translateServiceMock}],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(OfficeFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call save on save click', () => {
        component.form.patchValue({
            code: 'qwe',
            descriptionDe: 'de',
            descriptionFr: 'fr',
            descriptionIt: 'it',
            descriptionRm: 'rm',
            descriptionEn: 'en',
        });

        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).toHaveBeenCalledTimes(1);
    });

    it('should not save if form is invalid', () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });
});
