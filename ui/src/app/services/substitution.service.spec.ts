import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { SubstitutionService } from './substitution.service';
import { ApiService } from './api.service';

describe('SubstitutionService', () => {
  let service: SubstitutionService;
  let mockApiService: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);

    TestBed.configureTestingModule({
      providers: [
        SubstitutionService,
        { provide: ApiService, useValue: mockApiService }
      ]
    });

    service = TestBed.inject(SubstitutionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call API with correct endpoint for getSubstitutions', () => {
    const mockResponse = { substitutions: [] };
    mockApiService.post.and.returnValue(of(mockResponse));

    service.getSubstitutions('recipe-123', [0, 1], ['Gluten-Free']).subscribe();

    expect(mockApiService.post).toHaveBeenCalledWith(
      'recipes/recipe-123/substitutions',
      {
        ingredientIndices: [0, 1],
        dietaryConstraints: ['Gluten-Free']
      }
    );
  });

  it('should call API with correct endpoint for applySubstitutions', () => {
    const mockRecipe = { recipeResourceId: 'new-recipe-456' } as any;
    mockApiService.post.and.returnValue(of(mockRecipe));

    const selections = [{ ingredientIndex: 0, optionIndex: 0 }];
    service.applySubstitutions('recipe-123', selections, 'Modified Recipe').subscribe();

    expect(mockApiService.post).toHaveBeenCalledWith(
      'recipes/recipe-123/substitutions/apply',
      {
        selections,
        forkTitle: 'Modified Recipe'
      }
    );
  });

  it('should handle optional forkTitle', () => {
    const mockRecipe = { recipeResourceId: 'new-recipe-456' } as any;
    mockApiService.post.and.returnValue(of(mockRecipe));

    const selections = [{ ingredientIndex: 0, optionIndex: 0 }];
    service.applySubstitutions('recipe-123', selections).subscribe();

    expect(mockApiService.post).toHaveBeenCalledWith(
      'recipes/recipe-123/substitutions/apply',
      { selections }
    );
  });
});
