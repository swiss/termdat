import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {provideHttpClientTesting} from '@angular/common/http/testing';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslatePipe} from '@ngx-translate/core';
import {MockComponent, MockPipe} from 'ng-mocks';
import {BehaviorSubject, of} from 'rxjs';
import {AuthService} from '../../auth/auth.service';
import {UserInfo} from '../../auth/UserInfo';
import {ConfigsService} from '../../config.service';
import {SourceTemplatesService} from '../../entry/entry-form/source-templates/source-templates.service';
import {EntryTemplateListComponent} from './entry-template-list/entry-template-list.component';
import {ProfileComponent} from './profile.component';
import {SourceTemplateListComponent} from './source-template-list/source-template-list.component';

describe('ProfileComponent', () => {
    let component: ProfileComponent;
    let fixture: ComponentFixture<ProfileComponent>;
    const isAdminSubject = new BehaviorSubject(false);
    const isSuperUserSubject = new BehaviorSubject(false);
    const isWriterSubject = new BehaviorSubject(false);

    const sourceTemplates = [
        {
            id: 1,
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        },
        {
            id: 2,
            textDe: 'Text DE2',
            textFr: 'Text FR2',
            textIt: 'Text IT2',
            textEn: 'Text EN2',
            textRm: 'Text RM2',
        },
    ];

    const sourceTemplatesServiceMock = {
        getSourceTemplates: jest.fn(),
    };

    const configServiceMock = {
        frontendConfig: {
            roles: {
                allow: 'ALLOW',
            },
        },
    };

    const userInfo: UserInfo = {
        name: 'test',
        email: 'test@test.ch',
        roles: [configServiceMock.frontendConfig.roles.allow, 'test'],
    };

    const authServiceMock = {
        userInfo$: of(userInfo),
        isSuperUser$: isSuperUserSubject.asObservable(),
        isAdmin$: isAdminSubject.asObservable(),
        isWriter$: isWriterSubject.asObservable(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ProfileComponent, MockPipe(TranslatePipe), MockComponent(SourceTemplateListComponent), MockComponent(EntryTemplateListComponent)],
            providers: [
                {provide: AuthService, useValue: authServiceMock},
                {provide: SourceTemplatesService, useValue: sourceTemplatesServiceMock},
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                {provide: ConfigsService, useValue: configServiceMock},
            ],
        }).compileComponents();

        sourceTemplatesServiceMock.getSourceTemplates.mockReturnValue(of(sourceTemplates));
        fixture = TestBed.createComponent(ProfileComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        isAdminSubject.next(true);
        isSuperUserSubject.next(false);
        isWriterSubject.next(false);

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should retrieve user info', () => {
        expect(component.userInfo?.name).toBe(userInfo.name);
        expect(component.userInfo?.email).toBe(userInfo.email);
        expect(component.userInfo?.roles).toStrictEqual([configServiceMock.frontendConfig.roles.allow]);
    });

    it('should retrieve source templates', () => {
        expect(component.userInfo?.sourceTemplates).toBe(sourceTemplates);
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(component['unsubscribe'], 'next');
        jest.spyOn(component['unsubscribe'], 'complete');
        component.ngOnDestroy();
        expect(component['unsubscribe'].next).toHaveBeenCalledTimes(1);
        expect(component['unsubscribe'].complete).toHaveBeenCalledTimes(1);
    });

    it.each([[true, false, false, false, 1]])(
        'should call reloadSourceTemplates with permission',
        (isAdmin, isSuperUser, isWriter, isSuperReader, expectedCalls) => {
            const spyReload = jest.spyOn(component, 'reloadSourceTemplates');

            authServiceMock.isAdmin$ = of(isAdmin);
            authServiceMock.isSuperUser$ = of(isSuperUser);
            authServiceMock.isSuperUser$ = of(isSuperReader);
            authServiceMock.isWriter$ = of(isWriter);

            component.ngOnInit();
            fixture.detectChanges();

            expect(spyReload).toHaveBeenCalledTimes(expectedCalls);
        }
    );
});
