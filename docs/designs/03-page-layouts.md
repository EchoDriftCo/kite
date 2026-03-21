# RecipeVault Design System — Page Layouts

**Version:** 2.0 (UI Redesign)  
**Status:** Approved by Vega  
**Date:** 2026-03-21

---

## General Layout Principles

**Mobile-First:** All layouts start with mobile (< 600px), then scale up.

**Responsive Breakpoints:**
- **Mobile:** 0-599px (single column, full-width)
- **Tablet:** 600-959px (two-column where practical)
- **Desktop:** 960px+ (multi-column, side nav visible)

**Page Structure:**
1. **Toolbar** (fixed top, 56px mobile / 64px desktop)
2. **Side Nav** (drawer on mobile, persistent on desktop 960px+)
3. **Main Content** (scrollable, padding 16px mobile / 24px tablet / 32px desktop)
4. **FAB** (fixed bottom-right, 16px mobile / 24px desktop from edges)

**Max Content Width:** 1440px (center-aligned with side padding beyond this)

**Scroll Behavior:** Smooth scrolling, toolbar stays fixed, content scrolls beneath.

---

## 1. Login Page

**Route:** `/login`

**Purpose:** User authentication (Supabase email/password)

**Layout:**

```
┌─────────────────────────────────────┐
│         RecipeVault Logo            │  ← Center-aligned, large logo/title
│                                     │
│  ┌───────────────────────────────┐ │
│  │  Login Card                   │ │
│  │                               │ │
│  │  Email                        │ │  ← Text input
│  │  ┌─────────────────────────┐ │ │
│  │  └─────────────────────────┘ │ │
│  │                               │ │
│  │  Password                     │ │  ← Password input
│  │  ┌─────────────────────────┐ │ │
│  │  └─────────────────────────┘ │ │
│  │                               │ │
│  │  [Forgot Password?]           │ │  ← Text button, right-aligned
│  │                               │ │
│  │  [ LOGIN ]                    │ │  ← Primary button, full-width
│  │                               │ │
│  │  Don't have an account?       │ │
│  │  [Sign Up]                    │ │  ← Text button
│  └───────────────────────────────┘ │
└─────────────────────────────────────┘
```

**Specs:**
- **Card:** Max-width 400px, center-aligned (vertical + horizontal), 24px padding
- **Background:** `$background-secondary` (full-page background), card on `$surface-default`
- **Logo/Title:** 500 32px/40px, `$text-primary`, margin-bottom 32px
- **Form fields:** Stack vertically, 16px margin-bottom
- **Login button:** Full-width, 48px height (touch-friendly)
- **Error message:** Below login button, `$error` color, 400 12px/16px

**States:**
- **Loading:** Disable button, show spinner inside button label ("Logging in...")
- **Error:** Display error message below button (e.g., "Invalid credentials")
- **Success:** Redirect to `/recipes` (dashboard)

**Accessibility:**
- **Focus order:** Email → Password → Forgot Password → Login → Sign Up
- **Screen reader:** "Login form", field labels, error announcements

**Angular Structure:**
```html
<div class="login-page">
  <div class="login-container">
    <h1 class="logo">RecipeVault</h1>
    <mat-card class="login-card">
      <mat-card-content>
        <form [formGroup]="loginForm" (ngSubmit)="login()">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
            <mat-error *ngIf="loginForm.get('email')?.hasError('required')">Email is required</mat-error>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
            <mat-error *ngIf="loginForm.get('password')?.hasError('required')">Password is required</mat-error>
          </mat-form-field>
          
          <div class="forgot-password">
            <button mat-button type="button" (click)="forgotPassword()">Forgot Password?</button>
          </div>
          
          <button mat-raised-button color="primary" type="submit" [disabled]="loading">
            {{ loading ? 'Logging in...' : 'Login' }}
          </button>
          
          <p class="signup-prompt">
            Don't have an account? <button mat-button type="button" (click)="goToSignup()">Sign Up</button>
          </p>
        </form>
      </mat-card-content>
    </mat-card>
  </div>
</div>
```

---

## 2. Dashboard / Recipe List

**Route:** `/recipes`

**Purpose:** View all user's recipes (owned + public if filtered), search, filter, create new

**Layout (Desktop 960px+):**

```
┌──────────────────────────────────────────────────────────┐
│  [≡] RecipeVault        [🔍] [👤]                       │  ← Toolbar (fixed)
├────────┬─────────────────────────────────────────────────┤
│        │  My Recipes                    [+]              │  ← Page title + FAB
│        │                                                  │
│  Nav   │  [Search recipes...]            [Filter ▾]      │  ← Search bar + filter dropdown
│  Menu  │                                                  │
│        │  ┌────────┐ ┌────────┐ ┌────────┐              │
│ • Recipes│  │ Image  │ │ Image  │ │ Image  │              │  ← Recipe cards (grid)
│ • Meal   │  │ Title  │ │ Title  │ │ Title  │              │     3 columns desktop
│   Plans  │  │ 45 min │ │ 30 min │ │ 60 min │              │
│ • Settings│ └────────┘ └────────┘ └────────┘              │
│          │                                                  │
│          │  ┌────────┐ ┌────────┐ ┌────────┐              │
│          │  │ Image  │ │ Image  │ │ Image  │              │
│          │  │ Title  │ │ Title  │ │ Title  │              │
│          │  │ 20 min │ │ 50 min │ │ 35 min │              │
│          │  └────────┘ └────────┘ └────────┘              │
└──────────┴─────────────────────────────────────────────────┘
```

