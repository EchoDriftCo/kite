import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EquipmentSetupComponent } from './equipment-setup.component';

describe('EquipmentSetupComponent', () => {
  let component: EquipmentSetupComponent;
  let fixture: ComponentFixture<EquipmentSetupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EquipmentSetupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EquipmentSetupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
