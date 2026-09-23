import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {ConfigsService} from '../../config.service';
import {MailboxComponent} from './mailbox.component';

describe('MailboxComponent', () => {
    let component: MailboxComponent;
    let fixture: ComponentFixture<MailboxComponent>;

    const configServiceMock = {
        frontendConfig: {
            feedbackEmail: 'BK_DevOPS@bit.admin.ch',
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ReactiveFormsModule, MockModule(TranslateModule), MockModule(MatFormFieldModule), MailboxComponent],
            providers: [{provide: ConfigsService, useValue: configServiceMock}],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(MailboxComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show email adress as disabled', async () => {
        await fixture.whenStable();

        expect(component.form.controls.email.value).toEqual(configServiceMock.frontendConfig.feedbackEmail);
        const emailControl = fixture.debugElement.query(By.css('[formControlName=email]'));
        expect(emailControl).toBeTruthy();
        expect(emailControl.nativeElement.disabled).toEqual(true);
    });
});