**Layout (Mobile < 600px):**

```
┌─────────────────────────────────┐
│ [≡] RecipeVault       [🔍] [👤]│  ← Toolbar (fixed)
├─────────────────────────────────┤
│  My Recipes                     │  ← Page title
│                                 │
│  [Search recipes...]   [Filter]│  ← Search + filter
│                                 │
│  ┌───────────────────────────┐ │  ← Recipe card (full-width)
│  │ Image                     │ │     Single column mobile
│  │ Title                     │ │
│  │ 45 min · 4 servings       │ │
│  │ [Dessert] [Holiday]       │ │  ← Tags
│  └───────────────────────────┘ │
│                                 │
│  ┌───────────────────────────┐ │
│  │ Image                     │ │
│  │ Title                     │ │
│  │ 30 min · 2 servings       │ │
│  └───────────────────────────┘ │
│                                 │
│                        [+]      │  ← FAB (fixed bottom-right)
└─────────────────────────────────┘
```

**Specs:**
- **Grid:** 1 column (mobile), 2 columns (tablet 600-959px), 3 columns (desktop 960px+)
- **Card gap:** 16px (mobile), 24px (desktop)
- **Card width:** 100% (mobile), ~320px (desktop in grid)
- **Search bar:** Full-width (mobile), max-width 600px (desktop), 48px height
- **Filter dropdown:** Icon button (mobile), outlined button with label (desktop)
- **Empty state:** Show when no recipes (see `02-components.md` for empty state spec)
- **Loading state:** Skeleton loaders (3 cards on desktop, 2 on mobile)

**Search Behavior:**
- **Debounced input:** 300ms delay before triggering search API
- **Live results:** Update grid as user types
- **Clear button:** X icon inside search field (right side) when text exists

**Filter Options:**
- **My Recipes** (default)
- **Public Recipes** (browse community recipes)
- **Tags** (multi-select dropdown, if tags exist)
- **Dietary Flags** (future phase)

**Angular Structure:**
```html
<div class="dashboard-page">
  <div class="page-header">
    <h1>My Recipes</h1>
  </div>
  
  <div class="search-bar">
    <mat-form-field appearance="outline">
      <mat-label>Search recipes</mat-label>
      <input matInput [(ngModel)]="searchQuery" (ngModelChange)="search()" />
      <mat-icon matSuffix>search</mat-icon>
      <button mat-icon-button matSuffix *ngIf="searchQuery" (click)="clearSearch()">
        <mat-icon>close</mat-icon>
      </button>
    </mat-form-field>
    
    <button mat-stroked-button [matMenuTriggerFor]="filterMenu">
      <mat-icon>filter_list</mat-icon>
      Filter
    </button>
  </div>
  
  <div class="recipe-grid" *ngIf="recipes.length > 0">
    <mat-card *ngFor="let recipe of recipes" class="recipe-card" (click)="viewRecipe(recipe.id)">
      <img mat-card-image [src]="recipe.imageUrl || 'assets/placeholder.png'" [alt]="recipe.title" />
      <mat-card-header>
        <mat-card-title>{{ recipe.title }}</mat-card-title>
        <mat-card-subtitle>{{ recipe.totalTimeMinutes }} min · {{ recipe.yield }} servings</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content *ngIf="recipe.tags.length > 0">
        <div class="tags">
          <mat-chip *ngFor="let tag of recipe.tags">{{ tag.name }}</mat-chip>
        </div>
      </mat-card-content>
    </mat-card>
  </div>
  
  <div class="empty-state" *ngIf="recipes.length === 0 && !loading">
    <mat-icon>restaurant</mat-icon>
    <h3>No Recipes Yet</h3>
    <p>Start building your recipe collection by adding your first recipe.</p>
    <button mat-raised-button color="primary" (click)="createRecipe()">Add Recipe</button>
  </div>
  
  <button mat-fab color="primary" class="fab" (click)="createRecipe()" aria-label="Add recipe">
    <mat-icon>add</mat-icon>
  </button>
</div>
```

---

## 3. Recipe Detail

**Route:** `/recipes/:id`

**Purpose:** View full recipe (title, image, ingredients, instructions), edit, delete, share

**Layout (Desktop):**

