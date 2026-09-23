import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'fileSize'})
export class FileSizePipe implements PipeTransform {
    transform(value: number | undefined | null, decimals = 2): string {
        if (value === null || value === undefined) {
            return '';
        }

        return FileSizePipe.formatBytes(value, decimals);
    }

    private static formatBytes(bytes: number, decimals: number) {
        if (bytes === 0) {
            return '0 Bytes';
        }

        const k = 1024;
        const dm = decimals < 0 ? 0 : decimals;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return `${parseFloat((bytes / k ** i).toFixed(dm))} ${sizes[i]}`;
    }
}
