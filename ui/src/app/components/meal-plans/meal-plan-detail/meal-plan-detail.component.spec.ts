import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';

import { MealPlanDetailComponent } from './meal-plan-detail.component';
import { MealPlanService } from '../../../services/meal-plan.service';
import { MealPlan, MealSlot } from '../../../models/meal-plan.model';

// The API stores plan dates as timestamptz, so they arrive as UTC midnight.
// In any time zone west of UTC, new Date() puts those on the previous day.
const plan: MealPlan = {
  mealPlanResourceId: 'mp1',
  name: 'Week of Oct 19',
  startDate: '2026-10-19T00:00:00Z',
  endDate: '2026-10-25T00:00:00Z',
  entries: [
    { date: '2026-10-20T00:00:00Z', mealSlot: MealSlot.Breakfast, recipeResourceId: 'r3', recipeTitle: 'Pancakes', servings: 2, isLeftover: false },
  ],
} as MealPlan;

describe('MealPlanDetailComponent', () => {
  let component: MealPlanDetailComponent;
  let fixture: ComponentFixture<MealPlanDetailComponent>;
  let mealPlanService: jasmine.SpyObj<MealPlanService>;

  beforeEach(async () => {
    mealPlanService = jasmine.createSpyObj('MealPlanService', ['getMealPlan', 'updateMealPlan']);
    mealPlanService.getMealPlan.and.returnValue(of(plan));
    mealPlanService.updateMealPlan.and.returnValue(of(plan));

    await TestBed.configureTestingModule({
      imports: [MealPlanDetailComponent],
      providers: [
        { provide: MealPlanService, useValue: mealPlanService },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: new Map([['id', 'mp1']]) } } },
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) },
        { provide: MatDialog, useValue: jasmine.createSpyObj('MatDialog', ['open']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MealPlanDetailComponent);
    component = fixture.componentInstance;
    component.ngOnInit();
  });

  it('starts the grid on the plan start date', () => {
    expect(component.calendarDays.length).toBe(7);
    expect(component.calendarDays[0].dateStr).toBe('2026-10-19');
    expect(component.calendarDays[6].dateStr).toBe('2026-10-25');
  });

  it('puts entries under their own date', () => {
    const day = component.calendarDays.find(d => d.dateStr === '2026-10-20')!;
    expect(day.entries.length).toBe(1);
  });

  it('saves plan and entry dates unchanged', () => {
    component.toggleLeftover(plan.entries[0]);

    const request = mealPlanService.updateMealPlan.calls.mostRecent().args[1];
    expect(request.startDate).toBe('2026-10-19');
    expect(request.endDate).toBe('2026-10-25');
    expect(request.entries![0].date).toBe('2026-10-20');
  });
});
