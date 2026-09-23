import {MatSort} from '@angular/material/sort';
import {TdMatTableDataSource} from './td-mat-table-data-source';

interface Animal {
    name?: string;
    description?: string;
    body?: AnimalBody;
    date?: Date;
}

interface AnimalBody {
    height: number;
}

describe('TdMatTableDataSource', () => {
    let dataSource: TdMatTableDataSource<Animal>;
    const animals: Animal[] = [
        {name: 'Cat', description: 'ööö', body: {height: 200}, date: new Date('01/12/2020 08:30 AM')},
        {name: 'Bear', description: 'ooo'},
        {name: 'Ape', description: 'aaa', body: {height: 300}, date: new Date('09/11/2019 08:30 AM')},
        {name: undefined, description: 'zzz', body: {height: 400}, date: new Date('08/04/2018 08:30 AM')},
    ];
    const animal: Animal = {
        name: 'Zebrra',
        description: 'something something',
        body: {
            height: 500,
        },
    };

    beforeEach(() => {
        dataSource = new TdMatTableDataSource<Animal>([]);
        dataSource.language = 'de';
        dataSource.sort = new MatSort();
    });

    it('should be created', () => {
        expect(dataSource).toBeTruthy();
    });

    it('should sort alphabetically and undefined', () => {
        const sort = {active: 'name', direction: 'desc'} as MatSort;

        const sortedData = dataSource.sortData(animals, sort);

        expect(sortedData).toEqual([
            {body: {height: 200}, description: 'ööö', name: 'Cat', date: new Date('2020-01-12T07:30:00.000Z')},
            {description: 'ooo', name: 'Bear'},
            {body: {height: 300}, description: 'aaa', name: 'Ape', date: new Date('2019-09-11T06:30:00.000Z')},
            {body: {height: 400}, description: 'zzz', name: undefined, date: new Date('2018-08-04T06:30:00.000Z')},
        ]);
    });

    it('should sort locally alphabetical', () => {
        const sort = {active: 'description', direction: 'asc'} as MatSort;

        const sortedData = dataSource.sortData(animals, sort);

        expect(sortedData).toEqual([
            {body: {height: 300}, description: 'aaa', name: 'Ape', date: new Date('2019-09-11T06:30:00.000Z')},
            {description: 'ooo', name: 'Bear'},
            {body: {height: 200}, description: 'ööö', name: 'Cat', date: new Date('2020-01-12T07:30:00.000Z')},
            {body: {height: 400}, description: 'zzz', name: undefined, date: new Date('2018-08-04T06:30:00.000Z')},
        ]);
    });

    it('should sort numbers', () => {
        const sort = {active: 'body.height', direction: 'asc'} as MatSort;

        const sortedData = dataSource.sortData(animals, sort);

        expect(sortedData).toEqual([
            {description: 'ooo', name: 'Bear'},
            {body: {height: 200}, description: 'ööö', name: 'Cat', date: new Date('2020-01-12T07:30:00.000Z')},
            {body: {height: 300}, description: 'aaa', name: 'Ape', date: new Date('2019-09-11T06:30:00.000Z')},
            {body: {height: 400}, description: 'zzz', name: undefined, date: new Date('2018-08-04T06:30:00.000Z')},
        ]);
    });

    it('should sort dates', () => {
        const sort = {active: 'date', direction: 'asc'} as MatSort;

        const sortData = dataSource.sortData(animals, sort);

        expect(sortData).toEqual([
            {description: 'ooo', name: 'Bear'},
            {body: {height: 400}, description: 'zzz', name: undefined, date: new Date('2018-08-04T06:30:00.000Z')},
            {body: {height: 300}, description: 'aaa', name: 'Ape', date: new Date('2019-09-11T06:30:00.000Z')},
            {body: {height: 200}, description: 'ööö', name: 'Cat', date: new Date('2020-01-12T07:30:00.000Z')},
        ]);
    });

    it('should get Data with . notation', () => {
        const data = dataSource.sortingDataAccessor(animal, 'body.height');
        expect(data).toBe(500);
    });
});
