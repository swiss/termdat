import {ComponentFixture, TestBed} from '@angular/core/testing';
import {NewsItemSummary} from 'api/NewsItemSummary';
import {NewsItemBlockComponent} from './news-item-block.component';

describe('NewsItemBlockComponent', () => {
    let component: NewsItemBlockComponent;
    let fixture: ComponentFixture<NewsItemBlockComponent>;

    const mockNewsItem: NewsItemSummary = {
        id: 1,
        created: new Date(2024, 1, 1),
        createdBy: 'Bruce',
        modified: new Date(2024, 12, 1),
        modifiedBy: 'Bruce',
        title: 'Test news title',
        text: 'Test news text',
        validFrom: new Date(2025, 1, 1),
        validTo: new Date(2025, 2, 2),
        canMoveDown: true,
        canMoveUp: true,
        isEnabled: true,
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [NewsItemBlockComponent],
        }).compileComponents();

        fixture = TestBed.createComponent(NewsItemBlockComponent);
        component = fixture.componentInstance;

        component.newsItem = mockNewsItem;

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should display news data', () => {
        const compiled = fixture.nativeElement as HTMLElement;

        expect(compiled.querySelector('.news-date')?.textContent?.trim()).toBeTruthy();
        expect(compiled.querySelector('h3')?.textContent).toContain('Test news title');
        expect(compiled.querySelector('.news-text')?.textContent).toContain('Test news text');
    });
});
