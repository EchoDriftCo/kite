import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { SubstitutionDialogComponent } from './substitution-dialog.component';
import { SubstitutionService } from '../../../services/substitution.service';
import { Recipe } from '../../../models/recipe.model';

describe('SubstitutionDialogComponent', () => {
  let component: SubstitutionDialogComponent;
  let fixture: ComponentFixture<SubstitutionDialogComponent>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<SubstitutionDialogComponent>>;
  let mockSubstitutionService: jasmine.SpyObj<SubstitutionService>;

  const mockRecipe: Recipe = {
    recipeResourceId: '123',
    title: 'Test Recipe',
    yield: 4,
    ingredients: [
      { sortOrder: 0, item: 'flour', quantity: 2, unit: 'cups' },
      { sortOrder: 1, item: 'milk', quantity: 1, unit: 'cup' }
    ],
    instructions: []
  };

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockSubstitutionService = jasmine.createSpyObj('SubstitutionService', [
      'getSubstitutions',
      'applySubstitutions'
    ]);

    await TestBed.configureTestingModule({
      imports: [SubstitutionDialogComponent, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { recipe: mockRecipe } },
        { provide: SubstitutionService, useValue: mockSubstitutionService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SubstitutionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize ingredient selection list', () => {
    expect(component.ingredientItems.length).toBe(2);
    expect(component.ingredientItems[0].ingredient.item).toBe('flour');
  });

  it('should toggle dietary constraints', () => {
    component.toggleConstraint('Gluten-Free');
    expect(component.hasConstraint('Gluten-Free')).toBe(true);

    component.toggleConstraint('Gluten-Free');
    expect(component.hasConstraint('Gluten-Free')).toBe(false);
  });

  it('should enable proceed button when ingredients or constraints are selected', () => {
    expect(component.canProceedToSuggestions()).toBe(false);

    component.ingredientItems[0].selected = true;
    expect(component.canProceedToSuggestions()).toBe(true);
  });

  it('should call service when fetching suggestions', () => {
    const mockResponse = {
      substitutions: [
        {
          originalIndex: 0,
          originalText: '2 cups flour',
          options: [
            {
              name: 'Gluten-Free Flour',
              ingredients: [{ quantity: 2, unit: 'cups', item: 'gluten-free flour' }],
              notes: 'Works well'
            }
          ]
        }
      ]
    };

    mockSubstitutionService.getSubstitutions.and.returnValue(of(mockResponse));

    component.ingredientItems[0].selected = true;
    component.fetchSuggestions();

    expect(mockSubstitutionService.getSubstitutions).toHaveBeenCalled();
  });
});