```
┌──────────────────────────────────────────────────────────┐
│  [←] Recipe Name                [Edit] [Delete] [Share]  │  ← Toolbar (back + actions)
├────────┬─────────────────────────────────────────────────┤
│        │  ┌─────────────────────────────────────────┐    │
│  Nav   │  │                                         │    │
│  Menu  │  │        Recipe Image (16:9)              │    │  ← Hero image
│        │  │                                         │    │
│        │  └─────────────────────────────────────────┘    │
│        │                                                  │
│        │  Grandma's Apple Pie                            │  ← Title (h1)
│        │  45 min · 8 servings                            │  ← Metadata
│        │  [Dessert] [Holiday] [Vegetarian]               │  ← Tags
│        │                                                  │
│        │  ─────────────────────────────────────────      │  ← Divider
│        │                                                  │
│        │  Ingredients              Instructions          │  ← Two-column layout
│        │  • 2 cups flour           1. Preheat oven...    │
│        │  • 1 cup sugar            2. Mix dry...         │
│        │  • 3 apples               3. Roll dough...      │
│        │  • ...                    4. Bake for...        │
│        │                                                  │
└────────┴─────────────────────────────────────────────────┘
```

**Layout (Mobile):**

```
┌─────────────────────────────────┐
│ [←] Recipe Name       [⋮]       │  ← Toolbar (back + overflow menu)
├─────────────────────────────────┤
│ ┌─────────────────────────────┐ │
│ │                             │ │
│ │   Recipe Image (16:9)       │ │  ← Hero image
│ │                             │ │
│ └─────────────────────────────┘ │
│                                 │
│ Grandma's Apple Pie             │  ← Title (h2, smaller on mobile)
│ 45 min · 8 servings             │  ← Metadata
│ [Dessert] [Holiday]             │  ← Tags (scrollable horizontal)
│                                 │
│ ────────────────────────────    │  ← Divider
│                                 │
│ Ingredients                     │  ← Single column (stack)
│ • 2 cups flour                  │
│ • 1 cup sugar                   │
│ • 3 apples, sliced              │
│ • ...                           │
│                                 │
│ Instructions                    │
│ 1. Preheat oven to 375°F.       │
│ 2. Mix dry ingredients...       │
│ 3. Roll dough into circle...    │
│ 4. Bake for 45 minutes.         │
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Hero image:** Full-width, 16:9 aspect ratio, max-height 400px, `object-fit: cover`
- **Title:** 500 32px/40px (desktop), 500 24px/32px (mobile), margin-top 24px
- **Metadata:** 400 14px/20px, `$text-hint`, margin-top 8px
- **Tags:** Horizontal scrollable chips (mobile), wrapped (desktop), margin-top 12px
- **Divider:** 1px solid `$divider`, margin-top/bottom 24px
- **Two-column layout (desktop 960px+):** Ingredients 40% width, Instructions 60% width, 32px gap
- **Single-column layout (mobile):** Stack Ingredients above Instructions, 24px gap
- **Ingredient list:** Unordered list (`<ul>`), 400 14px/20px, 8px line spacing
- **Instruction list:** Ordered list (`<ol>`), 400 14px/20px, 12px line spacing, bold step numbers

**Actions:**
- **Edit:** Navigate to `/recipes/:id/edit` (inline editing future phase)
- **Delete:** Open confirmation dialog, on confirm → delete recipe → redirect to `/recipes`
- **Share:** Open share dialog with options (Copy Link, Make Public toggle)
- **Overflow menu (mobile):** Edit, Delete, Share, Print

**Accessibility:**
- **Back button:** "Navigate back to recipe list"
- **Action buttons:** Clear aria-labels ("Edit recipe", "Delete recipe", "Share recipe")
- **Image alt text:** Recipe title

**Angular Structure:**
```html
<div class="recipe-detail-page">
  <mat-toolbar>
    <button mat-icon-button (click)="goBack()" aria-label="Go back">
      <mat-icon>arrow_back</mat-icon>
    </button>
    <span class="toolbar-title">{{ recipe.title }}</span>
    <span class="spacer"></span>
    <button mat-icon-button (click)="editRecipe()" aria-label="Edit recipe">
      <mat-icon>edit</mat-icon>
    </button>
    <button mat-icon-button (click)="deleteRecipe()" aria-label="Delete recipe">
      <mat-icon>delete</mat-icon>
    </button>
    <button mat-icon-button (click)="shareRecipe()" aria-label="Share recipe">
      <mat-icon>share</mat-icon>
    </button>
  </mat-toolbar>
  
  <div class="recipe-content">
    <img *ngIf="recipe.imageUrl" [src]="recipe.imageUrl" [alt]="recipe.title" class="hero-image" />
    
    <div class="recipe-header">
      <h1>{{ recipe.title }}</h1>
      <p class="metadata">{{ recipe.totalTimeMinutes }} min · {{ recipe.yield }} servings</p>
      <div class="tags" *ngIf="recipe.tags.length > 0">
        <mat-chip *ngFor="let tag of recipe.tags">{{ tag.name }}</mat-chip>
      </div>
    </div>
    
    <mat-divider></mat-divider>
    
    <div class="recipe-body">
      <section class="ingredients">
        <h2>Ingredients</h2>
        <ul>
          <li *ngFor="let ing of recipe.ingredients">
            {{ ing.quantity }} {{ ing.unit }} {{ ing.item }}
            <span *ngIf="ing.preparation">, {{ ing.preparation }}</span>
          </li>
        </ul>
      </section>
      
      <section class="instructions">
        <h2>Instructions</h2>
        <ol>
          <li *ngFor="let step of recipe.instructions">{{ step.instruction }}</li>
        </ol>
      </section>
    </div>
  </div>
