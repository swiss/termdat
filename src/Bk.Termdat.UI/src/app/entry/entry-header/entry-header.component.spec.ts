import {Clipboard} from '@angular/cdk/clipboard';
import {EventEmitter} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatDialog} from '@angular/material/dialog';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {By} from '@angular/platform-browser';
import {TranslateModule, TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObIconModule, ObNotificationService} from '@oblique/oblique';
import {CollectionSummary} from 'api/CollectionSummary';
import {EntryDetail} from 'api/EntryDetail';
import {OfficeSummary} from 'api/OfficeSummary';
import {PictureDetail} from 'api/PictureDetail';
import {Reliability} from 'api/Reliability';
import {Status} from 'api/Status';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {BehaviorSubject} from 'rxjs';
import {AuthService} from '../../auth/auth.service';
import {ConfigsService} from '../../config.service';
import {WINDOW} from '../../shared/injection.tokens';
import {EntryHeaderComponent} from './entry-header.component';
import {PictureMetadataComponent} from './picture-metadata/picture-metadata.component';
import {ReferencedByListComponent} from './referenced-by-list/referenced-by-list.component';

describe('EntryDetailMetadataComponent', () => {
    let component: EntryHeaderComponent;
    let fixture: ComponentFixture<EntryHeaderComponent>;
    let entry: EntryDetail;
    let entryNotAch: EntryDetail;

    const dialogMock = {
        open: jest.fn(),
    };

    const onLangChangeEventEmitter = new EventEmitter<void>();
    let translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        instant: jest.fn().mockImplementation(key => key),
        onLangChange: onLangChangeEventEmitter,
    } as unknown as TranslateService;
    const notificationServiceMock = {
        success: jest.fn(),
    };
    const clipboardMock = {
        copy: jest.fn(),
    };
    const windowMock = {
        location: {
            origin: 'https://test.ch',
        },
    };
    const isAdminSubject = new BehaviorSubject(true);
    const isSuperUserSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);
    const authServiceMock = {
        isAdmin$: isAdminSubject.asObservable(),
        isSuperUser$: isSuperUserSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
    };

    const configServiceMock = {
        frontendConfig: {
            feedbackEmail: 'BK_DevOPS@bit.admin.ch',
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatIconModule),
                MockModule(ObIconModule),
                MockModule(TranslateModule),
                MockModule(MatTooltipModule),
                EntryHeaderComponent,
                MockPipe(TranslatePipe),
                MockComponent(PictureMetadataComponent),
                MockComponent(ReferencedByListComponent),
            ],
            providers: [
                {
                    provide: MatDialog,
                    useValue: dialogMock,
                },
                {
                    provide: TranslateService,
                    useValue: translateServiceMock,
                },
                {provide: ObNotificationService, useValue: notificationServiceMock},
                {provide: Clipboard, useValue: clipboardMock},
                {
                    provide: WINDOW,
                    useValue: windowMock,
                },
                {provide: AuthService, useValue: authServiceMock},
                {provide: ConfigsService, useValue: configServiceMock},
            ],
        }).compileComponents();
        translateServiceMock = TestBed.inject(TranslateService);

        entry = {
            collection: {
                isObsolete: true,
                name: 'Name1',
                code: 'Code1',
                active: false,
                exportLindas: true,
                activeApi: true,
            } as CollectionSummary,
            relatedCollections: [
                {
                    isObsolete: true,
                    name: 'Name2',
                    code: 'Code2',
                } as CollectionSummary,
            ],
            lenochClassifications: [],
            id: 1,
            status: Status.Validated,
            reliability: Reliability.Approved,
            identification: 10,
            office: {
                code: 'ACH',
                name: 'Schweizerische Bundesverwaltung',
            } as OfficeSummary,
            isMainOffice: true,
            picture: {
                descriptionDe: 'de description',
                descriptionFr: 'fr description',
            } as PictureDetail,
            languageDetails: [],
            modified: new Date(),
            created: new Date(),
            deleted: false,
            concurrencyToken: 999,
            subjects: [{id: 1, name: 'name1', domains: [{id: 2, name: 'name2', subjectId: 1}]}],
        };

        entryNotAch = {
            collection: {
                isObsolete: true,
                name: 'Name3',
                code: 'Code3',
            } as CollectionSummary,
            relatedCollections: [
                {
                    isObsolete: true,
                    name: 'Name4',
                    code: 'Code4',
                } as CollectionSummary,
            ],
            lenochClassifications: [],
            id: 2,
            status: Status.InProgress,
            reliability: Reliability.FirstDraft,
            identification: 11,
            office: {
                code: 'CHB',
                name: 'Nicht Schweizerische Bundesverwaltung',
            } as OfficeSummary,
            isMainOffice: false,
            picture: {
                descriptionDe: 'de updated description',
            } as PictureDetail,
            languageDetails: [],
            modified: new Date(),
            created: new Date(),
            deleted: false,
            subjects: [],
            concurrencyToken: 999,
        };

        fixture = TestBed.createComponent(EntryHeaderComponent);
        component = fixture.componentInstance;
        component.entryDetail = entry;
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should validate correctly', () => {
        component.entryDetail = {} as EntryDetail;
        expect(component.isValidated).toEqual(false);

        component.entryDetail = {status: Status.Validated} as EntryDetail;
        expect(component.isValidated).toEqual(true);

        component.entryDetail = {status: Status.InProgress} as EntryDetail;
        expect(component.isValidated).toEqual(false);
    });

    it('should have correct statusName', () => {
        component.entryDetail = {} as EntryDetail;
        expect(component.statusName).toEqual(Status[Status.InProgress]);

        component.entryDetail = {status: Status.Validated} as EntryDetail;
        expect(component.statusName).toEqual(Status[Status.Validated]);

        component.entryDetail = {status: Status.InProgress} as EntryDetail;
        expect(component.statusName).toEqual(Status[Status.InProgress]);
    });

    it('should have correct reliabilityName', () => {
        component.entryDetail = {} as EntryDetail;
        expect(component.reliabilityName).toEqual(Reliability[Reliability.FirstDraft]);

        component.entryDetail = {reliability: Reliability.Tentative} as EntryDetail;
        expect(component.reliabilityName).toEqual(Reliability[Reliability.Tentative]);

        component.entryDetail = {reliability: Reliability.FirstDraft} as EntryDetail;
        expect(component.reliabilityName).toEqual(Reliability[Reliability.FirstDraft]);

        component.entryDetail = {reliability: Reliability.Approved} as EntryDetail;
        expect(component.reliabilityName).toEqual(Reliability[Reliability.Approved]);
    });

    it('should set isAdmin correctly', () => {
        expect(component.isAdmin).toEqual(true);
    });

    it('should have correct translatedPictureAltText', () => {
        component.entryDetail = {picture: {}} as EntryDetail;
        expect(component.translatedPictureAltText).toEqual('entry.picture.descriptionNotSet');

        component.entryDetail = {picture: {descriptionDe: 'de'}} as EntryDetail;
        expect(component.translatedPictureAltText).toEqual('de');
    });

    it('can handle collection, office, created, modified and relatedCollections being null', async () => {
        component.entryDetail = {} as EntryDetail;

        fixture.detectChanges();
        await fixture.whenStable();

        expect(fixture.debugElement.query(By.css('legend'))).toBeTruthy();
    });

    it('should add obsolete class when collection is obsolete', async () => {
        const obsolete = fixture.debugElement.queryAll(By.css('.obsolete'));

        expect(obsolete.length).toEqual(2);
    });

    it('should add inactive message when collection is inactive', async () => {
        const inactiveCollection = fixture.debugElement.queryAll(By.css('.collectionInactive'));

        expect(inactiveCollection.length).toEqual(1);
    });

    it('should render LINDAS chip when collection is exported to LINDAS', async () => {
        const lindasChip = fixture.debugElement.queryAll(By.css('.chip.lindas'));

        expect(lindasChip.length).toEqual(1);
        expect(lindasChip[0].nativeElement.textContent).toContain('LINDAS');
    });

    it('should render API chip when collection is active for API and user is admin, super user or writer', async () => {
        const activeApiChip = fixture.debugElement.queryAll(By.css('.chip.activeApi'));

        expect(activeApiChip.length).toEqual(1);
    });

    it('should hide API chip when user is neither admin, super user nor writer', async () => {
        isAdminSubject.next(false);
        isSuperUserSubject.next(false);
        isWriterSubject.next(false);
        fixture.detectChanges();
        await fixture.whenStable();

        const activeApiChip = fixture.debugElement.queryAll(By.css('.chip.activeApi'));

        expect(activeApiChip.length).toEqual(0);
    });

    it('should render deleted chip when entry is deleted', async () => {
        let deletedChip = fixture.debugElement.query(By.css('.chip.deleted'));
        expect(deletedChip).toBeFalsy();

        component.entry!.deleted = true;

        fixture.detectChanges();
        await fixture.whenStable();

        deletedChip = fixture.debugElement.query(By.css('.chip.deleted'));
        expect(deletedChip).toBeTruthy();
    });

    it('should re-calculate properties when entry detail is updated', () => {
        expect(component.isMainOffice).toEqual(true);
        expect(component.isValidated).toEqual(true);
        expect(component.reliabilityName).toEqual(Reliability[Reliability.Approved]);
        expect(component.statusName).toEqual(Status[Status.Validated]);
        expect(component.translatedPictureAltText).toBe('de description');

        component.entryDetail = entryNotAch;

        expect(component.isMainOffice).toEqual(false);
        expect(component.isValidated).toEqual(false);
        expect(component.reliabilityName).toEqual(Reliability[Reliability.FirstDraft]);
        expect(component.statusName).toEqual(Status[Status.InProgress]);
        expect(component.translatedPictureAltText).toBe('de updated description');
    });

    it('should open picture dialog', () => {
        dialogMock.open.mockClear();
        component.openPicture();
        expect(dialogMock.open).toHaveBeenCalledTimes(1);
    });

    it('should calculate translated picture alt text', () => {
        expect(component.translatedPictureAltText).toBe('de description');

        translateServiceMock.getCurrentLang = jest.fn(() => 'fr');
        onLangChangeEventEmitter.emit();

        expect(component.translatedPictureAltText).toBe('fr description');
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(component['unsubscribe'], 'next');
        jest.spyOn(component['unsubscribe'], 'complete');
        component.ngOnDestroy();
        expect(component['unsubscribe'].next).toHaveBeenCalledTimes(1);
        expect(component['unsubscribe'].complete).toHaveBeenCalledTimes(1);
    });

    it('should generate email link', () => {
        expect(component.emailLink).toEqual('mailto:BK_DevOPS@bit.admin.ch?subject=feedback.subject&body=feedback.body%0D%0Dhttps://test.ch/entry/1');

        expect(translateServiceMock.instant).toHaveBeenCalledWith('feedback.body');
        expect(translateServiceMock.instant).toHaveBeenCalledWith('feedback.subject', {entryId: 1});
    });

    it('should update email link when language changes', () => {
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(2);

        component.entry!.id = 2;

        onLangChangeEventEmitter.next();

        expect(component.emailLink).toEqual('mailto:BK_DevOPS@bit.admin.ch?subject=feedback.subject&body=feedback.body%0D%0Dhttps://test.ch/entry/2');
        expect(translateServiceMock.instant).toHaveBeenCalledTimes(4);
    });

    it('should not generate email link when entry is undefined', () => {
        component.entry = undefined;

        expect(component.emailLink).toEqual('mailto:BK_DevOPS@bit.admin.ch?subject=feedback.subject&body=feedback.body%0D%0Dhttps://test.ch/entry/1');

        component.emailLink = '';

        onLangChangeEventEmitter.next();

        expect(component.emailLink).toEqual('');
    });

    it('should copy shortlink', () => {
        jest.clearAllMocks();
        clipboardMock.copy.mockReturnValue(true);
        const expectedLink = 'https://test.ch/entry/1';

        component.copyAsEntryUri();

        expect(clipboardMock.copy).toHaveBeenCalledTimes(1);
        expect(clipboardMock.copy).toHaveBeenCalledWith(expectedLink);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });

    it('should not copy shortlink if entry is undefined', () => {
        jest.clearAllMocks();
        component.entry = undefined;
        clipboardMock.copy.mockReturnValue(true);

        component.copyAsEntryUri();

        expect(clipboardMock.copy).toHaveBeenCalledTimes(0);
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(0);
    });

    it('should call copy on clipboard when copyToClipboard-function is called ', () => {
        jest.clearAllMocks();
        clipboardMock.copy.mockReturnValue(true);

        component.copyToClipboard('xyz');

        expect(clipboardMock.copy).toHaveBeenCalledTimes(1);
        expect(clipboardMock.copy).toHaveBeenCalledWith('xyz');
        expect(notificationServiceMock.success).toHaveBeenCalledTimes(1);
    });
});
