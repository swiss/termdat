import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {MockPipe} from 'ng-mocks';
import {PictureMetadataComponent} from './picture-metadata.component';

describe('PictureMetadataComponent', () => {
    let component: PictureMetadataComponent;
    let fixture: ComponentFixture<PictureMetadataComponent>;

    const translateServiceMock = {
        getCurrentLang: jest.fn(() => 'de'),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [PictureMetadataComponent, MockPipe(TranslatePipe)],
            providers: [
                {
                    provide: TranslateService,
                    useValue: translateServiceMock,
                },
            ],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(PictureMetadataComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