</div>
```

---

## 4. Recipe Edit / Create

**Route:** `/recipes/new` (create), `/recipes/:id/edit` (edit)

**Purpose:** Create new recipe or edit existing (manual entry or from AI parse)

**Layout (Desktop):**

```
┌──────────────────────────────────────────────────────────┐
│  [←] New Recipe                      [Save] [Cancel]     │  ← Toolbar
├────────┬─────────────────────────────────────────────────┤
│        │  Recipe Details                                 │
│  Nav   │                                                  │
│  Menu  │  Title                                           │
│        │  ┌─────────────────────────────────────────┐    │
│        │  └─────────────────────────────────────────┘    │
│        │                                                  │
│        │  Image URL (optional)                           │
│        │  ┌─────────────────────────────────────────┐    │
│        │  └─────────────────────────────────────────┘    │
│        │                                                  │
│        │  Prep Time (min)    Cook Time (min)  Servings   │
│        │  ┌────────┐         ┌────────┐       ┌───────┐ │
│        │  └────────┘         └────────┘       └───────┘ │
│        │                                                  │
│        │  ─────────────────────────────────────────      │
│        │                                                  │
│        │  Ingredients                                    │
│        │  ┌───────────────────────────────────────────┐ │
│        │  │ Qty   Unit      Item         Prep         │ │
│        │  │ [2] [cup] [all-purpose flour] [sifted]    │ │  ← Ingredient row
│        │  │ [1] [cup] [sugar]             []          │ │
│        │  │ ...                                       │ │
│        │  └───────────────────────────────────────────┘ │
│        │  [+ Add Ingredient]                             │
│        │                                                  │
│        │  ─────────────────────────────────────────      │
│        │                                                  │
│        │  Instructions                                   │
│        │  ┌───────────────────────────────────────────┐ │
│        │  │ 1. [Preheat oven to 375°F.]              │ │  ← Step textarea
│        │  │ 2. [Mix dry ingredients...]              │ │
│        │  │ ...                                       │ │
│        │  └───────────────────────────────────────────┘ │
│        │  [+ Add Step]                                   │
│        │                                                  │
└────────┴─────────────────────────────────────────────────┘
```

**Layout (Mobile):**

```
┌─────────────────────────────────┐
│ [←] New Recipe        [✓]       │  ← Toolbar (back + save icon)
├─────────────────────────────────┤
│ Title                           │
│ ┌─────────────────────────────┐ │
│ └─────────────────────────────┘ │
│                                 │
│ Image URL (optional)            │
│ ┌─────────────────────────────┐ │
│ └─────────────────────────────┘ │
│                                 │
│ Prep (min)  Cook (min)  Servings│
│ ┌────┐      ┌────┐      ┌────┐ │  ← Side-by-side small inputs
│ └────┘      └────┘      └────┘ │
│                                 │
│ ───────────────────────────────│
│                                 │
│ Ingredients                     │
│ ┌─────────────────────────────┐ │
│ │ Qty [2]  Unit [cup]         │ │  ← Stacked inputs per ingredient
│ │ Item [flour]                │ │
│ │ Prep [sifted]  [X Remove]   │ │
│ └─────────────────────────────┘ │
│ [+ Add Ingredient]              │
│                                 │
│ ───────────────────────────────│
│                                 │
│ Instructions                    │
│ ┌─────────────────────────────┐ │
│ │ 1. [Step text...]           │ │  ← Textarea per step
│ │    [X Remove]               │ │
│ └─────────────────────────────┘ │
│ [+ Add Step]                    │
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Form layout:** Single column (mobile), wider on desktop (max-width 800px, center-aligned)
- **Title input:** Full-width, required
- **Image URL input:** Full-width, optional, with image preview below (if valid URL)
- **Time inputs:** Number inputs, 100px width each, side-by-side with labels above
- **Ingredient rows:** 
  - Desktop: Table-like layout (Qty 80px, Unit 120px, Item flex-grow, Prep 150px, Remove button)
  - Mobile: Stacked (Qty + Unit on one row, Item + Prep on second row, Remove button inline)
- **Instruction steps:** Textarea per step, auto-resize, numbered automatically, Remove button per step
- **Add buttons:** Text button with `+` icon, left-aligned below list
- **Save button:** Primary button (toolbar on mobile, inline on desktop)
- **Cancel button:** Text button (navigate back, discard changes with confirmation if dirty form)

