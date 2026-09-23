import {ChangeDetectionStrategy, Component, computed, EventEmitter, inject, input, OnDestroy, Output} from '@angular/core';
import {rxResource, toSignal} from '@angular/core/rxjs-interop';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {SourceTemplatesService} from './source-templates.service';
import {SourceTemplateDefaults} from './SourceTemplateDefaults';
import {SourceTemplatesModel} from './SourceTemplatesModel';
import {SourceTemplateType} from './SourceTemplateType';

@Component({
    selector: 'td-source-templates',
    templateUrl: './source-templates.component.html',
    styleUrls: ['./source-templates.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconButton, ObButtonDirective, MatTooltip, MatMenuTrigger, MatIcon, MatMenu, MatMenuItem, TranslatePipe],
})
export class SourceTemplatesComponent implements OnDestroy {
    language = input<keyof SourceTemplateDefaults>('de');
    @Output() readonly templateSelected = new EventEmitter<string>();
    templateTypes = SourceTemplateType;
    readonly sourceTemplatesService = inject(SourceTemplatesService);
    readonly sourceTemplatesByUser = computed(() => this.sourceTemplatesByUserResource.value() ?? []);

    private readonly sourceTemplateDefaults = toSignal(this.sourceTemplatesService.getSourceTemplateDefaults());
    private readonly sourceTemplateModel = computed(() => {
        const sourceTemplateDefaults = this.sourceTemplateDefaults();
        return sourceTemplateDefaults ? new SourceTemplatesModel(sourceTemplateDefaults) : undefined;
    });

    private readonly sourceTemplatesByUserResource = rxResource({
        params: () => this.language(),
        stream: ({params}) => this.sourceTemplatesService.getSourceTemplateTexts(params),
    });

    ngOnDestroy(): void {
        this.templateSelected.complete();
    }

    selectTemplate(type: SourceTemplateType): void {
        const sourceTemplateModel = this.sourceTemplateModel();
        if (!sourceTemplateModel) {
            return;
        }

        this.templateSelected.emit(sourceTemplateModel.getByType(type, this.language()));
    }

    selectTemplateByUser(selectedTemplate: string): void {
        this.templateSelected.emit(selectedTemplate);
    }
}
