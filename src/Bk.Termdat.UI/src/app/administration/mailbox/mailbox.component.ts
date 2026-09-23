import {Component} from '@angular/core';
import {FormControl, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {MatInput} from '@angular/material/input';
import {TranslatePipe} from '@ngx-translate/core';
import {ConfigsService} from '../../config.service';

@Component({
    selector: 'td-mailbox',
    templateUrl: './mailbox.component.html',
    styleUrls: ['./mailbox.component.scss'],
    imports: [ReactiveFormsModule, MatFormField, MatLabel, MatInput, TranslatePipe],
})
export class MailboxComponent {
    form: UntypedFormGroup;

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        readonly configService: ConfigsService
    ) {
        this.form = this.formBuilder.group({
            email: new FormControl({value: '', disabled: true}),
        });

        this.form.controls.email.setValue(configService.frontendConfig.feedbackEmail);
    }
}
