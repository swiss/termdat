import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatIconModule} from '@angular/material/icon';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {MockModule, MockPipe} from 'ng-mocks';
import {FileImportUploadComponent} from './file-import-upload.component';

describe('FileImportUploadComponent', () => {
    let component: FileImportUploadComponent;
    let fixture: ComponentFixture<FileImportUploadComponent>;
    let files: FileList;
    let file: File;

    const translateServiceMock = {
        instant: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MockModule(MatIconModule), FileImportUploadComponent, MockPipe(TranslatePipe)],
            providers: [{provide: TranslateService, useValue: translateServiceMock}],
        }).compileComponents();

        file = getFile();
        // @ts-ignore
        files = {
            length: 1,
            item: jest.fn().mockReturnValue(file),
        };

        fixture = TestBed.createComponent(FileImportUploadComponent);
        component = fixture.componentInstance;
        component.maxFileSize = 1024;
        component.supportedFileTypes = ['.tbx'];
        fixture.detectChanges();

        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('can drop file', () => {
        expect(component.errorMessage).toBeUndefined();

        let receivedFile;
        component.importedFile.subscribe(value => (receivedFile = value));

        component.onFileDropped(files);

        expect(component.errorMessage).toBeUndefined();
        expect(component.loading).toBeFalsy();
        expect(receivedFile).not.toBeNull();
        expect(receivedFile).toEqual(file);
    });

    it('can choose file', () => {
        expect(component.errorMessage).toBeUndefined();

        let receivedFile: File;
        component.importedFile.subscribe(value => (receivedFile = value));

        component.onFileChange({
            target: {
                files,
            },
        } as unknown as Event);

        expect(component.errorMessage).toBeUndefined();
        expect(component.loading).toBeFalsy();
        expect(receivedFile!).not.toBeNull();
        expect(receivedFile!).toEqual(file);
        expect(receivedFile!.name).toEqual('test.tbx');
    });

    function getFile() {
        return new File([], 'test.tbx');
    }
});
