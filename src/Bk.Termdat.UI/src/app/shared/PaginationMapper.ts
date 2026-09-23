import {PageEvent} from '@angular/material/paginator';
import {PagingParameters} from 'api/PagingParameters';
import {Pagination} from './Pagination';

export const mapToPagingParameters = (pagination: Pagination): PagingParameters => {
    return {
        pageSize: pagination?.pageSize ?? 25,
        pageIndex: (pagination?.pageIndex ?? 0) + 1,
    };
};

export const mapToPagination = (pageEvent: PageEvent): Pagination => {
    return {
        pageSize: pageEvent.pageSize,
        pageIndex: pageEvent.pageIndex,
    };
};

export const createDefaultPagination = (): Pagination => {
    return {
        pageSize: 25,
        pageIndex: 0,
    };
};

export const defaultPageSize = [25, 50, 75, 100];
export const writerPageSize = [25, 50, 75, 100, 250];
export const superReaderPageSize = [25, 50, 75, 100, 250];
export const authenticatedPageSize = [25, 50, 75, 100, 250, 500, 750, 1000, 5000];