**Validation:**
- **Title:** Required
- **Servings:** Required, min 1
- **At least one ingredient:** Required
- **At least one instruction step:** Required
- **Show errors inline:** Below fields, `$error` color

**Angular Structure:**
```html
<div class="recipe-edit-page">
  <mat-toolbar>
    <button mat-icon-button (click)="goBack()" aria-label="Go back">
      <mat-icon>arrow_back</mat-icon>
    </button>
    <span class="toolbar-title">{{ isEditMode ? 'Edit Recipe' : 'New Recipe' }}</span>
    <span class="spacer"></span>
    <button mat-raised-button color="primary" (click)="save()" [disabled]="!recipeForm.valid">
      Save
    </button>
  </mat-toolbar>
  
  <div class="recipe-edit-content">
    <form [formGroup]="recipeForm">
      <mat-form-field appearance="outline">
        <mat-label>Recipe Title</mat-label>
        <input matInput formControlName="title" required />
        <mat-error *ngIf="recipeForm.get('title')?.hasError('required')">Title is required</mat-error>
      </mat-form-field>
      
      <mat-form-field appearance="outline">
        <mat-label>Image URL (optional)</mat-label>
        <input matInput formControlName="imageUrl" />
      </mat-form-field>
      
      <div class="time-inputs">
        <mat-form-field appearance="outline">
          <mat-label>Prep Time (min)</mat-label>
          <input matInput type="number" formControlName="prepTimeMinutes" min="0" />
        </mat-form-field>
        
        <mat-form-field appearance="outline">
          <mat-label>Cook Time (min)</mat-label>
          <input matInput type="number" formControlName="cookTimeMinutes" min="0" />
        </mat-form-field>
        
        <mat-form-field appearance="outline">
          <mat-label>Servings</mat-label>
          <input matInput type="number" formControlName="yield" min="1" required />
        </mat-form-field>
      </div>
      
      <mat-divider></mat-divider>
      
      <h2>Ingredients</h2>
      <div formArrayName="ingredients">
        <div *ngFor="let ing of ingredients.controls; let i = index" [formGroupName]="i" class="ingredient-row">
          <mat-form-field appearance="outline">
            <mat-label>Qty</mat-label>
            <input matInput type="number" formControlName="quantity" step="0.1" />
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Unit</mat-label>
            <input matInput formControlName="unit" />
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Item</mat-label>
            <input matInput formControlName="item" required />
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Prep</mat-label>
            <input matInput formControlName="preparation" />
          </mat-form-field>
          
          <button mat-icon-button type="button" (click)="removeIngredient(i)" aria-label="Remove ingredient">
            <mat-icon>delete</mat-icon>
          </button>
        </div>
      </div>
      <button mat-button type="button" (click)="addIngredient()">
        <mat-icon>add</mat-icon> Add Ingredient
      </button>
      
      <mat-divider></mat-divider>
      
      <h2>Instructions</h2>
      <div formArrayName="instructions">
        <div *ngFor="let step of instructions.controls; let i = index" [formGroupName]="i" class="instruction-row">
          <span class="step-number">{{ i + 1 }}.</span>
          <mat-form-field appearance="outline">
            <mat-label>Step</mat-label>
            <textarea matInput formControlName="instruction" rows="3" required></textarea>
          </mat-form-field>
          <button mat-icon-button type="button" (click)="removeInstruction(i)" aria-label="Remove step">
            <mat-icon>delete</mat-icon>
          </button>
        </div>
      </div>
      <button mat-button type="button" (click)="addInstruction()">
        <mat-icon>add</mat-icon> Add Step
      </button>
    </form>
  </div>
</div>
```

---

## 5. Import Recipe (AI Parse)

**Route:** `/recipes/import` or modal from `/recipes`

**Purpose:** Upload recipe image, AI parses, user reviews/edits, saves

**Layout (Modal):**

```
┌─────────────────────────────────────────────┐
│  Import Recipe                         [X]  │  ← Dialog header
├─────────────────────────────────────────────┤
│                                             │
│  Upload a photo of a recipe card, page,    │  ← Instructions
│  or screenshot. AI will extract the         │
│  recipe details.                            │
│                                             │
│  ┌───────────────────────────────────────┐ │
│  │                                       │ │
│  │        [📷 Upload Image]              │ │  ← Upload button / drop zone
│  │                                       │ │
│  │    or drag & drop here                │ │
│  │                                       │ │
│  └───────────────────────────────────────┘ │
│                                             │
│  ──── After upload, show preview: ────     │
│                                             │
│  ┌──────────┐  Recipe detected!            │  ← Thumbnail + status
│  │ [thumb]  │  Parsing...                  │
│  └──────────┘                              │
│                                             │
│  [Cancel]                      [Continue]  │  ← Actions
└─────────────────────────────────────────────┘
```

