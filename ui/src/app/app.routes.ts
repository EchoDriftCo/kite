import { Routes } from '@angular/router';
import { RecipeListComponent } from './components/recipes/recipe-list/recipe-list.component';
import { DiscoverComponent } from './components/recipes/discover/discover.component';
import { RecipeDetailComponent } from './components/recipes/recipe-detail/recipe-detail.component';
import { RecipeFormComponent } from './components/recipes/recipe-form/recipe-form.component';
import { RecipeGeneratorComponent } from './components/recipes/recipe-generator/recipe-generator.component';
import { RecipeMixerComponent } from './components/recipes/recipe-mixer/recipe-mixer.component';
import { CookingModeComponent } from './components/recipes/cooking-mode/cooking-mode.component';
import { CookingHistoryComponent } from './components/recipes/cooking-history/cooking-history.component';
import { CookingStatsComponent } from './components/recipes/cooking-stats/cooking-stats.component';
import { MealPlanListComponent } from './components/meal-plans/meal-plan-list/meal-plan-list.component';
import { MealPlanFormComponent } from './components/meal-plans/meal-plan-form/meal-plan-form.component';
import { MealPlanDetailComponent } from './components/meal-plans/meal-plan-detail/meal-plan-detail.component';
import { GroceryListComponent } from './components/meal-plans/grocery-list/grocery-list.component';
import { CircleListComponent } from './components/circles/circle-list/circle-list.component';
import { CircleDetailComponent } from './components/circles/circle-detail/circle-detail.component';
import { AcceptInviteComponent } from './components/circles/accept-invite/accept-invite.component';
import { CollectionListComponent } from './components/collections/collection-list/collection-list.component';
import { DietaryProfileComponent } from './components/dietary-profile/dietary-profile.component';
import { EquipmentSetupComponent } from './components/equipment-setup/equipment-setup.component';
import { IngredientSearchComponent } from './components/ingredient-search/ingredient-search.component';
import { LoginComponent } from './components/login/login.component';
import { RequestResetComponent } from './components/login/request-reset.component';
import { ResetPasswordComponent } from './components/login/reset-password.component';
import { SharedRecipeComponent } from './components/recipes/shared-recipe/shared-recipe.component';
import { SettingsComponent } from './components/settings/settings.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/recipes', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'reset-password', component: RequestResetComponent },
  { path: 'update-password', component: ResetPasswordComponent },
  { path: 'share/:token', component: SharedRecipeComponent },
  { path: 'join/:token', component: AcceptInviteComponent, canActivate: [authGuard] },
  { path: 'discover', component: DiscoverComponent, canActivate: [authGuard] },
  { path: 'recipes', component: RecipeListComponent, canActivate: [authGuard] },
  { path: 'recipes/new', component: RecipeFormComponent, canActivate: [authGuard] },
  { path: 'recipes/generate', component: RecipeGeneratorComponent, canActivate: [authGuard] },
  { path: 'recipes/mix', component: RecipeMixerComponent, canActivate: [authGuard] },
  { path: 'recipes/:id', component: RecipeDetailComponent, canActivate: [authGuard] },
  { path: 'recipes/:id/edit', component: RecipeFormComponent, canActivate: [authGuard] },
  { path: 'recipes/:id/cook', component: CookingModeComponent, canActivate: [authGuard] },
  { path: 'cooking-history', component: CookingHistoryComponent, canActivate: [authGuard] },
  { path: 'cooking-stats', component: CookingStatsComponent, canActivate: [authGuard] },
  { path: 'circles', component: CircleListComponent, canActivate: [authGuard] },
  { path: 'circles/:id', component: CircleDetailComponent, canActivate: [authGuard] },
  { path: 'collections', component: CollectionListComponent, canActivate: [authGuard] },
  { path: 'dietary-profiles', component: DietaryProfileComponent, canActivate: [authGuard] },
  { path: 'equipment', component: EquipmentSetupComponent, canActivate: [authGuard] },
  { path: 'ingredient-search', component: IngredientSearchComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'meal-plans', component: MealPlanListComponent, canActivate: [authGuard] },
  { path: 'meal-plans/new', component: MealPlanFormComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id', component: MealPlanDetailComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id/edit', component: MealPlanFormComponent, canActivate: [authGuard] },
  { path: 'meal-plans/:id/grocery-list', component: GroceryListComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/recipes' }
];
