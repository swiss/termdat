import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatOptionModule} from '@angular/material/core';
import {MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject} from 'rxjs';
import {EntryTemplateService} from '../../shared/entry-template/entry-template.service';
import {EntryTemplateSelectionDialogComponent} from './entry-template-selection-dialog.component';

describe('EntryTemplateSelectionDialogComponent', () => {
    let component: EntryTemplateSelectionDialogComponent;
    let fixture: ComponentFixture<EntryTemplateSelectionDialogComponent>;
    let formBuilder: FormBuilder;

    const dialogRefMock = {
        close: jest.fn(),
    };
    const langChangeSubject = new BehaviorSubject<{lang: string}>({lang: 'de'});
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
    };
    const entryTemplateServiceMock = {
        getEntryTemplates: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatDialogModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSelectModule),
                MockModule(MatOptionModule),
                EntryTemplateSelectionDialogComponent,
                MockPipe(TranslatePipe),
            ],
            providers: [
                {provide: MatDialogRef, useValue: dialogRefMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: EntryTemplateService, useValue: entryTemplateServiceMock},
            ],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);

        fixture = TestBed.createComponent(EntryTemplateSelectionDialogComponent);
        component = fixture.componentInstance;

        component.templateSelectionForm = formBuilder.group({
            entryTemplateId: formBuilder.control(null),
        });

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize templates', () => {
        jest.resetAllMocks();
        component.ngOnInit();

        expect(entryTemplateServiceMock.getEntryTemplates).toHaveBeenCalledTimes(1);
    });

    it('closes dialog on confirm with the selected template-id', () => {
        component.ngOnInit();
        fixture.detectChanges();
        component.templateSelectionForm.controls.entryTemplateId.setValue(42);

        component.selectTemplate();

        expect(dialogRefMock.close).toHaveBeenCalledTimes(1);
        expect(dialogRefMock.close).toHaveBeenCalledWith(42);
    });
});