**Flow:**
1. **Upload step:** User uploads image, show progress bar, API parses
2. **Parsing step:** Show spinner + "Parsing recipe..." message (3-5 seconds)
3. **Review step:** Navigate to `/recipes/new` with pre-filled form data (user can edit before saving)
4. **Warnings:** If AI confidence low, show warning banner ("Please review ingredients carefully")

**Specs:**
- **Upload area:** Dashed border, 200px height, center-aligned icon + text
- **Drag & drop:** Highlight border on drag-over (`$accent-teal` border, 2px solid)
- **Accepted formats:** JPEG, PNG, HEIC (client-side validation)
- **Max file size:** 10MB (client-side validation, server enforces)
- **Progress bar:** Indeterminate during upload, determinate during parse (if progress available)
- **Error handling:** Show error snackbar if parse fails, allow retry

**Angular Structure:**
```html
<mat-dialog-content class="import-dialog">
  <h2>Import Recipe</h2>
  <p>Upload a photo of a recipe card, page, or screenshot. AI will extract the recipe details.</p>
  
  <div class="upload-zone" 
       (drop)="onDrop($event)" 
       (dragover)="onDragOver($event)" 
       (dragleave)="onDragLeave($event)"
       [class.drag-over]="isDragOver">
    <input #fileInput type="file" accept="image/*" (change)="onFileSelected($event)" hidden />
    <button mat-raised-button color="accent" (click)="fileInput.click()">
      <mat-icon>upload</mat-icon> Upload Image
    </button>
    <p>or drag & drop here</p>
  </div>
  
  <div *ngIf="uploading" class="upload-progress">
    <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
    <p>Parsing recipe...</p>
  </div>
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button [mat-dialog-close]="false">Cancel</button>
  <button mat-raised-button color="primary" [mat-dialog-close]="parsedRecipe" [disabled]="!parsedRecipe">
    Continue
  </button>
</mat-dialog-actions>
```

---

## 6. Meal Plan Calendar

**Route:** `/meal-plans`

**Purpose:** View weekly meal plan, add recipes to days/slots, generate grocery list

**Layout (Desktop):**

```
┌──────────────────────────────────────────────────────────┐
│  [≡] Meal Plans                    [+ New Plan]          │  ← Toolbar
├────────┬─────────────────────────────────────────────────┤
│        │  This Week's Plan          [< March 2026 >]     │  ← Week selector
│  Nav   │                                                  │
│  Menu  │  ┌──────┬──────┬──────┬──────┬──────┬──────┐   │  ← Calendar grid
│        │  │ Mon  │ Tue  │ Wed  │ Thu  │ Fri  │ Sat  │   │     7 columns (Sun-Sat)
│        │  ├──────┼──────┼──────┼──────┼──────┼──────┤   │
│        │  │ [B]  │ [B]  │ [B]  │ [B]  │ [B]  │ [B]  │   │  ← Breakfast row
│        │  │Oats  │Toast │...   │...   │...   │...   │   │
│        │  ├──────┼──────┼──────┼──────┼──────┼──────┤   │
│        │  │ [L]  │ [L]  │ [L]  │ [L]  │ [L]  │ [L]  │   │  ← Lunch row
│        │  │Salad │Soup  │...   │...   │...   │...   │   │
│        │  ├──────┼──────┼──────┼──────┼──────┼──────┤   │
│        │  │ [D]  │ [D]  │ [D]  │ [D]  │ [D]  │ [D]  │   │  ← Dinner row
│        │  │Pasta │Steak │...   │...   │...   │...   │   │
│        │  └──────┴──────┴──────┴──────┴──────┴──────┘   │
│        │                                                  │
│        │  [📋 Generate Grocery List]                     │  ← Action button
│        │                                                  │
└────────┴─────────────────────────────────────────────────┘
```

**Layout (Mobile):**

