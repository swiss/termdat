import {FileSizePipe} from './file-size.pipe';

describe('SizePipe', () => {
    it('create an instance', () => {
        const pipe = new FileSizePipe();
        expect(pipe).toBeTruthy();
    });

    it('can transform', () => {
        const pipe = new FileSizePipe();

        expect(pipe.transform(1024)).toEqual('1 KB');
        expect(pipe.transform(3.25464 * 1024 * 1024)).toEqual('3.25 MB');
        expect(pipe.transform(120.3748923432 * 1024 * 1024 * 1024)).toEqual('120.37 GB');
        expect(pipe.transform(undefined)).toEqual('');
        expect(pipe.transform(null)).toEqual('');
        expect(pipe.transform(0)).toEqual('0 Bytes');
    });

    it('can transform with decimals setting', () => {
        const pipe = new FileSizePipe();

        expect(pipe.transform(1.73843 * 1024, 0)).toEqual('2 KB');
        expect(pipe.transform(3.25464 * 1024 * 1024, 1)).toEqual('3.3 MB');
        expect(pipe.transform(120.3748923432 * 1024 * 1024 * 1024, 4)).toEqual('120.3749 GB');
    });
});
