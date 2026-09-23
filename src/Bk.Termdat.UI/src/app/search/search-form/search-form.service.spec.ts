import {TestBed} from '@angular/core/testing';
import {AbstractControl, ReactiveFormsModule, UntypedFormGroup} from '@angular/forms';
import {CommentStatus} from 'api/CommentStatus';
import {SearchQuery} from '../../../api/SearchQuery';
import {Status} from '../../../api/Status';
import {SearchFormService} from './search-form.service';

describe('SearchFormService', () => {
    let service: SearchFormService;

    const defaultOfficeId = 1;
    const defaultStateId = Status.Validated;

    const defaultSearchQuery: SearchQuery = {
        offices: [defaultOfficeId],
        officesPriority: true,
        lenochClassifications: [],
        lenochClassificationsPriority: false,
        collections: [],
        collectionsPriority: false,
        collectionsExclude: false,
        relatedCollections: [],
        glossaries: [],
        glossariesExclude: false,
        status: [defaultStateId],
        statusPriority: true,
        adminInfos: [],
        adminInfosPriority: false,
        adminInfosExclude: false,
        commentStatus: [],
        createdBy: undefined,
        createdByUser: false,
        createdByPriority: false,
        createdByExclude: false,
        modifiedBy: undefined,
        modifiedByUser: false,
        modifiedByPriority: false,
        modifiedByExclude: false,
        modifiedFrom: undefined,
        modifiedTo: undefined,
        modifiedPeriodPriority: false,
        modifiedPeriodExclude: false,
        subjectDomains: [],
        subjectDomainsPriority: false,
        reliability: [],
        reliabilityPriority: false,
        deleted: false,
        deletedPriority: false,
        hasMissingSource: false,
        fields: {
            term: true,
            name: true,
            abbreviation: true,
            phraseology: true,
            definition: false,
            note: false,
            context: false,
            source: false,
            metadata: true,
            country: false,
            comment: false,
        },
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [ReactiveFormsModule],
        });
        service = TestBed.inject(SearchFormService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('returns the correct values for admin on getSearchValues', () => {
        const searchQuery = {} as SearchQuery;
        const expected = {
            createdByUser: false,
            modifiedByUser: false,
        };

        const received = service.getSearchValues(searchQuery, true, false, false, false);

        expect(received).toStrictEqual(expected);
    });

    it('returns the correct values for superuser on getSearchValues', () => {
        const searchQuery = {} as SearchQuery;
        const expected = {
            createdByUser: false,
            deleted: false,
            deletedPriority: false,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            modifiedByUser: false,
            modifiedFrom: undefined,
            modifiedPeriodPriority: false,
            modifiedTo: undefined,
            collectionsExclude: false,
            glossariesExclude: false,
            adminInfosExclude: false,
            createdByExclude: false,
            modifiedByExclude: false,
            modifiedPeriodExclude: false,
        };

        const received = service.getSearchValues(searchQuery, false, true, false, false);

        expect(received).toStrictEqual(expected);
    });

    it('returns the correct values for superreader on getSearchValues', () => {
        const searchQuery = {} as SearchQuery;
        const expected = {
            createdBy: undefined,
            createdByPriority: false,
            createdByUser: false,
            deleted: false,
            deletedPriority: false,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            modifiedByUser: false,
            modifiedPeriodPriority: false,
            modifiedBy: undefined,
            modifiedByPriority: false,
            relatedCollections: [],
            reliability: [],
            reliabilityPriority: false,
            collectionsExclude: false,
            glossariesExclude: false,
            adminInfosExclude: false,
            createdByExclude: false,
            modifiedByExclude: false,
            modifiedPeriodExclude: false,
        };

        const received = service.getSearchValues(searchQuery, false, false, false, true);

        expect(received).toStrictEqual(expected);
    });

    it('returns the correct values for writer on getSearchValues', () => {
        const searchQuery = {} as SearchQuery;
        const expected = {
            createdBy: undefined,
            createdByPriority: false,
            deleted: false,
            deletedPriority: false,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            modifiedBy: undefined,
            modifiedByPriority: false,
            modifiedFrom: undefined,
            modifiedPeriodPriority: false,
            modifiedTo: undefined,
            relatedCollections: [],
            reliability: [],
            reliabilityPriority: false,
            collectionsExclude: false,
            glossariesExclude: false,
            adminInfosExclude: false,
            createdByExclude: false,
            modifiedByExclude: false,
            modifiedPeriodExclude: false,
        };

        const received = service.getSearchValues(searchQuery, false, false, true, false);

        expect(received).toStrictEqual(expected);
    });

    it('return the default form values on getFormDefaults', () => {
        const received = service.getFormDefaults();

        expect(received).toEqual(defaultSearchQuery);
    });

    it('should return form on setupFormGroup', () => {
        const form = service.setupFormGroup(new UntypedFormGroup({}));

        expect(form.controls.offices).toBeInstanceOf(AbstractControl);
        expect(form.controls.officesPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.lenochClassifications).toBeInstanceOf(AbstractControl);
        expect(form.controls.lenochClassificationsPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.subjectDomains).toBeInstanceOf(AbstractControl);
        expect(form.controls.subjectDomainsPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.collections).toBeInstanceOf(AbstractControl);
        expect(form.controls.relatedCollections).toBeInstanceOf(AbstractControl);
        expect(form.controls.collectionsPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.collectionsExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.glossaries).toBeInstanceOf(AbstractControl);
        expect(form.controls.glossariesExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.status).toBeInstanceOf(AbstractControl);
        expect(form.controls.statusPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.adminInfos).toBeInstanceOf(AbstractControl);
        expect(form.controls.adminInfosPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.adminInfosExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.commentStatus).toBeInstanceOf(AbstractControl);
        expect(form.controls.createdBy).toBeInstanceOf(AbstractControl);
        expect(form.controls.createdByUser).toBeInstanceOf(AbstractControl);
        expect(form.controls.createdByPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.createdByExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedBy).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedByUser).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedByPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedByExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedFrom).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedTo).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedPeriodPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.modifiedPeriodExclude).toBeInstanceOf(AbstractControl);
        expect(form.controls.reliability).toBeInstanceOf(AbstractControl);
        expect(form.controls.reliabilityPriority).toBeInstanceOf(AbstractControl);
        expect(form.controls.fields).toBeInstanceOf(AbstractControl);
    });

    it('should return form on setupFormGroup with provided values', () => {
        const searchQuery = {
            entryIds: '',
            offices: [1, 2, 3],
            officesPriority: true,
            lenochClassifications: [1, 2, 3],
            lenochClassificationsPriority: true,
            collections: [1, 2, 3],
            collectionsPriority: true,
            collectionsExclude: false,
            relatedCollections: [1, 2, 3],
            glossaries: [1, 2, 3],
            glossariesExclude: false,
            status: [Status.InProgress],
            statusPriority: true,
            adminInfos: [1, 2, 3],
            adminInfosPriority: true,
            adminInfosExclude: false,
            commentStatus: [CommentStatus.Empty],
            createdBy: 'createdBy',
            createdByUser: true,
            createdByPriority: true,
            createdByExclude: false,
            modifiedBy: 'modifiedBy',
            modifiedByUser: true,
            modifiedByPriority: true,
            modifiedByExclude: false,
            modifiedFrom: new Date(2023, 7, 1, 0, 0, 0),
            modifiedTo: new Date(2023, 8, 1, 0, 0, 0),
            modifiedPeriodPriority: true,
            modifiedPeriodExclude: false,
            subjectDomains: ['1', '2', '3'],
            subjectDomainsPriority: true,
            reliability: [1, 2, 3],
            reliabilityPriority: true,
            deleted: true,
            deletedPriority: true,
            hasMissingSource: true,
            fields: {
                term: true,
                name: true,
                abbreviation: true,
                phraseology: true,
                definition: true,
                note: true,
                context: true,
                source: true,
                metadata: true,
                country: true,
                comment: true,
            },
        } as SearchQuery;
        const form = service.setupFormGroup(new UntypedFormGroup({}), searchQuery);

        expect(form.getRawValue()).toEqual(searchQuery);
    });
});
