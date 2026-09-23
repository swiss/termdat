import {Component, EventEmitter, Input, Output, OnChanges} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {NoteTemplatesDataService} from './note-templates-data.service';
import {TemplateDataContainer} from './TemplateDataContainer';
import {TemplateMenu} from './TemplateMenu';

@Component({
    selector: 'td-note-templates',
    imports: [MatIcon, MatIconButton, MatMenu, MatMenuItem, MatTooltip, ObButtonDirective, TranslatePipe, MatMenuTrigger],
    templateUrl: './note-templates.component.html',
    styleUrl: './note-templates.component.scss',
})
export class NoteTemplatesComponent implements OnChanges {
    @Input() language: string = 'de';
    @Output() readonly templateSelected = new EventEmitter<string>();
    templateMenuItems: TemplateMenu[] | undefined;
    selectedTemplateMenu: TemplateMenu | undefined;

    constructor(private readonly noteTemplateDataService: NoteTemplatesDataService) {}

    ngOnChanges(): void {
        this.updateTemplateMenuItems();
    }

    setSelectedMenu(menu: TemplateMenu): void {
        this.selectedTemplateMenu = menu;
    }

    selectTemplate(selectedTemplate: string): void {
        this.templateSelected.emit(selectedTemplate);
    }

    private updateTemplateMenuItems(): void {
        const templateData = this.noteTemplateDataService.getTemplateDataContainer();
        const supportedLanguages = Object.keys(templateData);
        const languageToUse = supportedLanguages.includes(this.language) ? this.language : 'de';
        this.templateMenuItems = templateData[languageToUse as keyof TemplateDataContainer];
    }
}
