import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import {NgClass} from '@angular/common';
import {Component, EventEmitter, Inject, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, ValidatorFn} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import {MatIcon} from '@angular/material/icon';
import {MatInput} from '@angular/material/input';
import {MatProgressBar} from '@angular/material/progress-bar';
import {DomSanitizer, SafeUrl} from '@angular/platform-browser';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObAlertComponent, ObButtonDirective, ObTranslateParamsPipe} from '@oblique/oblique';
import {PictureDetail} from 'api/PictureDetail';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {DragAndDropUploadDirective} from '../../../shared/drag-and-drop-upload.directive';
import {ErrorService} from '../../../shared/error.service';
import {FileSizePipe} from '../../../shared/file-size.pipe';
import {CONSOLE} from '../../../shared/injection.tokens';
import {PictureUploadError} from './picture-upload-error.enum';
import {pictureValidator} from './picture.validator';
import {PictureChange} from './PictureChange';

@Component({
    selector: 'td-entry-form-picture',
    templateUrl: './entry-form-picture.component.html',
    styleUrls: ['./entry-form-picture.component.scss'],
    imports: [
        NgClass,
        MatButton,
        ObButtonDirective,
        MatIcon,
        ReactiveFormsModule,
        MatFormField,
        MatLabel,
        MatInput,
        CdkTextareaAutosize,
        MatError,
        ObAlertComponent,
        DragAndDropUploadDirective,
        MatProgressBar,
        FileSizePipe,
        ObTranslateParamsPipe,
        TranslatePipe,
    ],
})
export class EntryFormPictureComponent implements OnInit, OnDestroy {
    @Input() form!: UntypedFormGroup;
    @Output() readonly pictureChanged = new EventEmitter<PictureChange | null>();

    loading = false;
    error: PictureUploadError | undefined;
    PictureUploadError = PictureUploadError;
    displaySize: number;
    pictureData: SafeUrl | undefined;
    timestamp: number;

    _picture?: PictureDetail;
    @Input() set picture(value: PictureDetail | undefined) {
        this._picture = value;
        if (this.form) {
            this.modelToForm(value);
            this.error = undefined;
        }
    }

    private static readonly MAX_FILE_SIZE_UPLOAD = 2097152;
    private static readonly ALLOWED_TYPES_UPLOAD = ['image/jpeg', 'image/png', 'image/gif'];
    private readonly unsubscribe = new Subject<void>();

    constructor(
        public readonly errorService: ErrorService,
        private readonly formBuilder: UntypedFormBuilder,
        private readonly sanitizer: DomSanitizer,
        private readonly translateService: TranslateService,
        @Inject(CONSOLE) private readonly console: Console
    ) {
        this.displaySize = EntryFormPictureComponent.MAX_FILE_SIZE_UPLOAD;
        this.timestamp = EntryFormPictureComponent.getTimestamp();
    }

    ngOnInit(): void {
        this.setupForm();

        this.pictureChanged.pipe(takeUntil(this.unsubscribe)).subscribe(value => {
            this.form.controls.image.setValue(value !== null);
            this.pictureData = value?.data;

            this.markForm();
        });
    }

    ngOnDestroy() {
        this.pictureChanged.complete();
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    deletePictureData(): void {
        this.picture = undefined;
        this.pictureChanged.emit(null);
    }

    deletePicture() {
        if (this._picture) {
            this._picture.entryId = undefined;
        }
        this.pictureChanged.emit(null);
    }

    onFileChange(event: Event): void {
        if (!event?.target) {
            this.error = PictureUploadError.General;
            return;
        }

        this.loading = true;
        const target = event.target as HTMLInputElement;
        const files = target.files as FileList;

        this.readFile(files);
    }

    onFileDropped(files: FileList): void {
        this.loading = true;
        this.readFile(files);
    }

    get hasData(): boolean {
        if (!this.form) {
            return false;
        }

        return (
            this.form.value &&
            (this.form.value.sourceDe ||
                this.form.value.descriptionDe ||
                this.form.value.sourceFr ||
                this.form.value.descriptionFr ||
                this.form.value.sourceEn ||
                this.form.value.descriptionEn ||
                this.form.value.sourceIt ||
                this.form.value.descriptionIt ||
                this.form.value.sourceRm ||
                this.form.value.descriptionRm ||
                this.form.value.image)
        );
    }

    private static getTimestamp(): number {
        return Math.round(Date.now() / 1000);
    }

    private readFile(files: FileList): void {
        this.error = undefined;

        if (!files?.length) {
            this.error = PictureUploadError.General;
            this.loading = false;
            return;
        }

        if (files.length > 1) {
            this.error = PictureUploadError.Multiple;
            this.loading = false;
            return;
        }

        const file = files.item(0);

        if (!EntryFormPictureComponent.ALLOWED_TYPES_UPLOAD.includes(file!.type)) {
            this.error = PictureUploadError.FileType;
            this.loading = false;
            return;
        }

        if (file!.size > EntryFormPictureComponent.MAX_FILE_SIZE_UPLOAD) {
            this.error = PictureUploadError.Size;
            this.loading = false;
            return;
        }

        const reader = new FileReader();
        reader.readAsDataURL(file as Blob);

        reader.onload = () => {
            const data = reader.result as string;
            this.loading = false;
            this.timestamp = EntryFormPictureComponent.getTimestamp();

            this.pictureChanged.emit({
                file,
                data: this.sanitizer.bypassSecurityTrustUrl(data),
            } as PictureChange);
        };

        reader.onerror = error => {
            this.error = PictureUploadError.General;
            this.loading = false;
            this.console.error(error);
        };
    }

    private modelToForm(value?: PictureDetail) {
        this.form.patchValue({
            sourceDe: value?.sourceDe,
            descriptionDe: value?.descriptionDe,
            sourceFr: value?.sourceFr,
            descriptionFr: value?.descriptionFr,
            sourceEn: value?.sourceEn,
            descriptionEn: value?.descriptionEn,
            sourceIt: value?.sourceIt,
            descriptionIt: value?.descriptionIt,
            sourceRm: value?.sourceRm,
            descriptionRm: value?.descriptionRm,
            image: !!value?.entryId,
        });
    }

    private markForm(): void {
        this.form.markAsTouched();
        this.form.markAsDirty();
    }

    private setupForm(): void {
        this.form.setControl('sourceDe', this.formBuilder.control(this._picture?.sourceDe));
        this.form.setControl('descriptionDe', this.formBuilder.control(this._picture?.descriptionDe));
        this.form.setControl('sourceFr', this.formBuilder.control(this._picture?.sourceFr));
        this.form.setControl('descriptionFr', this.formBuilder.control(this._picture?.descriptionFr));
        this.form.setControl('sourceEn', this.formBuilder.control(this._picture?.sourceEn));
        this.form.setControl('descriptionEn', this.formBuilder.control(this._picture?.descriptionEn));
        this.form.setControl('sourceIt', this.formBuilder.control(this._picture?.sourceIt));
        this.form.setControl('descriptionIt', this.formBuilder.control(this._picture?.descriptionIt));
        this.form.setControl('sourceRm', this.formBuilder.control(this._picture?.sourceRm));
        this.form.setControl('descriptionRm', this.formBuilder.control(this._picture?.descriptionRm));
        this.form.setControl('image', this.formBuilder.control(!!this._picture?.entryId));

        this.form.setValidators(pictureValidator() as ValidatorFn);
    }
}