```
┌─────────────────────────────────┐
│ [≡] Meal Plans          [+]     │  ← Toolbar
├─────────────────────────────────┤
│ This Week's Plan                │
│ [< March 14-20, 2026 >]         │  ← Week selector (prev/next arrows)
│                                 │
│ Monday, March 14                │  ← Day header (list view)
│ ┌─────────────────────────────┐│
│ │ Breakfast: Oatmeal          ││  ← Meal slot
│ │ 30 min · 2 servings         ││
│ └─────────────────────────────┘│
│ ┌─────────────────────────────┐│
│ │ Lunch: Caesar Salad         ││
│ │ 15 min · 2 servings         ││
│ └─────────────────────────────┘│
│ ┌─────────────────────────────┐│
│ │ Dinner: Spaghetti           ││
│ │ 45 min · 4 servings         ││
│ └─────────────────────────────┘│
│                                 │
│ Tuesday, March 15               │
│ ...                             │
│                                 │
│ [📋 Generate Grocery List]      │  ← Fixed bottom button
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Calendar grid (desktop):** 7 columns (Sun-Sat), 3 or 4 rows (Breakfast, Lunch, Dinner, optional Snack)
- **Cell:** Min-height 100px, padding 8px, background `$surface-default`, border 1px solid `$divider`
- **Meal slot label:** [B], [L], [D], [S] (overline font, 11px, `$text-hint`)
- **Recipe name:** 400 14px/20px, `$text-primary`, truncate with ellipsis if too long
- **Add recipe button:** `+` icon button in empty cell, opens recipe selector dialog
- **Week navigation:** Prev/Next arrow buttons, display week range (e.g., "March 14-20, 2026")
- **Grocery list button:** Primary button, full-width (mobile), right-aligned (desktop)

**Interactions:**
- **Click cell:** Open recipe selector dialog (search/filter recipes to add to slot)
- **Click recipe in cell:** Open recipe detail dialog (quick view, option to remove from plan)
- **Generate Grocery List:** Navigate to `/meal-plans/:id/grocery-list` (new page or modal)

**Angular Structure:**
```html
<div class="meal-plan-page">
  <div class="page-header">
    <h1>This Week's Plan</h1>
    <button mat-raised-button color="primary" (click)="createPlan()">
      <mat-icon>add</mat-icon> New Plan
    </button>
  </div>
  
  <div class="week-selector">
    <button mat-icon-button (click)="previousWeek()" aria-label="Previous week">
      <mat-icon>chevron_left</mat-icon>
    </button>
    <span>{{ weekRange }}</span>
    <button mat-icon-button (click)="nextWeek()" aria-label="Next week">
      <mat-icon>chevron_right</mat-icon>
    </button>
  </div>
  
  <div class="meal-calendar">
    <div *ngFor="let day of week" class="day-column">
      <div class="day-header">{{ day.name }}</div>
      
      <div class="meal-slot" *ngFor="let slot of mealSlots">
        <span class="slot-label">{{ slot }}</span>
        <div *ngIf="getMeal(day, slot); let meal" class="meal-card" (click)="viewRecipe(meal.recipeId)">
          <p class="meal-title">{{ meal.recipe.title }}</p>
          <p class="meal-meta">{{ meal.recipe.totalTimeMinutes }} min · {{ meal.servings }} servings</p>
        </div>
        <button *ngIf="!getMeal(day, slot)" mat-icon-button (click)="addMeal(day, slot)" aria-label="Add meal">
          <mat-icon>add</mat-icon>
        </button>
      </div>
    </div>
  </div>
  
  <button mat-raised-button color="primary" (click)="generateGroceryList()">
    <mat-icon>shopping_cart</mat-icon> Generate Grocery List
  </button>
</div>
```

---

## 7. Grocery List

**Route:** `/meal-plans/:id/grocery-list`

**Purpose:** AI-generated consolidated grocery list from meal plan, organized by category

**Layout:**

```
┌─────────────────────────────────┐
│ [←] Grocery List        [Print] │  ← Toolbar
├─────────────────────────────────┤
│ For: This Week's Plan           │  ← Plan name
│ March 14-20, 2026               │
│                                 │
│ ───────────────────────────────│
│                                 │
│ Produce                         │  ← Category header
│ ☐ 3 apples                      │  ← Checkbox + consolidated item
│ ☐ 2 cups spinach                │
│ ☐ 1 onion, diced                │
│                                 │
│ Dairy                           │
│ ☐ 1 cup milk                    │
│ ☐ 8 oz cheddar cheese           │
│                                 │
│ Pantry                          │
│ ☐ 2 cups flour                  │
│ ☐ 1 cup sugar                   │
│                                 │
│ ...                             │
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Category headers:** 500 18px/24px, `$text-primary`, margin-top 24px, margin-bottom 12px
- **List items:** Checkbox + label (400 14px/20px), 8px gap, 4px margin-bottom
- **Checkbox:** Material checkbox, color `$accent-teal`, persistent checked state (local storage or API)
- **Print button:** Text button (toolbar), triggers print-friendly CSS (`@media print`)

**Print Styles:**
- Hide toolbar, nav, checkboxes render as empty boxes `☐` / `☑`
- Black text on white background
- Page breaks avoid splitting categories

**Angular Structure:**
```html
<div class="grocery-list-page">
  <mat-toolbar>
    <button mat-icon-button (click)="goBack()" aria-label="Go back">
      <mat-icon>arrow_back</mat-icon>
    </button>
    <span class="toolbar-title">Grocery List</span>
    <span class="spacer"></span>
    <button mat-button (click)="print()">
      <mat-icon>print</mat-icon> Print
    </button>
  </mat-toolbar>
  
  <div class="grocery-list-content">
    <div class="list-header">
      <p>For: {{ mealPlan.name }}</p>
      <p>{{ mealPlan.startDate | date }} - {{ mealPlan.endDate | date }}</p>
    </div>
    
    <mat-divider></mat-divider>
    
    <div *ngFor="let category of groceryList" class="category-section">
      <h2>{{ category.name }}</h2>
      <mat-checkbox *ngFor="let item of category.items" [(ngModel)]="item.checked">
        {{ item.quantity }} {{ item.unit }} {{ item.name }}
        <span *ngIf="item.preparation">, {{ item.preparation }}</span>
      </mat-checkbox>
    </div>
  </div>
</div>
```

