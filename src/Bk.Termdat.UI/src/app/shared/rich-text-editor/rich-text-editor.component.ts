import 'froala-editor/js/plugins/link.min.js';
import {Component, computed, input} from '@angular/core';
import {ControlContainer, FormGroupDirective, ReactiveFormsModule} from '@angular/forms';
import {FroalaEditorModule} from 'angular-froala-wysiwyg';
import {ConfigsService} from '../../config.service';

@Component({
    selector: 'td-rich-text-editor',
    imports: [ReactiveFormsModule, FroalaEditorModule],
    viewProviders: [
        {
            provide: ControlContainer,
            useExisting: FormGroupDirective,
        },
    ],
    templateUrl: './rich-text-editor.component.html',
    styleUrl: './rich-text-editor.component.scss',
    standalone: true,
})
export class RichTextEditorComponent {
    controlName = input.required<string>();
    heightMin = input<number>(150);

    options = computed(() => ({
        attribution: false,
        key: this.configService.frontendConfig.froalaKey,

        heightMin: this.heightMin(),

        toolbarButtons: {
            moreText: {
                buttons: ['bold', 'italic', 'underline', 'insertLink'],
                buttonsVisible: 4,
            },
            moreMisc: {
                buttons: ['undo', 'redo'],
                align: 'right',
                buttonsVisible: 2,
            },
        },

        pluginsEnabled: ['link'],

        events: {
            // eslint-disable-next-line object-shorthand, @typescript-eslint/no-explicit-any
            'paste.before': function (this: any, e: ClipboardEvent) {
                const text = e.clipboardData?.getData('text/plain');

                if (!text) {
                    e.preventDefault();
                    return false;
                }

                e.preventDefault();

                this.html.insert(text.replace(/\n/g, '<br>'));

                return false;
            },
        },
    }));

    constructor(protected readonly configService: ConfigsService) {}
}
