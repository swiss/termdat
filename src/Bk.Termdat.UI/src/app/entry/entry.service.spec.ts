import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {TranslateService} from '@ngx-translate/core';
import {EntryChipFieldInputUpdate} from 'api/EntryChipFieldInputUpdate';
import {EntryChipFieldUpdate} from 'api/EntryChipFieldUpdate';
import {EntryCommentUpdate} from 'api/EntryCommentUpdate';
import {EntryContentMove} from 'api/EntryContentMove';
import {EntryDetail} from 'api/EntryDetail';
import {EntryHeaderModification} from 'api/EntryHeaderModification';
import {EntryModification} from 'api/EntryModification';
import {EntryModificationDetail} from 'api/EntryModificationDetail';
import {EntryMultipleUpdateResult} from 'api/EntryMultipleUpdateResult';
import {EntryReferenceDetail} from 'api/EntryReferenceDetail';
import {EntrySearchAndReplace} from 'api/EntrySearchAndReplace';
import {EntrySourceUpdate} from 'api/EntrySourceUpdate';
import {ReferenceType} from 'api/ReferenceType';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {EntryConceptSystemPositionUpdate} from '../../api/EntryConceptSystemPositionUpdate';
import {MultiEditAction} from '../../api/MultiEditAction';
import {Option} from '../shared/Option';
import {EntryService} from './entry.service';

