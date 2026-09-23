import {NgClass} from '@angular/common';
import {Component, ElementRef, EventEmitter, Input, Output, ViewChild} from '@angular/core';
import {MatIcon} from '@angular/material/icon';
import {MatProgressBar} from '@angular/material/progress-bar';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObAlertComponent, ObTranslateParamsPipe} from '@oblique/oblique';
import {DragAndDropUploadDirective} from '../../../shared/drag-and-drop-upload.directive';
import {FileSizePipe} from '../../../shared/file-size.pipe';

@Component({
    selector: 'td-file-import-upload',
    templateUrl: './file-import-upload.component.html',
    styleUrls: ['./file-import-upload.component.scss'],
    imports: [ObAlertComponent, MatIcon, NgClass, TranslatePipe, MatProgressBar, DragAndDropUploadDirective],
    providers: [FileSizePipe, ObTranslateParamsPipe],
})
export class FileImportUploadComponent {
    @ViewChild('fileDrop', {static: false}) fileDrop!: ElementRef;

    loading = false;
    errorMessage: string | undefined = undefined;
    @Input() maxFileSize: number = 0;
    @Input() supportedFileTypes: string[] = [];

    @Output() readonly importedFile = new EventEmitter<File>();

    constructor(
        private readonly translateService: TranslateService,
        private readonly fileSizePipe: FileSizePipe,
        private readonly obTranslateParamsPipe: ObTranslateParamsPipe
    ) {}

    onFileChange(event: Event): void {
        this.errorMessage = undefined;

        if (!event?.target) {
            this.errorMessage = this.translateService.instant('file.import.error.general');
            return;
        }

        this.readFile((event.target as HTMLInputElement).files);
    }

    onFileDropped(files: FileList): void {
        this.readFile(files);
    }

    private readFile(files: FileList | null): void {
        this.loading = true;
        this.errorMessage = undefined;

        if (!files?.length) {
            this.errorMessage = this.translateService.instant('file.import.error.general');
            this.loading = false;
            return;
        }

        if (files.length !== 1) {
            this.errorMessage = this.translateService.instant('file.import.error.multiple');
            this.loading = false;
            return;
        }

        const file = files.item(0)!;
        const lastDot = file.name.lastIndexOf('.');
        const fileExtension = lastDot !== -1 ? file.name.substring(lastDot).toLowerCase() : '';

        if (!this.supportedFileTypes.includes(fileExtension)) {
            this.errorMessage = this.obTranslateParamsPipe.transform('file.import.error.fileType', {fileTypes: this.supportedFileTypes.join(', ')});
            this.loading = false;
            return;
        }

        if (file.size > this.maxFileSize) {
            const formattedSize = this.fileSizePipe.transform(this.maxFileSize, 0);
            this.errorMessage = this.translateService.instant('file.import.error.size', {size: formattedSize});
            this.loading = false;

            return;
        }

        this.loading = false;
        this.importedFile.emit(file);
    }
}
