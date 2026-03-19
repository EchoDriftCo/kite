import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EquipmentFilterComponent } from './equipment-filter.component';

describe('EquipmentFilterComponent', () => {
  let component: EquipmentFilterComponent;
  let fixture: ComponentFixture<EquipmentFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EquipmentFilterComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EquipmentFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
