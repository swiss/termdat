import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {TranslatePipe} from '@ngx-translate/core';
import {MockModule, MockPipe} from 'ng-mocks';
import {EntryTemplateCreateDialogComponent} from './entry-template-create-dialog.component';

describe('EntryTemplateCreateDialogComponent', () => {
    let component: EntryTemplateCreateDialogComponent;
    let fixture: ComponentFixture<EntryTemplateCreateDialogComponent>;
    let formBuilder: FormBuilder;

    const dialogRefMock = {
        close: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatDialogModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                EntryTemplateCreateDialogComponent,
                MockPipe(TranslatePipe),
            ],
            providers: [{provide: MatDialogRef, useValue: dialogRefMock}],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(EntryTemplateCreateDialogComponent);
        component = fixture.componentInstance;

        component.templateForm = formBuilder.group({
            name: formBuilder.control(''),
        });

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('closes dialog on confirm', () => {
        component.ngOnInit();
        fixture.detectChanges();
        component.templateForm.controls.name.setValue('Hund');

        component.createTemplate();

        expect(dialogRefMock.close).toHaveBeenCalledTimes(1);
    });
});
