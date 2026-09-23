import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormGroup} from '@angular/forms';
import {MatSelectModule} from '@angular/material/select';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ObNotificationService, WINDOW} from '@oblique/oblique';
import {DuplicateCheckRequest} from 'api/DuplicateCheckRequest';
import {DuplicateCheckResponse} from 'api/DuplicateCheckResponse';
import {MockComponent, MockModule} from 'ng-mocks';
import {Observable, of, Subject, throwError} from 'rxjs';
import {ExtendedSearchFiltersService} from '../search/extended-search-filters.service';
import {AutocompleteComponent} from '../shared/autocomplete/autocomplete.component';
import {ErrorService} from '../shared/error.service';
import {LanguageService} from '../shared/language.service';
import {DuplicateCheckComponent, exactlyOneValidator} from './duplicate-check.component';
import {DuplicateCheckService} from './duplicate-check.service';

describe('DuplicateCheckComponent', () => {
    let component: DuplicateCheckComponent;
    let fixture: ComponentFixture<DuplicateCheckComponent>;

    const langChangeSubject = new Subject();
    const translateServiceMock = {
        instant: jest.fn(),
        onLangChange: langChangeSubject.asObservable(),
        getCurrentLang: jest.fn(() => 'de'),
    };
    const errorServiceMock = {
        handleHttpError: jest.fn(),
        getControlError: jest.fn(),
    };
    const windowMock = {
        location: {
            origin: 'https://test.ch',
        },
        setTimeout: jest.fn(),
    };
    const extendedSearchFilterServiceMock = {
        getCollections: jest.fn(() => of([{id: '1', label: 'Collection'}])),
        getGlossaries: jest.fn(() => of([{id: '2', label: 'Glossary'}])),
    };

    const languageServiceMock = {
        languageSummaries$: of([{id: 'de', name: 'Deutsch'}]),
    };

    const duplicateCheckServiceMock = {
        performDuplicateCheck: jest.fn<Observable<DuplicateCheckResponse>, [DuplicateCheckRequest]>(),
        exportDuplicateCheckXlsx: jest.fn().mockReturnValue(of(new Blob())),
        exportDuplicateCheckTxt: jest.fn(() => of({ids: []})),
    };

    const notificationServiceMock = {
        error: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [DuplicateCheckComponent, MockModule(MatSelectModule), MockModule(TranslateModule), MockComponent(AutocompleteComponent)],
            providers: [
                {provide: DuplicateCheckService, useValue: duplicateCheckServiceMock},
                {provide: ExtendedSearchFiltersService, useValue: extendedSearchFilterServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: ErrorService, useValue: errorServiceMock},
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {
                    provide: WINDOW,
                    useValue: windowMock,
                },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(DuplicateCheckComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        translateServiceMock.instant.mockImplementation(input => input);
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('exactlyOneValidator', () => {
        let form: FormGroup;

        beforeEach(() => {
            const fb = new FormBuilder();

            form = fb.group(
                {
                    collection: fb.control<string | null>(null),
                    glossary: fb.control<string | null>(null),
                },
                {
                    validators: exactlyOneValidator,
                }
            );
        });

        it('should be invalid when neither collection nor glossary is selected', () => {
            expect(form.valid).toBe(false);
            expect(form.errors).toEqual({exactlyOneRequired: true});
        });

        it('should be valid when only collection is selected', () => {
            form.patchValue({collection: '1'});

            expect(form.valid).toBe(true);
        });

        it('should be valid when only glossary is selected', () => {
            form.patchValue({glossary: '1'});

            expect(form.valid).toBe(true);
        });

        it('should be invalid when both are selected', () => {
            form.patchValue({
                collection: '1',
                glossary: '2',
            });

            expect(form.valid).toBe(false);
            expect(form.errors).toEqual({exactlyOneRequired: true});
        });
    });

    it('should disable collection when glossary is selected', () => {
        component.form.patchValue({
            glossary: '123',
        });

        expect(component.form.get('collection')?.disabled).toBe(true);
    });

    it('should disable collection when glossary is selected', () => {
        component.form.patchValue({
            glossary: '123',
        });

        expect(component.form.get('collection')?.disabled).toBe(true);
    });

    it('should enable glossary again when collection is cleared', () => {
        component.form.patchValue({collection: '123'});
        component.form.patchValue({collection: null});

        expect(component.form.get('glossary')?.enabled).toBe(true);
    });

    it('should enable collection again when glossary is cleared', () => {
        component.form.patchValue({glossary: '123'});
        component.form.patchValue({glossary: null});

        expect(component.form.get('collection')?.enabled).toBe(true);
    });

    it('should perform duplicate check and export ids as text', () => {
        component.form.patchValue({
            language: 1,
            collection: '2',
        });

        duplicateCheckServiceMock.performDuplicateCheck.mockReturnValue(of({ids: [10, 20, 30]}));

        component.exportAsText();

        expect(duplicateCheckServiceMock.performDuplicateCheck).toHaveBeenCalledWith({
            languageId: 1,
            collectionId: '2',
            glossaryId: undefined,
        });

        expect(duplicateCheckServiceMock.exportDuplicateCheckTxt).toHaveBeenCalledWith([10, 20, 30]);
    });

    it('should perform duplicate check for glossary and export ids as text', () => {
        component.form.patchValue({
            language: 1,
            glossary: '3',
        });

        duplicateCheckServiceMock.performDuplicateCheck.mockReturnValue(of({ids: [10, 20, 30]}));

        component.exportAsText();

        expect(duplicateCheckServiceMock.performDuplicateCheck).toHaveBeenCalledWith({
            languageId: 1,
            glossaryId: '3',
            collectionId: undefined,
        });
    });

    it('should show an error notification when duplicate check fails', () => {
        component.form.patchValue({
            language: 1,
            collection: '2',
        });

        duplicateCheckServiceMock.performDuplicateCheck.mockReturnValue(throwError(() => new Error()));

        component.exportAsText();

        expect(notificationServiceMock.error).toHaveBeenCalledWith('duplicateCheck.exportError');

        expect(duplicateCheckServiceMock.exportDuplicateCheckTxt).not.toHaveBeenCalled();
    });

    it('should perform duplicate check and export ids as xlsx', () => {
        component.form.patchValue({
            language: 1,
            collection: '2',
        });

        component.exportAsXlsx();

        expect(duplicateCheckServiceMock.exportDuplicateCheckXlsx).toHaveBeenCalledWith({
            languageId: 1,
            collectionId: '2',
            glossaryId: undefined,
        });
    });

    it('should load collections, glossaries and languages', () => {
        fixture.detectChanges();

        expect(component.collections()).toEqual([{id: '1', label: 'Collection'}]);

        expect(component.glossaries()).toEqual([{id: '2', label: 'Glossary'}]);

        expect(component.languageOptions()).toEqual([{id: 'de', name: 'Deutsch'}]);
    });

    it('should reset the form', () => {
        component.form.patchValue({
            language: 'de',
            collection: '1',
        });

        component.reset();

        expect(component.form.value).toEqual({
            language: null,
            collection: null,
            glossary: null,
        });
    });
});
