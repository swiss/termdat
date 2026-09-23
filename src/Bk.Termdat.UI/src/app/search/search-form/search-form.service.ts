import {Injectable} from '@angular/core';
import {FormBuilder, FormGroup} from '@angular/forms';
import {SearchQuery} from 'api/SearchQuery';
import {Status} from '../../../api/Status';

@Injectable({
    providedIn: 'root',
})
export class SearchFormService {
    public readonly defaultOfficeId = 1;
    public readonly defaultStateId = Status.Validated;

    constructor(private readonly formBuilder: FormBuilder) {}

    public getFormDefaults(): SearchQuery {
        return {
            entryIds: undefined,
            offices: [this.defaultOfficeId],
            officesPriority: true,
            lenochClassifications: [],
            lenochClassificationsPriority: false,
            collections: [],
            collectionsPriority: false,
            collectionsExclude: false,
            relatedCollections: [],
            glossaries: [],
            glossariesExclude: false,
            status: [this.defaultStateId],
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
    }

    public getSearchValues(formValue: SearchQuery, isAdmin: boolean, isSuperUser: boolean, isWriter: boolean, isSuperReader: boolean): SearchQuery | undefined {
        const defaultValues = this.getFormDefaults();

        if (!isAdmin) {
            formValue.lenochClassifications = defaultValues.lenochClassifications;
            formValue.lenochClassificationsPriority = defaultValues.lenochClassificationsPriority;
            formValue.modifiedPeriodPriority = defaultValues.modifiedPeriodPriority;
            formValue.deleted = defaultValues.deleted;
            formValue.deletedPriority = defaultValues.deletedPriority;
            formValue.collectionsExclude = defaultValues.collectionsExclude;
            formValue.glossariesExclude = defaultValues.glossariesExclude;
            formValue.adminInfosExclude = defaultValues.adminInfosExclude;
            formValue.createdByExclude = defaultValues.createdByExclude;
            formValue.modifiedByExclude = defaultValues.modifiedByExclude;
            formValue.modifiedPeriodExclude = defaultValues.modifiedPeriodExclude;
        }

        if (!isAdmin && !isSuperReader) {
            formValue.modifiedFrom = defaultValues.modifiedFrom;
            formValue.modifiedTo = defaultValues.modifiedTo;
        }

        if (!(isAdmin || isSuperUser || isWriter || isSuperReader)) {
            formValue.adminInfos = defaultValues.adminInfos;
            formValue.adminInfosPriority = defaultValues.adminInfosPriority;
            formValue.commentStatus = defaultValues.commentStatus;
            if (formValue.fields && defaultValues.fields) {
                formValue.fields.comment = defaultValues.fields.comment;
            }
            formValue.hasMissingSource = defaultValues.hasMissingSource;
        }

        if (!(isAdmin || isSuperUser)) {
            formValue.createdBy = defaultValues.createdBy;
            formValue.createdByPriority = defaultValues.createdByPriority;
            formValue.modifiedBy = defaultValues.modifiedBy;
            formValue.modifiedByPriority = defaultValues.modifiedByPriority;
            formValue.relatedCollections = defaultValues.relatedCollections;
            formValue.reliability = defaultValues.reliability;
            formValue.reliabilityPriority = defaultValues.reliabilityPriority;
        }

        if (!isWriter) {
            formValue.createdByUser = defaultValues.createdByUser;
            formValue.modifiedByUser = defaultValues.modifiedByUser;
        }

        return formValue;
    }

    public setupFormGroup(form: FormGroup, savedValues?: SearchQuery): FormGroup {
        const defaults = savedValues ?? this.getFormDefaults();

        form.setControl('entryIds', this.formBuilder.control(defaults.entryIds));
        form.setControl('offices', this.formBuilder.control(defaults.offices));
        form.setControl('officesPriority', this.formBuilder.control(defaults.officesPriority));
        form.setControl('lenochClassifications', this.formBuilder.control(defaults.lenochClassifications));
        form.setControl('lenochClassificationsPriority', this.formBuilder.control(defaults.lenochClassificationsPriority));
        form.setControl('subjectDomains', this.formBuilder.control(defaults.subjectDomains));
        form.setControl('subjectDomainsPriority', this.formBuilder.control(defaults.subjectDomainsPriority));
        form.setControl('collections', this.formBuilder.control(defaults.collections));
        form.setControl('relatedCollections', this.formBuilder.control(defaults.relatedCollections));
        form.setControl('collectionsPriority', this.formBuilder.control(defaults.collectionsPriority));
        form.setControl('collectionsExclude', this.formBuilder.control(defaults.collectionsExclude));
        form.setControl('glossaries', this.formBuilder.control(defaults.glossaries));
        form.setControl('glossariesExclude', this.formBuilder.control(defaults.glossariesExclude));
        form.setControl('status', this.formBuilder.control(defaults.status));
        form.setControl('statusPriority', this.formBuilder.control(defaults.statusPriority));
        form.setControl('adminInfos', this.formBuilder.control(defaults.adminInfos));
        form.setControl('adminInfosPriority', this.formBuilder.control(defaults.adminInfosPriority));
        form.setControl('adminInfosExclude', this.formBuilder.control(defaults.adminInfosExclude));
        form.setControl('commentStatus', this.formBuilder.control(defaults.commentStatus));
        form.setControl('createdBy', this.formBuilder.control(defaults.createdBy));
        form.setControl('createdByUser', this.formBuilder.control(defaults.createdByUser));
        form.setControl('createdByPriority', this.formBuilder.control(defaults.createdByPriority));
        form.setControl('createdByExclude', this.formBuilder.control(defaults.createdByExclude));
        form.setControl('modifiedBy', this.formBuilder.control(defaults.modifiedBy));
        form.setControl('modifiedByUser', this.formBuilder.control(defaults.modifiedByUser));
        form.setControl('modifiedByPriority', this.formBuilder.control(defaults.modifiedByPriority));
        form.setControl('modifiedByExclude', this.formBuilder.control(defaults.modifiedByExclude));
        form.setControl('modifiedFrom', this.formBuilder.control(defaults.modifiedFrom));
        form.setControl('modifiedTo', this.formBuilder.control(defaults.modifiedTo));
        form.setControl('modifiedPeriodPriority', this.formBuilder.control(defaults.modifiedPeriodPriority));
        form.setControl('modifiedPeriodExclude', this.formBuilder.control(defaults.modifiedPeriodExclude));
        form.setControl('reliability', this.formBuilder.control(defaults.reliability));
        form.setControl('reliabilityPriority', this.formBuilder.control(defaults.reliabilityPriority));
        form.setControl('deleted', this.formBuilder.control(defaults.deleted));
        form.setControl('deletedPriority', this.formBuilder.control(defaults.deletedPriority));
        form.setControl('hasMissingSource', this.formBuilder.control(defaults.hasMissingSource));
        form.setControl('fields', this.formBuilder.group(defaults.fields!));

        return form;
    }
}
