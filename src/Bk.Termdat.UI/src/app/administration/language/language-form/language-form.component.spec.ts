import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {RouterLink} from '@angular/router';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {ObUnsavedChangesDirective} from '@oblique/oblique';
import {MockDirectives, MockModule, MockPipe} from 'ng-mocks';
import {of, Subject} from 'rxjs';
import {LanguageService} from '../../../shared/language.service';
import {LanguageFormComponent} from './language-form.component';

describe('LanguageFormComponent', () => {
    let component: LanguageFormComponent;
    let fixture: ComponentFixture<LanguageFormComponent>;

    const onLangChangeSubject = new Subject();
    const translateServiceMock = {
        onLangChange: onLangChangeSubject.asObservable(),
        instant: jest.fn(),
    };

    const languageServiceMock = {
        nextSort: jest.fn(),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ReactiveFormsModule,
                MockModule(MatIconModule),
                MockModule(MatFormFieldModule),
                MockModule(MatSlideToggleModule),
                LanguageFormComponent,
                MockPipe(TranslatePipe),
                MockDirectives(RouterLink, ObUnsavedChangesDirective),
            ],
            providers: [
                {provide: TranslateService, useValue: translateServiceMock},
                {provide: LanguageService, useValue: languageServiceMock},
            ],
        }).compileComponents();

        languageServiceMock.nextSort.mockReturnValue(of(101));

        fixture = TestBed.createComponent(LanguageFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        await fixture.whenStable();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call save on click', () => {
        jest.spyOn(component.saved, 'emit');

        component.form.patchValue({
            isoCode: 'IT',
            descriptionDe: 'Italienisch',
            descriptionFr: 'Italien',
            descriptionIt: 'Italiano',
            descriptionRm: 'Italiano',
            descriptionEn: 'Italian',
        });

        component.save();

        expect(component.saved.emit).toHaveBeenCalledTimes(1);
    });

    it('cannot save when invalid', async () => {
        jest.spyOn(component.saved, 'emit');

        component.save();

        expect(component.saved.emit).not.toHaveBeenCalled();
    });

    it('should destroy properly', () => {
        /* eslint-disable dot-notation */
        jest.spyOn(component['unsubscribe'], 'next');
        jest.spyOn(component['unsubscribe'], 'complete');
        component.ngOnDestroy();
        expect(component['unsubscribe'].next).toHaveBeenCalledTimes(1);
        expect(component['unsubscribe'].complete).toHaveBeenCalledTimes(1);
    });
});
