import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { of } from 'rxjs';

import { DiscoverComponent } from './discover.component';
import { RecipeService } from '../../../services/recipe.service';

describe('DiscoverComponent', () => {
  let component: DiscoverComponent;
  let fixture: ComponentFixture<DiscoverComponent>;
  let recipeServiceSpy: jasmine.SpyObj<RecipeService>;

  const mockPagedResult = {
    items: [],
    pageNumber: 1,
    pageSize: 12,
    totalCount: 0,
    totalPages: 0
  };

  beforeEach(async () => {
    recipeServiceSpy = jasmine.createSpyObj('RecipeService', ['discoverRecipes', 'forkRecipe']);
    recipeServiceSpy.discoverRecipes.and.returnValue(of(mockPagedResult));

    await TestBed.configureTestingModule({
      imports: [DiscoverComponent],
      providers: [
        { provide: RecipeService, useValue: recipeServiceSpy },
        provideRouter([]),
        provideAnimationsAsync()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DiscoverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load recipes on init', () => {
    expect(recipeServiceSpy.discoverRecipes).toHaveBeenCalledOnceWith(
      jasmine.objectContaining({ sortBy: 'newest', pageNumber: 1, pageSize: 12 })
    );
  });

  it('should reset page to 1 on search', () => {
    component.pageNumber = 3;
    component.searchTitle = 'pasta';
    component.onSearch();
    expect(component.pageNumber).toBe(1);
    expect(recipeServiceSpy.discoverRecipes).toHaveBeenCalledWith(
      jasmine.objectContaining({ title: 'pasta', pageNumber: 1 })
    );
  });

  it('should reset page to 1 on sort change', () => {
    component.pageNumber = 2;
    component.sortBy = 'popular';
    component.onSortChange();
    expect(component.pageNumber).toBe(1);
    expect(recipeServiceSpy.discoverRecipes).toHaveBeenCalledWith(
      jasmine.objectContaining({ sortBy: 'popular', pageNumber: 1 })
    );
  });

  it('should display empty state when no recipes', () => {
    component.recipes = [];
    fixture.detectChanges();
    const el = fixture.nativeElement.querySelector('.empty-state');
    expect(el).toBeTruthy();
  });
});
