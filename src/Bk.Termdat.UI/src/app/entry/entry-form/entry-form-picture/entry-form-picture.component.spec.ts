import {readFileSync} from 'fs';
import {TextFieldModule} from '@angular/cdk/text-field';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObAlertModule, ObTranslateParamsModule} from '@oblique/oblique';
import {PictureDetail} from 'api/PictureDetail';
import {MockDirective, MockModule, MockPipe} from 'ng-mocks';
import {DragAndDropUploadDirective} from '../../../shared/drag-and-drop-upload.directive';
import {ErrorService} from '../../../shared/error.service';
import {FileSizePipe} from '../../../shared/file-size.pipe';
import {CONSOLE} from '../../../shared/injection.tokens';
import {EntryFormPictureComponent} from './entry-form-picture.component';
import {PictureUploadError} from './picture-upload-error.enum';

describe('EntryFormPictureComponent', () => {
    let component: EntryFormPictureComponent;
    let fixture: ComponentFixture<EntryFormPictureComponent>;

    const translateServiceMock = {
        instant: jest.fn().mockImplementation(v => v),
    };
    const consoleMock = {
        error: jest.fn(),
    };
    const errorServiceMock = {
        getControlError: jest.fn(),
    };
    let formBuilder: FormBuilder;
    let picture: PictureDetail;
    let form: FormGroup;
    let files: FileList;
    let file: File;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatFormFieldModule),
                MockModule(MatIconModule),
                MockModule(MatButtonModule),
                MockModule(MatProgressBarModule),
                MockModule(ObAlertModule),
                MockModule(ObTranslateParamsModule),
                MockModule(TranslateModule),
                MockModule(TextFieldModule),
                EntryFormPictureComponent,
                MockDirective(DragAndDropUploadDirective),
                MockPipe(FileSizePipe),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: CONSOLE, useValue: consoleMock},
                {provide: ErrorService, useValue: errorServiceMock},
            ],
        }).compileComponents();

        formBuilder = TestBed.inject(FormBuilder);
        file = getImageFile();

        // @ts-ignore
        files = {
            length: 1,
            item: jest.fn().mockReturnValue(file),
        };

        form = formBuilder.group({});
        picture = {
            entryId: 1,
            descriptionDe: 'test description',
            descriptionEn: null,
            descriptionFr: null,
            descriptionIt: null,
            descriptionRm: null,
            sourceDe: null,
            sourceEn: 'test source',
            sourceFr: null,
            sourceIt: null,
            sourceRm: null,
        } as unknown as PictureDetail;

        fixture = TestBed.createComponent(EntryFormPictureComponent);
        component = fixture.componentInstance;
        component.form = form;

        fixture.detectChanges();

        component.picture = picture;

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('setups picture form correctly', () => {
        expect(component.form.pristine).toEqual(true);
        expect(component.form.touched).toEqual(false);
        expect(component.form.value).toEqual({
            descriptionDe: 'test description',
            descriptionEn: null,
            descriptionFr: null,
            descriptionIt: null,
            descriptionRm: null,
            image: true,
            sourceDe: null,
            sourceEn: 'test source',
            sourceFr: null,
            sourceIt: null,
            sourceRm: null,
        });
        expect(component.form.valid).toBeTruthy();

        component.form.controls.image.setValue(false);

        expect(component.form.valid).toBeFalsy();
        expect(component.form.hasError('picture')).toBeTruthy();
    });

    it('updates form controls when picture is set', () => {
        picture.descriptionIt = 'it new';
        picture.sourceFr = 'fr new';

        component.picture = picture;

        expect(component.form.value).toEqual({
            descriptionDe: 'test description',
            descriptionEn: null,
            descriptionFr: null,
            descriptionIt: 'it new',
            descriptionRm: null,
            image: true,
            sourceDe: null,
            sourceEn: 'test source',
            sourceFr: 'fr new',
            sourceIt: null,
            sourceRm: null,
        });
    });

    it('can delete picture', done => {
        expect(form.touched).toEqual(false);
        expect(form.dirty).toEqual(false);
        expect(component._picture?.entryId).toBeDefined();

        component.pictureChanged.subscribe(value => {
            expect(component.error).toBeUndefined();
            expect(component.loading).toBeFalsy();
            expect(value).toBeNull();

            done();
        });

        component.deletePicture();

        expect(form.touched).toEqual(true);
        expect(form.dirty).toEqual(true);
        expect(component._picture?.entryId).not.toBeDefined();
    });

    it('can delete picture data', done => {
        expect(form.touched).toEqual(false);
        expect(form.dirty).toEqual(false);
        expect(component._picture).toBeDefined();

        component.pictureChanged.subscribe(value => {
            expect(component.error).toBeUndefined();
            expect(component.loading).toBeFalsy();
            expect(value).toBeNull();

            done();
        });

        component.deletePictureData();

        expect(form.touched).toEqual(true);
        expect(form.dirty).toEqual(true);
        expect(component._picture).toBeUndefined();
    });

    it('can get picture', () => {
        expect(component._picture).toEqual(picture);
    });

    it('can drop picture', done => {
        const oldTimestamp = component.timestamp;
        expect(oldTimestamp).toBeGreaterThan(0);
        expect(component.error).toBeUndefined();

        component.pictureChanged.subscribe(value => {
            expect(component.error).toBeUndefined();
            expect(component.loading).toBeFalsy();
            expect(component.timestamp).toBeGreaterThanOrEqual(oldTimestamp);
            expect(value).not.toBeNull();
            expect(value!.data).toMatch(/^data:image\//);
            expect(value!.file).toEqual(file);

            done();
        });

        component.onFileDropped(files);

        expect(component.loading).toEqual(true);
    });

    it('can choose picture', done => {
        const oldTimestamp = component.timestamp;
        expect(oldTimestamp).toBeGreaterThan(0);
        expect(component.error).toBeUndefined();

        component.pictureChanged.subscribe(value => {
            expect(component.error).toBeUndefined();
            expect(component.loading).toBeFalsy();
            expect(component.timestamp).toBeGreaterThanOrEqual(oldTimestamp);
            expect(value).not.toBeNull();
            expect(value!.data).toMatch(/^data:image\//);
            expect(value!.file).toEqual(file);

            done();
        });

        component.onFileChange({
            target: {
                files,
            },
        } as unknown as Event);

        expect(component.loading).toEqual(true);
    });

    it('adds general error when chosen image event is undefined', () => {
        expect(component.error).toBeUndefined();

        component.onFileChange(undefined as unknown as Event);

        expect(component.error).toEqual(PictureUploadError.General);
        expect(component.loading).toBeFalsy();
    });

    it('adds general error when chosen image event target is undefined', () => {
        expect(component.error).toBeUndefined();

        component.onFileChange({} as Event);

        expect(component.error).toEqual(PictureUploadError.General);
        expect(component.loading).toBeFalsy();
    });

    it('adds general error when chosen image event target contains no files', () => {
        expect(component.error).toBeUndefined();

        component.onFileChange({
            target: {
                files: [],
            },
        } as unknown as Event);

        expect(component.error).toEqual(PictureUploadError.General);
        expect(component.loading).toBeFalsy();
    });

    it('adds multiple error when chosen image event target contains no files', () => {
        expect(component.error).toBeUndefined();

        component.onFileChange({
            target: {
                files: [{}, {}],
            },
        } as unknown as Event);

        expect(component.error).toEqual(PictureUploadError.Multiple);
        expect(component.loading).toBeFalsy();
    });

    it('adds fileType error when chosen file is not allowed', () => {
        expect(component.error).toBeUndefined();

        const newFile = {
            type: 'application/pdf',
        } as File;
        component.onFileChange({
            target: {
                files: {
                    length: 1,
                    item: jest.fn().mockReturnValue(newFile),
                },
            },
        } as unknown as Event);

        expect(component.error).toEqual(PictureUploadError.FileType);
        expect(component.loading).toBeFalsy();
    });

    it('adds size error when chosen file is to big', () => {
        expect(component.error).toBeUndefined();

        const newFile = {
            type: 'image/png',
            size: 3 * 1024 * 1024,
        } as File;
        component.onFileChange({
            target: {
                files: {
                    length: 1,
                    item: jest.fn().mockReturnValue(newFile),
                },
            },
        } as unknown as Event);

        expect(component.error).toEqual(PictureUploadError.Size);
        expect(component.loading).toBeFalsy();
    });
});

function getImageFile() {
    const imgFile = readFileSync('src/app/entry/entry-form/entry-form-picture/test-fixture.jpg');
    const arrayBuffer = Uint8Array.from(imgFile).buffer;
    return new File([arrayBuffer], 'test.jpg', {type: 'image/jpeg'});
}
