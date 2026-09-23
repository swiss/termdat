import {EventEmitter} from '@angular/core';
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatIconModule} from '@angular/material/icon';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {MockComponent, MockModule, MockPipe} from 'ng-mocks';
import {PictureMetadataComponent} from '../picture-metadata/picture-metadata.component';
import {PictureDialogComponent} from './picture-dialog.component';

describe('PictureDialogComponent', () => {
    let component: PictureDialogComponent;
    let fixture: ComponentFixture<PictureDialogComponent>;

    const dialogRefMock = {
        close: jest.fn(),
    };

    const onLangChangeEventEmitter = new EventEmitter();

    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
        onLangChange: onLangChangeEventEmitter,
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatDialogModule),
                MockModule(MatButtonModule),
                MockModule(MatIconModule),
                PictureDialogComponent,
                MockPipe(TranslatePipe),
                MockComponent(PictureMetadataComponent),
            ],
            providers: [
                {provide: MatDialogRef, useValue: dialogRefMock},
                {
                    provide: MAT_DIALOG_DATA,
                    useValue: {
                        descriptionDe: 'de description',
                        descriptionFr: 'fr description',
                    },
                },
                {provide: TranslateService, useValue: translateServiceMock},
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(PictureDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should calculate translated picture alt text', () => {
        expect(component.translatedPictureAltText).toBe('de description');

        translateServiceMock.getCurrentLang = jest.fn(() => 'fr');
        onLangChangeEventEmitter.emit();

        expect(component.translatedPictureAltText).toBe('fr description');
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
