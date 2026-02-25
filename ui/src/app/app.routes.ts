import { Routes } from '@angular/router';
import { RecipeListComponent } from './components/recipes/recipe-list/recipe-list.component';
import { RecipeDetailComponent } from './components/recipes/recipe-detail/recipe-detail.component';
import { RecipeFormComponent } from './components/recipes/recipe-form/recipe-form.component';
import { CookingModeComponent } from './components/recipes/cooking-mode/cooking-mode.component';
import { MealPlanListComponent } from './components/meal-plans/meal-plan-list/meal-plan-list.component';
import { MealPlanFormComponent } from './components/meal-plans/meal-plan-form/meal-plan-form.component';
import { MealPlanDetailComponent } from './components/meal-plans/meal-plan-detail/meal-plan-detail.component';
import { GroceryListComponent } from './components/meal-plans/grocery-list/grocery-list.component';
import { CircleListComponent } from './components/circles/circle-list/circle-list.component';
import { CircleDetailComponent } from './components/circles/circle-detail/circle-detail.component';
import { AcceptInviteComponent } from './components/circles/accept-invite/accept-invite.component';
import { LoginComponent } from './components/login/login.component';
import { SharedRecipeComponent } from './components/recipes/shared-recipe/shared-recipe.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/recipes', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'share/:token', component: SharedRecipeComponent },
  { path: 'join/:token', component: AcceptInviteComponent, canActivate: [authGuard] },
  { path: 'recipes', component: RecipeListComponent, canActivate: [authGuard] },
  { path: 'recipes/new', component: RecipeFormComponent, canActivate: [authGuard] },
  { path: 'recipes/:id', component: RecipeDetailComponent, canActivate: [authGuard] },
  { path: 'recipes/:id/edit', component: RecipeFormComponent, canActivate: [authGuard] },
  { path: 'recipes/:id/cook', component: CookingModeComponent, canActivate: [authGuard] },
  { path: 'circles', component: CircleListComponent, canActivate: [authGuard] },
  { path: 'circles/:id', component: CircleDetailComponent, canActivate: [authGuard] },
  { path: 'meal-plans', component: MealPlanListComponent, canActivate: [authGuard] },
  { path: 'meal-plans/new', component: MealPlanFormComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id', component: MealPlanDetailComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id/edit', component: MealPlanFormComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id/grocery-list', component: GroceryListComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/recipes' }
];
