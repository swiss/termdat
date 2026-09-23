import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {ReactiveFormsModule, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {MatTooltip} from '@angular/material/tooltip';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObButtonDirective} from '@oblique/oblique';
import {DomainSummary} from 'api/DomainSummary';
import {SubjectSummary} from 'api/SubjectSummary';
import {Subject} from 'rxjs';
import {catchError, switchMap, takeUntil} from 'rxjs/operators';
import {TermdatClassificationService} from '../../administration/termdat-classification/termdat-classification.service';
import {SubjectEntryComponent} from './subject-entry/subject-entry.component';

@Component({
    selector: 'td-subject-form',
    templateUrl: './subject-form.component.html',
    styleUrls: ['./subject-form.component.scss'],
    imports: [SubjectEntryComponent, ReactiveFormsModule, MatIconButton, ObButtonDirective, MatTooltip, MatIcon, TranslatePipe],
})
export class SubjectFormComponent implements OnInit, OnDestroy {
    @Input() formArray!: UntypedFormArray;
    @Input() entryDomains: DomainSummary[] = [];
    @Input() entrySubjects: SubjectSummary[] = [];
    @Input() required: boolean = true;

    subjects: SubjectSummary[] = [];

    private readonly unsubscribe = new Subject<void>();

    constructor(
        private readonly formBuilder: UntypedFormBuilder,
        private readonly termdatClassificationService: TermdatClassificationService,
        private readonly translateService: TranslateService
    ) {}

    ngOnInit(): void {
        this.termdatClassificationService
            .getSubjects()
            .pipe(
                catchError(() => []),
                takeUntil(this.unsubscribe)
            )
            .subscribe(subjects => {
                this.subjects = subjects.sort((a, b) => a.name.localeCompare(b.name, this.translateService.getCurrentLang(), {sensitivity: 'accent'}));
                this.setupForm(this.entrySubjects);
            });

        this.translateService.onLangChange
            .pipe(
                switchMap(() => this.termdatClassificationService.getSubjects()),
                takeUntil(this.unsubscribe)
            )
            .subscribe(subjects => (this.subjects = subjects));
    }

    ngOnDestroy(): void {
        this.unsubscribe.next();
        this.unsubscribe.complete();
    }

    addSubject() {
        this.addEmptySubject();
        this.markForm();
    }

    removeSubject(index: number) {
        this.formArray.removeAt(index);
        this.markForm();
    }

    private setupForm(subjects?: SubjectSummary[]): void {
        if (!subjects?.length || !this.formArray) {
            this.addEmptySubject();
            return;
        }

        this.formArray.clear();
        subjects.forEach(s => {
            const entryDomains: DomainSummary[] = this.entryDomains.filter(d => d.subjectId === s.id) ?? [];
            const formGroup = this.formBuilder.group({
                subject: this.formBuilder.control(s.id, {validators: this.required ? [Validators.required] : []}),
                domains: this.formBuilder.control(entryDomains.map(d => d.id)),
            });
            this.formArray.push(formGroup);
        });
    }

    private addEmptySubject() {
        const formGroup = new UntypedFormGroup({
            subject: this.formBuilder.control(null, {validators: this.required ? [Validators.required] : []}),
            domains: this.formBuilder.control({value: [], disabled: true}),
        });
        this.formArray.push(formGroup);
    }

    private markForm(): void {
        this.formArray.markAsDirty();
        this.formArray.markAsTouched();
    }
}
