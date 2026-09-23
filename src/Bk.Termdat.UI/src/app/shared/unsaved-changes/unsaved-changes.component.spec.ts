import {DebugElement} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatDialogActions, MatDialogContent, MatDialogModule, MatDialogRef, MatDialogTitle} from '@angular/material/dialog';
import {By} from '@angular/platform-browser';
import {TranslateModule} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {UnsavedChangeAction} from './unsaved-change-action';
import {UnsavedChangesComponent} from './unsaved-changes.component';

describe('UnsavedChangesComponent', () => {
    let component: UnsavedChangesComponent;
    let fixture: ComponentFixture<UnsavedChangesComponent>;
    const dialogRefMock = {
        close: jest.fn(),
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [{provide: MatDialogRef, useValue: dialogRefMock}],
            imports: [MockModule(MatDialogModule), TranslateModule.forRoot(), UnsavedChangesComponent],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(UnsavedChangesComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    afterEach(() => {
        dialogRefMock.close.mockClear();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should render dialog', () => {
        const element: DebugElement = fixture.debugElement;

        const heading = element.query(By.directive(MatDialogTitle));
        expect(heading).toBeTruthy();
        expect(heading.nativeElement.textContent).toContain('unsavedChanges.title');

        const content = element.query(By.directive(MatDialogContent));
        expect(content).toBeTruthy();
        expect(content.nativeElement.textContent).toContain('unsavedChanges.text');

        const actions = element.query(By.directive(MatDialogActions));
        expect(actions).toBeTruthy();

        const abort = actions.query(By.css('#unsaved-abort'));
        expect(abort).toBeTruthy();
        expect(abort.nativeElement.textContent).toContain('unsavedChanges.abort');

        const revert = actions.query(By.css('#unsaved-revert'));
        expect(revert).toBeTruthy();
        expect(revert.nativeElement.textContent).toContain('unsavedChanges.revert');

        const save = actions.query(By.css('#unsaved-save'));
        expect(save).toBeTruthy();
        expect(save.nativeElement.textContent).toContain('unsavedChanges.save');
    });

    it('closes dialog with REVERT when revert button is clicked', () => {
        const element: DebugElement = fixture.debugElement;

        const revert = element.query(By.css('#unsaved-revert'));
        revert.triggerEventHandler('click', {});

        expect(dialogRefMock.close).toHaveBeenCalledTimes(1);
        expect(dialogRefMock.close).toHaveBeenCalledWith(UnsavedChangeAction.DISCARD);
    });

    it('closes dialog with SAVE when revert button is clicked', () => {
        const element: DebugElement = fixture.debugElement;

        const save = element.query(By.css('#unsaved-save'));
        save.triggerEventHandler('click', {});

        expect(dialogRefMock.close).toHaveBeenCalledTimes(1);
        expect(dialogRefMock.close).toHaveBeenCalledWith(UnsavedChangeAction.SAVE);
    });
});
