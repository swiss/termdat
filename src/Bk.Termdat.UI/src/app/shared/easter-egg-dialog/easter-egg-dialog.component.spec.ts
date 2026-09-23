import {ComponentFixture, TestBed} from '@angular/core/testing';
import {EasterEggDialogComponent} from './easter-egg-dialog.component';

describe('EasterEggDialogComponent', () => {
    let component: EasterEggDialogComponent;
    let fixture: ComponentFixture<EasterEggDialogComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [EasterEggDialogComponent],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(EasterEggDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
