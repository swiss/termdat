import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatIconModule} from '@angular/material/icon';
import {TranslatePipe} from '@ngx-translate/core';
import {ObMockAlertModule} from '@oblique/oblique';
import {MockModule, MockPipe} from 'ng-mocks';
import {ConfirmDeleteDialogComponent} from './confirm-delete-dialog.component';

describe('ConfirmDeleteDialogComponent', () => {
    let component: ConfirmDeleteDialogComponent;
    let fixture: ComponentFixture<ConfirmDeleteDialogComponent>;

    const matDialogRef = {};
    const data = {};

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MockModule(MatDialogModule), MockModule(MatIconModule), ObMockAlertModule, ConfirmDeleteDialogComponent, MockPipe(TranslatePipe)],
            providers: [
                {provide: MatDialogRef, useValue: matDialogRef},
                {provide: MAT_DIALOG_DATA, useValue: data},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(ConfirmDeleteDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
