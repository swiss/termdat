import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatTableModule} from '@angular/material/table';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObAlertModule} from '@oblique/oblique';
import {MockModule, MockPipe} from 'ng-mocks';
import {Subject} from 'rxjs';
import {FileImportErrorComponent} from './file-import-error.component';

describe('FileImportErrorComponent', () => {
    let component: FileImportErrorComponent;
    let fixture: ComponentFixture<FileImportErrorComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MatTableModule, MockModule(ObAlertModule), FileImportErrorComponent, MockPipe(TranslatePipe)],
            providers: [{provide: TranslateService, useValue: translateServiceMock}],
        }).compileComponents();

        fixture = TestBed.createComponent(FileImportErrorComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set datasource on ngOnChanges', () => {
        component.errorList = ['test', 'values'];

        component.ngOnChanges();

        expect(component.dataSource.data).toEqual(component.errorList);
    });
});
