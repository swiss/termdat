import {DragAndDropUploadDirective} from './drag-and-drop-upload.directive';

describe('DragAndDropUploadDirective', () => {
    let directive: DragAndDropUploadDirective;
    const event = {
        preventDefault: jest.fn(),
        stopPropagation: jest.fn(),
        dataTransfer: {
            files: [] as string[],
        },
    };

    beforeEach(() => {
        directive = new DragAndDropUploadDirective();
        event.dataTransfer = {
            files: [] as string[],
        };
    });

    afterEach(() => {
        event.stopPropagation.mockClear();
        event.preventDefault.mockClear();
    });

    it('should create an instance', () => {
        expect(directive).toBeTruthy();
    });

    it('should default fileOver to false', () => {
        expect(directive.fileOver).toEqual(false);
    });

    it('sets fileOver to true when onDragOver is called', () => {
        directive.onDragOver(event as unknown as Event);

        expect(directive.fileOver).toEqual(true);
        expect(event.preventDefault).toHaveBeenCalledTimes(1);
        expect(event.stopPropagation).toHaveBeenCalledTimes(1);
    });

    it('sets fileOver to false when onDragLeave is called', () => {
        directive.fileOver = true;

        directive.onDragLeave(event as unknown as Event);

        expect(directive.fileOver).toEqual(false);
        expect(event.preventDefault).toHaveBeenCalledTimes(1);
        expect(event.stopPropagation).toHaveBeenCalledTimes(1);
    });

    it('sets fileOver to false and emits dropped files when onDrop is called', () => {
        directive.fileOver = true;
        event.dataTransfer.files = ['1', '2', '3'];
        let droppedFiles;

        directive.fileDropped.subscribe(files => {
            droppedFiles = files;
        });

        directive.onDrop(event as unknown as DragEvent);

        expect(directive.fileOver).toEqual(false);
        expect(event.preventDefault).toHaveBeenCalledTimes(1);
        expect(event.stopPropagation).toHaveBeenCalledTimes(1);
        expect(droppedFiles).toEqual(['1', '2', '3']);
    });

    it('sets fileOver to false but emits nothing when onDrop is called without files', () => {
        directive.fileOver = true;
        let droppedFiles = null;

        directive.fileDropped.subscribe(files => {
            droppedFiles = files;
        });

        directive.onDrop(event as unknown as DragEvent);

        expect(directive.fileOver).toEqual(false);
        expect(event.preventDefault).toHaveBeenCalledTimes(1);
        expect(event.stopPropagation).toHaveBeenCalledTimes(1);
        expect(droppedFiles).toBeNull();
    });
});
