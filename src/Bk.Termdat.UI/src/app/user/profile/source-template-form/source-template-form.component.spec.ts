import {ComponentFixture, TestBed} from '@angular/core/testing';
import {FormBuilder, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDialog} from '@angular/material/dialog';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatMenuModule} from '@angular/material/menu';
import {TranslateModule} from '@ngx-translate/core';
import {MockModule} from 'ng-mocks';
import {SourceTemplateFormComponent} from './source-template-form.component';

describe('SourceTemplateFormComponent', () => {
    let component: SourceTemplateFormComponent;
    let fixture: ComponentFixture<SourceTemplateFormComponent>;
    let formBuilder: FormBuilder;

    const dialogRefMock = {
        afterClosed: jest.fn(),
    };
    const dialogMock = {
        open: jest.fn(() => dialogRefMock),
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                MockModule(MatMenuModule),
                MockModule(TranslateModule),
                MockModule(MatIconModule),
                MockModule(MatButtonModule),
                MockModule(MatInputModule),
                MockModule(TranslateModule),
                FormsModule,
                ReactiveFormsModule,
                SourceTemplateFormComponent,
            ],
            providers: [{provide: MatDialog, useValue: dialogMock}],
        }).compileComponents();

        formBuilder = new FormBuilder();
        fixture = TestBed.createComponent(SourceTemplateFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call create event', () => {
        const spy = jest.spyOn(component.create, 'emit');

        component.form = formBuilder.group({
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        });
        component.updateMode = false;
        component.createOrUpdateSourceTemplate();

        expect(spy).toHaveBeenCalledTimes(1);
        expect(spy).toHaveBeenCalledWith({
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        });
    });

    it.each([true, false])('should not call event if empty, updateMode %s', inUpdateMode => {
        const spy = jest.spyOn(component.create, 'emit');

        component.form = formBuilder.group({
            textDe: '',
            textFr: '',
            textIt: '',
            textEn: '',
            textRm: '',
        });
        component.updateMode = inUpdateMode;
        component.createOrUpdateSourceTemplate();

        expect(component.isEmpty).toEqual(true);
        expect(spy).toHaveBeenCalledTimes(0);
    });

    it('should call update event', () => {
        const spy = jest.spyOn(component.update, 'emit');
        component.sourceTemplate = {
            id: 1,
            textDe: 'Old Text DE',
            textFr: 'Old Text FR',
            textIt: 'Old Text IT',
            textEn: 'Old Text EN',
            textRm: 'Old Text RM',
        };
        component.form = formBuilder.group({
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        });
        component.updateMode = true;
        component.createOrUpdateSourceTemplate();

        expect(spy).toHaveBeenCalledTimes(1);
        expect(spy).toHaveBeenCalledWith({
            id: 1,
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        });
    });

    it('should reset fields in update mode', () => {
        component.form = formBuilder.group({
            textDe: 'Text DE',
            textFr: 'Text FR',
            textIt: 'Text IT',
            textEn: 'Text EN',
            textRm: 'Text RM',
        });
        component.updateMode = false;
        component.resetFields();

        expect(component.form.controls.textDe.value).toEqual('');
        expect(component.form.controls.textFr.value).toEqual('');
        expect(component.form.controls.textIt.value).toEqual('');
        expect(component.form.controls.textEn.value).toEqual('');
        expect(component.form.controls.textRm.value).toEqual('');
    });

    it('should call delete event', () => {
        const spy = jest.spyOn(component.delete, 'emit');
        component.sourceTemplate = {
            id: 1,
            textDe: 'Old Text DE',
            textFr: 'Old Text FR',
            textIt: 'Old Text IT',
            textEn: 'Old Text EN',
            textRm: 'Old Text RM',
        };
        component.updateMode = true;
        component.deleteEntry();

        expect(spy).toHaveBeenCalledTimes(1);
        expect(spy).toHaveBeenCalledWith(component.sourceTemplate);
    });
});
