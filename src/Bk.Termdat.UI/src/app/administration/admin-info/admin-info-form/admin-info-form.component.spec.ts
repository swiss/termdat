import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {ActivatedRoute} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {AdminInfoFormComponent} from './admin-info-form.component';

describe('AdminInfoFormComponent', () => {
    let component: AdminInfoFormComponent;
    let fixture: ComponentFixture<AdminInfoFormComponent>;

    const translateServiceMock = {
        instant: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                ReactiveFormsModule,
                AdminInfoFormComponent,
                MockPipe(TranslatePipe),
                MockDirective(ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ActivatedRoute, useValue: {}},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(AdminInfoFormComponent);
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
            code: 'code1',
            description: 'de',
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
