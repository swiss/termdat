import {HttpResponse} from '@angular/common/http';

export const downloadFile = (data: HttpResponse<Blob>, fallbackFileName: string): void => {
    const contentDisposition = data.headers.get('content-disposition') ?? '';
    const regExp = /filename\*=UTF-8''([^;]+?)(?:;|$)/i;
    const findFilename = regExp.exec(contentDisposition);
    const filename = findFilename && findFilename.length > 1 ? decodeURIComponent(findFilename[1].trim()) : fallbackFileName;
    download(data.body!, data.body!.type, filename);
};

export const downloadTxtFile = (data: string, filename: string): void => {
    download(new Blob([data]), 'text/plain;charset=utf-8', filename);
};

function download(content: BlobPart, fileType: string, filename: string) {
    const file = new Blob([content], {type: fileType});
    const anchor = document.createElement('a');
    anchor.setAttribute('style', 'display:none;');
    document.body.appendChild(anchor);
    anchor.download = filename;
    anchor.href = URL.createObjectURL(file);
    anchor.target = '_blank';
    anchor.click();
    document.body.removeChild(anchor);
}
