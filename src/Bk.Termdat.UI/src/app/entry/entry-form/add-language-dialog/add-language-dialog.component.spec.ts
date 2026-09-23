import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {LanguageSummary} from 'api/LanguageSummary';
import {MockModule} from 'ng-mocks';
import {AddLanguageDialogComponent} from './add-language-dialog.component';

describe('AddLanguageDialogComponent', () => {
    let component: AddLanguageDialogComponent;
    let fixture: ComponentFixture<AddLanguageDialogComponent>;
    const dialogRefMock = {
        close: jest.fn(),
    };
    let data: LanguageSummary[] = [];

    beforeEach(async () => {
        data = [{id: 1, name: 'German'} as LanguageSummary, {id: 2, name: 'French'} as LanguageSummary, {id: 3, name: 'Italian'} as LanguageSummary];

        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatCheckboxModule),
                MockModule(MatDialogModule),
                MockModule(MatButtonModule),
                MockModule(TranslateModule),
                AddLanguageDialogComponent,
            ],
            providers: [
                {provide: MatDialogRef, useValue: dialogRefMock},
                {provide: MAT_DIALOG_DATA, useValue: data},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(AddLanguageDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should setup checkboxes from dialog data', () => {
        expect(component.form.controls).toHaveLength(3);
        expect(component.form.controls[0].value).toEqual(false);
        expect(component.form.controls[1].value).toEqual(false);
        expect(component.form.controls[2].value).toEqual(false);
    });

    it('displays error when no checkbox is checked', () => {
        component.form.markAsTouched();
        fixture.detectChanges();

        expect(fixture.debugElement.query(By.css('.add-language-error'))).toBeTruthy();

        component.form.controls[0].setValue(true);
        fixture.detectChanges();

        expect(fixture.debugElement.query(By.css('.add-language-error'))).toBeNull();
    });

    it('doesnt close dialog when form is invalid', () => {
        component.confirmDialog();

        expect(dialogRefMock.close).not.toHaveBeenCalled();
    });

    it('returns selected languages on confirm', () => {
        component.form.controls[0].setValue(true);
        fixture.detectChanges();

        component.confirmDialog();

        expect(dialogRefMock.close).toHaveBeenCalledTimes(1);
        expect(dialogRefMock.close).toHaveBeenCalledWith([data[0]]);
    });
});
