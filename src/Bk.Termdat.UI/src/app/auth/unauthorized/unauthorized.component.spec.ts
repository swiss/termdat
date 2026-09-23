import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslatePipe} from '@ngx-translate/core';
import {MockPipe} from 'ng-mocks';
import {BehaviorSubject} from 'rxjs';
import {ConfigsService} from '../../config.service';
import {AuthService} from '../auth.service';
import {Role} from '../Role';
import {UnauthorizedComponent} from './unauthorized.component';

/* eslint-disable dot-notation */
describe('UnauthorizedComponent', () => {
    let component: UnauthorizedComponent;
    let fixture: ComponentFixture<UnauthorizedComponent>;

    const rolesSubject = new BehaviorSubject<Role[]>([]);
    const authServiceMock = {
        roles$: rolesSubject.asObservable(),
    };

    const configServiceMock = {
        frontendConfig: {
            requestAccessLink: 'http://some.link',
            appUrl: '',
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [UnauthorizedComponent, MockPipe(TranslatePipe)],
            providers: [
                {provide: AuthService, useValue: authServiceMock},
                {provide: ConfigsService, useValue: configServiceMock},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(UnauthorizedComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show access request url with empty roles', async () => {
        component.showRequestAccessLink = false;
        component.requestAccessLink = '';
        rolesSubject.next([]);

        component.ngOnInit();
        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.showRequestAccessLink).toBeTruthy();
        expect(component.requestAccessLink).toBeTruthy();
    });

    it('should hide access request url with roles', async () => {
        component.showRequestAccessLink = false;
        component.requestAccessLink = '';
        rolesSubject.next([Role.SuperUser]);

        component.ngOnInit();
        fixture.detectChanges();
        await fixture.whenStable();

        expect(component.showRequestAccessLink).toBeFalsy();
        expect(component.requestAccessLink).toBe('');
    });

    it('should unsubscribe', () => {
        jest.spyOn(component['unsubscribe'], 'next');
        jest.spyOn(component['unsubscribe'], 'complete');

        component.ngOnDestroy();

        expect(component['unsubscribe'].next).toHaveBeenCalled();
        expect(component['unsubscribe'].complete).toHaveBeenCalled();
    });
});
