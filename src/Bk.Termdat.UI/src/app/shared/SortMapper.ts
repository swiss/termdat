import {Sort} from '@angular/material/sort';

export function mapToSortString(sort: Sort): string {
    if (sort?.active) {
        const direction = sort.direction === 'desc' ? '-' : '';
        return `${direction}${sort.active}`;
    }

    return '';
}
