import {Directive, EventEmitter, HostBinding, HostListener, Output} from '@angular/core';

@Directive({selector: '[tdDragAndDropUpload]'})
export class DragAndDropUploadDirective {
    @HostBinding('class.file-over') fileOver = false;
    @Output() readonly fileDropped = new EventEmitter<FileList>();

    @HostListener('dragover', ['$event']) onDragOver(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = true;
    }

    @HostListener('dragleave', ['$event']) public onDragLeave(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = false;
    }

    @HostListener('drop', ['$event']) public onDrop(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = false;
        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.fileDropped.emit(files);
        }
    }
}
