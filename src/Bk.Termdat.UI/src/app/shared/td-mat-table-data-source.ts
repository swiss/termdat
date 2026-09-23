import {_isNumberValue} from '@angular/cdk/coercion';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {get} from 'lodash';

// Implementation of MatTableDataSource with
//   * custom sortData
//   * custom sortingDataAccessor
// Implementations are modified source-copies of MatTableDataSource:
// https://github.com/angular/components/blob/master/src/material/table/table-data-source.ts
export class TdMatTableDataSource<T> extends MatTableDataSource<T, MatPaginator> {
    language?: string;

    // enable localized sorting using localeCompare and languge property
    sortData = (data: T[], sort: MatSort) => {
        const active = sort.active;
        const direction = sort.direction;
        if (!active || direction === '') {
            return data;
        }

        return data.sort((a: T, b: T) => {
            const valueA = this.sortingDataAccessor(a, active);
            const valueB = this.sortingDataAccessor(b, active);

            // If both valueA and valueB exist (truthy), then compare the two. Otherwise, check if
            // one value exists while the other doesn't. In this case, existing value should come last.
            // This avoids inconsistent results when comparing values to undefined/null.
            // If neither value exists, return 0 (equal).
            let comparatorResult = 0;
            if (valueA != null && valueB != null) {
                comparatorResult = this.compare(valueA, valueB);
            } else if (valueA != null) {
                comparatorResult = 1;
            } else if (valueB != null) {
                comparatorResult = -1;
            }

            return comparatorResult * (direction === 'asc' ? 1 : -1);
        });
    };

    // Enable '.' notation in matColumnDef to access entry properties for sorting
    sortingDataAccessor = (data: T, sortHeaderId: string): string | number => {
        // using lodash to get value at path of object
        const value = get(data, sortHeaderId);

        if (_isNumberValue(value)) {
            const numberValue = Number(value);

            // Numbers beyond `MAX_SAFE_INTEGER` can't be compared reliably so we
            // leave them as strings. For more info: https://goo.gl/y5vbSg
            return numberValue < Number.MAX_SAFE_INTEGER ? numberValue : value;
        }

        return value;
    };

    private compare(valueA: string | number, valueB: string | number): number {
        // If there are data in the column that can be converted to a number,
        // it must be ensured that the rest of the data
        // is of the same type so as not to order incorrectly.
        const valueAType = typeof valueA;
        const valueBType = typeof valueB;

        if (valueAType === 'number' && valueBType === 'number') {
            if (valueA > valueB) {
                return 1;
            } else if (valueA < valueB) {
                return -1;
            }
        }

        if (Object.prototype.toString.call(valueA) === '[object Date]' && Object.prototype.toString.call(valueB) === '[object Date]') {
            if (valueA > valueB) {
                return 1;
            } else if (valueA < valueB) {
                return -1;
            }
        }

        // This is the actual part that had to be changed
        return valueA.toString().localeCompare(valueB.toString(), this.language);
    }
}
