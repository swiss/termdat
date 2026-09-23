import {DatePipe} from '@angular/common';
import {Component, Input} from '@angular/core';
import {NewsItemSummary} from 'api/NewsItemSummary';

@Component({
    selector: 'td-news-item-block',
    imports: [DatePipe],
    templateUrl: './news-item-block.component.html',
    styleUrl: './news-item-block.component.scss',
})
export class NewsItemBlockComponent {
    @Input({required: true}) newsItem!: NewsItemSummary;
}