describe('EntryService', () => {
    let service: EntryService;
    let httpMock: HttpTestingController;

    const translateServiceMock = {
        instant: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [{provide: TranslateService, useValue: translateServiceMock}, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
        });
        translateServiceMock.instant.mockImplementation(value => value);
        service = TestBed.inject(EntryService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should delete entry', () => {
        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${id}?soft=true`);
        expect(req.request.method).toEqual('DELETE');
        req.flush({});
        httpMock.verify();
    });

    it('should hard delete entry', () => {
        const id = 1;
        service.delete(id, false).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${id}?soft=false`);
        expect(req.request.method).toEqual('DELETE');
        req.flush({});
        httpMock.verify();
    });

    it('should delete multiple entries', () => {
        const ids: number[] = [1, 2];

        service.deleteMultiple(ids).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/multi-delete?soft=true`);
        expect(req.request.method).toEqual('DELETE');
        expect(req.request.body).toEqual(ids);
        req.flush({});
        httpMock.verify();
    });

    it('should hard delete multiple entries', () => {
        const ids: number[] = [1, 2];

        service.deleteMultiple(ids, false).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/multi-delete?soft=false`);
        expect(req.request.method).toEqual('DELETE');
        expect(req.request.body).toEqual(ids);
        req.flush({});
        httpMock.verify();
    });

    it('should restore entry', done => {
        const id = 1;
        service.restore(id).subscribe(() => {
            done();
        });

        const req = httpMock.expectOne('/api/entries/restore');
        expect(req.request.method).toEqual('POST');
        expect(req.request.body).toEqual(id);

        req.flush({});
    });

    it('should restore multiple entries', () => {
        const ids: number[] = [1, 2];

        service.restoreMultiple(ids).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/multi-restore`);
        expect(req.request.method).toEqual('POST');
        expect(req.request.body).toEqual(ids);
        req.flush({});
        httpMock.verify();
    });

    it('should get an entry', () => {
        let data;
        const entryId = 21;
        const sourceLanguageIds = [1, 2, 3];
        const targetLanguageIds = [4, 5, 6];

        const sourceIds = sourceLanguageIds.map(id => `sourceLanguageIds=${id}`);
        const targetIds = targetLanguageIds.map(id => `targetLanguageIds=${id}`);

        const query = `?${sourceIds.join('&')}&${targetIds.join('&')}`;

        service.get(entryId, sourceLanguageIds, targetLanguageIds).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entries/${entryId}${query}`);
        expect(req.request.method).toEqual('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it('should get an entry without translations', () => {
        let data;
        const entryId = 21;

        service.get(entryId).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entries/${entryId}`);
        expect(req.request.method).toEqual('GET');

        req.flush({});
        httpMock.verify();

        expect(data).toBeTruthy();
    });

    it('should create Date types after entry was retrieved', () => {
        let data: EntryDetail;
        const entryId = 21;

        service.get(entryId).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entries/${entryId}`);
        expect(req.request.method).toEqual('GET');

        const result = {created: '2023-05-25T12:20:00Z', modified: '2023-05-25T12:25:00Z', locked: '2023-05-25T12:30:00Z'};
        req.flush(result);
        httpMock.verify();

        expect(data!).toBeTruthy();
        expect(data!.created).toBeInstanceOf(Date);
        expect(data!.created).toEqual(new Date(result.created));
        expect(data!.modified).toBeInstanceOf(Date);
        expect(data!.modified).toEqual(new Date(result.modified));
        expect(data!.locked).toBeInstanceOf(Date);
        expect(data!.locked).toEqual(new Date(result.locked));
    });

    it('should get reliabilities', () => {
        let data: Option[];
        service.getReliabilities().subscribe(result => (data = result));

        expect(translateServiceMock.instant).toHaveBeenCalledTimes(5);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reliability.FirstDraft');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reliability.Tentative');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reliability.Translated');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reliability.Verified');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reliability.Approved');

        expect(data!).toHaveLength(5);

        expect(data![0].id).toEqual(Reliability.FirstDraft);
        expect(data![0].name).toEqual('field.reliability.FirstDraft');
        expect(data![1].id).toEqual(Reliability.Tentative);
        expect(data![1].name).toEqual('field.reliability.Tentative');
        expect(data![2].id).toEqual(Reliability.Translated);
        expect(data![2].name).toEqual('field.reliability.Translated');
        expect(data![3].id).toEqual(Reliability.Verified);
        expect(data![3].name).toEqual('field.reliability.Verified');
        expect(data![4].id).toEqual(Reliability.Approved);
        expect(data![4].name).toEqual('field.reliability.Approved');

        httpMock.verify();
    });

    it('should get reference types', () => {
        let data: Option[];
        service.getReferenceTypes().subscribe(result => (data = result));

        expect(translateServiceMock.instant).toHaveBeenCalledTimes(6);
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.None');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.General');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.Specific');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.Antonym');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.Whole');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('field.reference.Part');

        expect(data!).toHaveLength(6);

        expect(data![0].id).toEqual(ReferenceType.None);
        expect(data![0].name).toEqual('field.reference.None');
        expect(data![1].id).toEqual(ReferenceType.General);
        expect(data![1].name).toEqual('field.reference.General');
        expect(data![2].id).toEqual(ReferenceType.Specific);
        expect(data![2].name).toEqual('field.reference.Specific');
        expect(data![3].id).toEqual(ReferenceType.Antonym);
        expect(data![3].name).toEqual('field.reference.Antonym');
        expect(data![4].id).toEqual(ReferenceType.Whole);
        expect(data![4].name).toEqual('field.reference.Whole');
        expect(data![5].id).toEqual(ReferenceType.Part);
        expect(data![5].name).toEqual('field.reference.Part');

        httpMock.verify();
    });

    it('should load concept system positions', () => {
        let data;
        const id = 1;
        service.getConceptSystemPositions(id).subscribe(result => (data = result));
        const query = `?collectionId=${id}`;

        const req = httpMock.expectOne(`/api/concept-system-positions${query}`);
        expect(req.request.method).toEqual('GET');

        req.flush([{}]);

        expect(data).toHaveLength(1);

        httpMock.verify();
    });

    it('can create', () => {
        const entry = {status: Status.InProgress} as EntryModification;

        let entryDetail: EntryModificationDetail;
        service.create(entry).subscribe(data => (entryDetail = data));

        const req = httpMock.expectOne(`/api/entries`);
        expect(req.request.method).toEqual('POST');
        expect(req.request.body).toEqual(entry);

        const result = {id: 1, status: Status.InProgress, reliability: Reliability.Approved} as EntryModificationDetail;
        req.flush(result);
        expect(entryDetail!).toEqual(result);

        httpMock.verify();
    });

    it('should update an entry', () => {
        const entry = {} as EntryModification;
        entry.id = 1;
        entry.status = Status.InProgress;

        let entryDetail: EntryModificationDetail;
        service.update(entry).subscribe(data => (entryDetail = data));

        const req = httpMock.expectOne(`/api/entries/${entry.id}`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(FormData);
        const body = Object.fromEntries(req.request.body.entries());
        expect(body).toEqual({
            id: '1',
            status: '0',
        });

        const result = {id: 1, status: Status.InProgress, reliability: Reliability.Approved} as EntryModificationDetail;
        req.flush(result);
        expect(entryDetail!).toEqual(result);

        httpMock.verify();
    });

    it('should emit entryDeleted$ when delete is successful', () => {
        let deletedId: number | null = null;
        service.entryDeleted$.subscribe(id => (deletedId = id));

        expect(deletedId).toBeNull();

        const id = 1;
        service.delete(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${id}?soft=true`);
        expect(req.request.method).toEqual('DELETE');
        req.flush({});
        httpMock.verify();

        expect(deletedId).toEqual(id);
    });

    it('should emit entryUpdated$ when update is successful', () => {
        let updatedId: number | null = null;

        service.entryUpdated$.subscribe(id => {
            updatedId = id;
        });

        expect(updatedId).toBeNull();

        const entry = {} as EntryModification;
        entry.id = 1;

        service.update(entry).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${entry.id}`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(FormData);
        const body = Object.fromEntries(req.request.body.entries());
        expect(body).toEqual({
            id: '1',
        });
        req.flush(entry);
        httpMock.verify();

        expect(updatedId).toEqual(entry.id);
    });

    it('should emit entrySelected$ when entry is selected', () => {
        let selected = -1;

        service.entrySelected$.subscribe(id => {
            selected = id;
        });

        service.selectEntry(10);

        expect(selected).toEqual(10);
    });

    it('should update mandatory header fields', () => {
        const update: EntryHeaderModification = {
            ids: [1, 2],
            status: Status.Validated,
            reliability: Reliability.Approved,
            officeId: 3,
            collectionId: 4,
        };

        service.updateMandatoryHeaderFields(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-mandatory-header-fields`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(FormData);
        const body = Object.fromEntries(req.request.body.entries());
        expect(body).toEqual({
            'ids[0]': '1',
            'ids[1]': '2',
            status: '1',
            reliability: '4',
            officeId: '3',
            collectionId: '4',
        });
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple glossaries', () => {
        const update: EntryChipFieldUpdate = {ids: [1, 2], codes: ['3', '4'], action: 0};

        service.updateMultipleGlossaries(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-glossaries`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual(update);
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple admin infos', () => {
        const update: EntryChipFieldInputUpdate = {ids: [1, 2], codes: [{id: 3, code: 'abc', description: 'test'}], action: 1};

        service.updateMultipleAdminInfos(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-admin-infos`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            codes: [{id: 3, code: 'abc', description: 'test'}],
            action: 1,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple related collections', () => {
        const update: EntryChipFieldUpdate = {ids: [1, 2], codes: ['3', '4'], action: 2};

        service.updateMultipleRelatedCollections(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-related-collections`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            codes: ['3', '4'],
            action: 2,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple comment', () => {
        const update: EntryCommentUpdate = {ids: [1, 2], comment: 'updated comment', action: 2};

        service.updateMultipleComment(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-comment`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            comment: 'updated comment',
            action: 2,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple concept system position', () => {
        const update: EntryConceptSystemPositionUpdate = {ids: [1, 2], conceptSystemPositionId: 3, action: 2};

        service.updateMultipleConceptSystemPosition(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-concept-system-position`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            conceptSystemPositionId: 3,
            action: 2,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should update multiple source', () => {
        const update: EntrySourceUpdate = {
            ids: [1, 2],
            languageIds: [1, 2],
            action: MultiEditAction.Set,
            targetFields: ['terminusSource', 'noteSource'],
            updateValue: 'newSource',
        };

        service.updateMultipleSource(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/update-multiple-source`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            languageIds: [1, 2],
            targetFields: ['terminusSource', 'noteSource'],
            updateValue: 'newSource',
            action: 1,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should move multiple content', () => {
        const update: EntryContentMove = {ids: [1, 2], languageIds: [1, 2], sourceField: 'terminus', destinationField: 'note', forAllSynonyms: true};

        service.moveContent(update).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/move-content`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(Object);
        expect(req.request.body).toEqual({
            ids: [1, 2],
            languageIds: [1, 2],
            sourceField: 'terminus',
            destinationField: 'note',
            forAllSynonyms: true,
        });
        req.flush({});
        httpMock.verify();
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(service['entryUpdatedSubject'], 'complete');
        jest.spyOn(service['entryDeletedSubject'], 'complete');
        jest.spyOn(service['entrySelectedSubject'], 'complete');
        service.ngOnDestroy();
        expect(service['entryUpdatedSubject'].complete).toHaveBeenCalledTimes(1);
        expect(service['entryDeletedSubject'].complete).toHaveBeenCalledTimes(1);
        expect(service['entrySelectedSubject'].complete).toHaveBeenCalledTimes(1);
    });

    it('should get an entry for modification', () => {
        const entryId = 21;
        let data: EntryModificationDetail;

        service.getModification(entryId).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entries/${entryId}/modification`);
        expect(req.request.method).toEqual('GET');

        req.flush({id: entryId});
        httpMock.verify();

        expect(data!).toBeTruthy();
        expect(data!.id).toEqual(entryId);
    });

    it('should get a reference entry', () => {
        const entryId = 21;
        let data: EntryReferenceDetail;

        service.getReferenceEntry(entryId).subscribe(result => (data = result));

        const req = httpMock.expectOne(`/api/entries/${entryId}/reference`);
        expect(req.request.method).toEqual('GET');

        req.flush({id: entryId});
        httpMock.verify();

        expect(data!).toBeTruthy();
        expect(data!.id).toEqual(entryId);
    });

    it('should perform multiple search and replace', () => {
        const update: EntrySearchAndReplace = {
            ids: [1, 2],
            searchField: 'note',
            searchValue: 'value1',
            replaceValue: 'value2',
            languageIds: [3, 4],
        };
        let data: EntryMultipleUpdateResult;

        service.multipleSearchAndReplace(update).subscribe(count => {
            data = count;
        });

        const req = httpMock.expectOne(`/api/entries/multiple-search-and-replace`);
        expect(req.request.method).toEqual('PUT');
        expect(req.request.body).toBeInstanceOf(FormData);
        const body = Object.fromEntries(req.request.body.entries());
        expect(body).toEqual({
            'ids[0]': '1',
            'ids[1]': '2',
            'languageIds[0]': '3',
            'languageIds[1]': '4',
            searchValue: 'value1',
            replaceValue: 'value2',
            searchField: 'note',
        });
        req.flush({});
        httpMock.verify();
        expect(data!).toBeTruthy();
    });

    it('should lock entry', () => {
        const id = 1;
        service.lock(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${id}/lock`);
        expect(req.request.method).toEqual('PUT');
        req.flush({});
        httpMock.verify();
    });

    it('should unlock entry', () => {
        const id = 1;
        service.unlock(id).subscribe(() => {});

        const req = httpMock.expectOne(`/api/entries/${id}/unlock`);
        expect(req.request.method).toEqual('PUT');
        req.flush({});
        httpMock.verify();
    });
});
