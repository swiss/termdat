import {Component} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ReactiveFormsModule, FormControl, FormGroup} from '@angular/forms';
import {ConfigsService} from '../../config.service';
import {RichTextEditorComponent} from './rich-text-editor.component';

@Component({
    standalone: true,
    imports: [ReactiveFormsModule, RichTextEditorComponent],
    template: `
        <form [formGroup]="form">
            <td-rich-text-editor [controlName]="'text'" />
        </form>
    `,
})
class TestHostComponent {
    form = new FormGroup({
        text: new FormControl(''),
    });
}

describe('RichTextEditorComponent', () => {
    let fixture: ComponentFixture<TestHostComponent>;

    const configServiceMock = {
        frontendConfig: {
            froalaKey: 'test-key',
        },
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [TestHostComponent],
            providers: [
                {
                    provide: ConfigsService,
                    useValue: configServiceMock,
                },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(TestHostComponent);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(fixture.componentInstance).toBeTruthy();
    });
});