---

## 8. Settings

**Route:** `/settings`

**Purpose:** User account settings, preferences, subscription (future)

**Layout:**

```
┌─────────────────────────────────┐
│ [≡] Settings                    │  ← Toolbar
├─────────────────────────────────┤
│ Account                         │  ← Section header
│ ┌─────────────────────────────┐│
│ │ Email: user@example.com     ││  ← Read-only field
│ │ [Change Password]           ││  ← Button
│ │ [Log Out]                   ││  ← Button
│ └─────────────────────────────┘│
│                                 │
│ Preferences                     │
│ ┌─────────────────────────────┐│
│ │ ☐ Email notifications       ││  ← Toggle
│ │ ☐ Weekly meal plan reminder ││
│ └─────────────────────────────┘│
│                                 │
│ Data                            │
│ ┌─────────────────────────────┐│
│ │ [Export Recipes (JSON)]     ││  ← Button
│ │ [Delete Account]            ││  ← Destructive button
│ └─────────────────────────────┘│
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Section layout:** Content cards, 24px margin-bottom
- **Section headers:** 500 20px/28px, `$text-primary`, margin-bottom 12px
- **Settings cards:** Background `$surface-default`, padding 16px, border-radius 12px
- **Toggle switches:** Material slide-toggle, color `$accent-teal`
- **Destructive actions:** Warn color button, confirmation dialog before action

**Angular Structure:**
```html
<div class="settings-page">
  <h1>Settings</h1>
  
  <section class="settings-section">
    <h2>Account</h2>
    <mat-card>
      <mat-card-content>
        <p><strong>Email:</strong> {{ user.email }}</p>
        <button mat-raised-button color="accent" (click)="changePassword()">Change Password</button>
        <button mat-stroked-button (click)="logout()">Log Out</button>
      </mat-card-content>
    </mat-card>
  </section>
  
  <section class="settings-section">
    <h2>Preferences</h2>
    <mat-card>
      <mat-card-content>
        <mat-slide-toggle [(ngModel)]="preferences.emailNotifications" color="accent">
          Email notifications
        </mat-slide-toggle>
        <mat-slide-toggle [(ngModel)]="preferences.weeklyReminder" color="accent">
          Weekly meal plan reminder
        </mat-slide-toggle>
      </mat-card-content>
    </mat-card>
  </section>
  
  <section class="settings-section">
    <h2>Data</h2>
    <mat-card>
      <mat-card-content>
        <button mat-raised-button color="accent" (click)="exportRecipes()">Export Recipes (JSON)</button>
        <button mat-stroked-button color="warn" (click)="deleteAccount()">Delete Account</button>
      </mat-card-content>
    </mat-card>
  </section>
</div>
```

---

## 9. Cooking Mode (Future Phase)

**Route:** `/recipes/:id/cook`

**Purpose:** Step-by-step guided cooking view, large text, timers, hands-free navigation

**Layout:**

```
┌─────────────────────────────────┐
│ [X] Cooking Mode                │  ← Minimal toolbar (exit only)
├─────────────────────────────────┤
│                                 │
│        Step 2 of 8              │  ← Progress indicator
│                                 │
│  Mix dry ingredients together   │  ← Large instruction text
│  in a large bowl until well     │     (18-24px font, high contrast)
│  combined.                      │
│                                 │
│  ────────────────────────────   │
│                                 │
│  Ingredients for this step:     │  ← Contextual ingredients
│  • 2 cups flour                 │     (if AI can parse step context)
│  • 1 cup sugar                  │
│                                 │
│                                 │
│  [< Previous]      [Next >]     │  ← Navigation buttons (large, touch-friendly)
│                                 │
└─────────────────────────────────┘
```

**Specs:**
- **Full-screen mode:** Hide side nav, minimal toolbar
- **Instruction text:** 500 24px/36px (mobile), 500 32px/48px (desktop), high contrast
- **Progress:** Step X of Y, progress bar at top
- **Navigation buttons:** 56px height, full-width on mobile, inline on desktop
- **Screen wake lock:** Keep screen on while in cooking mode (Web API)
- **Voice control (future):** "Next step", "Previous step" voice commands

---

## Next Steps

Kovacs: Implement these page layouts in Angular, starting with login → dashboard → recipe detail → edit. Use Angular Material components with custom SCSS overrides from `01-foundation.md` and `02-components.md`.

Priority order:
1. Login
2. Dashboard / Recipe List
3. Recipe Detail
4. Recipe Edit/Create
5. Meal Plan Calendar
6. Settings
7. Import (AI parse)
8. Grocery List
9. Cooking Mode (future phase)
