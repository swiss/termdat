import {HttpParams} from '@angular/common/http';

export const appendMany = (params: HttpParams, name: string, ids?: number[] | string[]): HttpParams => {
    if (!ids?.length) {
        return params;
    }

    ids.forEach(id => {
        params = params.append(name, id);
    });

    return params;
};

export const append = (params: HttpParams, name: string, value?: string | Date | boolean | number) => {
    if (value === null || value === undefined || value === '' || value === false) {
        return params;
    }

    if (value instanceof Date) {
        return params.set(name, value.toISOString());
    }

    return params.set(name, value);
};
