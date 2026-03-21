# RecipeVault Design System — Components

**Version:** 2.0 (UI Redesign)  
**Status:** Approved by Vega  
**Date:** 2026-03-21

---

## Buttons

### Primary Button (Filled)

**Use for:** High-emphasis actions (Save, Create Recipe, Submit, Continue)

**Specs:**
- **Background:** `$primary-amber` (#f59e0b)
- **Text color:** `#0f172a` (dark slate for contrast)
- **Font:** 500 14px/20px, uppercase, letter-spacing 0.05em
- **Padding:** 8px vertical, 24px horizontal
- **Border-radius:** 8px
- **Min-width:** 64px
- **Min-height:** 40px (touch-friendly)
- **Shadow:** none at rest, `$shadow-1` on hover

**States:**
- **Hover:** Background → `$primary-amber-hover` (#fbbf24), shadow → `$shadow-1`, transition 150ms
- **Active/Pressed:** Background → `$primary-amber-active` (#d97706), scale(0.98)
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Background → `$surface-hover` (#475569), text → `$text-disabled` (#94a3b8), cursor not-allowed

**Angular Material:**
```html
<button mat-raised-button color="primary">Save Recipe</button>
```

**Custom SCSS:**
```scss
.button-primary {
  background-color: var(--color-primary);
  color: #0f172a;
  font: var(--font-button);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding: 8px 24px;
  border-radius: var(--radius-md);
  min-width: 64px;
  min-height: 40px;
  box-shadow: var(--shadow-0);
  transition: all var(--transition-fast);
  
  &:hover:not(:disabled) {
    background-color: var(--color-primary-hover);
    box-shadow: var(--shadow-1);
  }
  
  &:active:not(:disabled) {
    background-color: var(--color-primary-active);
    transform: scale(0.98);
  }
  
  &:focus-visible {
    outline: 2px solid var(--color-accent);
    outline-offset: 2px;
  }
  
  &:disabled {
    background-color: var(--color-surface-hover);
    color: var(--color-text-disabled);
    cursor: not-allowed;
  }
}
```

---

### Secondary Button (Outlined)

**Use for:** Medium-emphasis actions (Cancel, Back, Edit, Filter)

**Specs:**
- **Background:** Transparent
- **Border:** 1px solid `$accent-teal` (#14b8a6)
- **Text color:** `$accent-teal` (#14b8a6)
- **Font:** 500 14px/20px, uppercase, letter-spacing 0.05em
- **Padding:** 7px vertical, 23px horizontal (accounts for 1px border)
- **Border-radius:** 8px
- **Min-width:** 64px
- **Min-height:** 40px

**States:**
- **Hover:** Background → `rgba(20, 184, 166, 0.08)`, border → `$accent-teal-hover` (#2dd4bf)
- **Active/Pressed:** Background → `rgba(20, 184, 166, 0.16)`, border → `$accent-teal-active` (#0d9488)
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Border → `$border-default`, text → `$text-disabled`, cursor not-allowed

**Angular Material:**
```html
<button mat-stroked-button color="accent">Cancel</button>
```

**Custom SCSS:**
```scss
.button-secondary {
  background-color: transparent;
  border: 1px solid var(--color-accent);
  color: var(--color-accent);
  font: var(--font-button);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding: 7px 23px;
  border-radius: var(--radius-md);
  min-width: 64px;
  min-height: 40px;
  transition: all var(--transition-fast);
  
  &:hover:not(:disabled) {
    background-color: rgba(20, 184, 166, 0.08);
    border-color: var(--color-accent-hover);
  }
  
  &:active:not(:disabled) {
    background-color: rgba(20, 184, 166, 0.16);
    border-color: var(--color-accent-active);
  }
  
  &:focus-visible {
    outline: 2px solid var(--color-accent);
    outline-offset: 2px;
  }
  
  &:disabled {
    border-color: var(--color-border-default);
    color: var(--color-text-disabled);
    cursor: not-allowed;
  }
}
```

---

### Text Button (Flat)

**Use for:** Low-emphasis actions (Learn More, View Details, links)

**Specs:**
- **Background:** Transparent
- **Text color:** `$accent-teal` (#14b8a6)
- **Font:** 500 14px/20px, uppercase, letter-spacing 0.05em
- **Padding:** 8px 16px
- **Border-radius:** 8px
- **Min-height:** 36px

**States:**
- **Hover:** Background → `rgba(20, 184, 166, 0.08)`
- **Active/Pressed:** Background → `rgba(20, 184, 166, 0.16)`
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Text → `$text-disabled`, cursor not-allowed

**Angular Material:**
```html
<button mat-button color="accent">Learn More</button>
```

---

### Icon Button

**Use for:** Toolbar actions, FABs, quick actions (Edit, Delete, Share, Menu)

**Specs:**
- **Size:** 40x40px (touch-friendly)
- **Icon size:** 24x24px
- **Background:** Transparent at rest
- **Icon color:** `$text-primary` (#f1f5f9)
- **Border-radius:** `$radius-full` (fully rounded)

**States:**
- **Hover:** Background → `rgba(241, 245, 249, 0.08)`
- **Active/Pressed:** Background → `rgba(241, 245, 249, 0.16)`, scale(0.95)
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Icon color → `$text-disabled`, cursor not-allowed

**Angular Material:**
```html
<button mat-icon-button aria-label="Edit recipe">
  <mat-icon>edit</mat-icon>
</button>
```

---

### Floating Action Button (FAB)

**Use for:** Primary screen action (Add Recipe, Create Meal Plan)

**Specs:**
- **Size:** 56x56px (desktop), 48x48px (mobile)
- **Icon size:** 24x24px
- **Background:** `$primary-amber` (#f59e0b)
- **Icon color:** `#0f172a` (dark slate)
- **Border-radius:** `$radius-full`
- **Shadow:** `$shadow-2` at rest, `$shadow-3` on hover
- **Position:** Fixed, bottom-right corner (16px from edge on mobile, 24px on desktop)

**States:**
- **Hover:** Background → `$primary-amber-hover`, shadow → `$shadow-3`, scale(1.05)
- **Active/Pressed:** Background → `$primary-amber-active`, scale(0.95)
- **Focus:** 2px solid `$accent-teal` outline, 2px offset

**Angular Material:**
```html
<button mat-fab color="primary" aria-label="Add recipe">
  <mat-icon>add</mat-icon>
</button>
```

**Positioning CSS:**
```scss
.fab-container {
  position: fixed;
  bottom: 16px;
  right: 16px;
  z-index: 1000;
  
  @include breakpoint-md {
    bottom: 24px;
    right: 24px;
  }
}
```

---

## Form Fields

### Text Input

**Specs:**
- **Background:** `rgba(51, 65, 85, 0.5)` (semi-transparent slate-700)
- **Border:** 1px solid `$border-default` at rest, `$accent-teal` on focus
- **Border-radius:** 8px
- **Padding:** 12px 16px
- **Font:** 400 14px/20px
- **Text color:** `$text-primary`
- **Label color:** `$text-secondary` (floating label above field)
- **Placeholder color:** `$text-hint`
- **Min-height:** 48px (touch-friendly)

**States:**
- **Focus:** Border → 2px solid `$accent-teal`, background → `rgba(51, 65, 85, 0.7)`, label → `$accent-teal`
- **Error:** Border → 2px solid `$error`, label → `$error`, helper text → `$error`
- **Disabled:** Background → `rgba(51, 65, 85, 0.3)`, text → `$text-disabled`, cursor not-allowed
- **Filled (with value):** Label stays floated, border `$border-default`

**Helper Text:**
- **Font:** 400 12px/16px
- **Color:** `$text-hint` (normal), `$error` (error state)
- **Margin-top:** 4px

**Angular Material:**
```html
<mat-form-field appearance="outline">
  <mat-label>Recipe Title</mat-label>
  <input matInput placeholder="Enter recipe name" />
  <mat-hint>Give your recipe a memorable name</mat-hint>
  <mat-error *ngIf="titleControl.hasError('required')">Title is required</mat-error>
</mat-form-field>
```

**Custom SCSS:**
```scss
.mat-mdc-form-field {
  width: 100%;
  
  .mat-mdc-text-field-wrapper {
    background-color: rgba(51, 65, 85, 0.5);
    border: 1px solid var(--color-border-default);
    border-radius: var(--radius-md);
    padding: 12px 16px;
    min-height: 48px;
    transition: all var(--transition-fast);
  }
  
  .mat-mdc-input-element {
    color: var(--color-text-primary);
    font: var(--font-body);
  }
  
  .mat-mdc-form-field-label {
    color: var(--color-text-secondary);
  }
  
  &.mat-focused {
    .mat-mdc-text-field-wrapper {
      border: 2px solid var(--color-accent);
      background-color: rgba(51, 65, 85, 0.7);
    }
    
    .mat-mdc-form-field-label {
      color: var(--color-accent);
    }
  }
  
  &.mat-form-field-invalid {
    .mat-mdc-text-field-wrapper {
      border: 2px solid var(--color-error);
    }
    
    .mat-mdc-form-field-label,
    .mat-mdc-form-field-hint {
      color: var(--color-error);
    }
  }
  
  &.mat-form-field-disabled {
    .mat-mdc-text-field-wrapper {
      background-color: rgba(51, 65, 85, 0.3);
    }
    
    .mat-mdc-input-element {
      color: var(--color-text-disabled);
    }
  }
}

.mat-mdc-form-field-hint,
.mat-mdc-form-field-error {
  font: var(--font-caption);
  margin-top: 4px;
}
```

---

### Textarea

Same as text input, but:
- **Min-height:** 120px
- **Resize:** Vertical only
- **Line-height:** 1.5 (readable for multi-line)

**Angular Material:**
```html
<mat-form-field appearance="outline">
  <mat-label>Instructions</mat-label>
  <textarea matInput rows="6" placeholder="Enter cooking steps..."></textarea>
</mat-form-field>
```

---

### Select Dropdown

**Specs:**
- Same as text input for base styling
- **Dropdown icon:** `arrow_drop_down` (Material icon), 24x24px, right-aligned
- **Dropdown panel:** Background `$surface-default`, shadow `$shadow-3`, border-radius 8px
- **Option hover:** Background `$surface-hover`
- **Option selected:** Background `$accent-teal` with 0.2 opacity, text bold

**Angular Material:**
```html
<mat-form-field appearance="outline">
  <mat-label>Category</mat-label>
  <mat-select placeholder="Choose category">
    <mat-option value="breakfast">Breakfast</mat-option>
    <mat-option value="lunch">Lunch</mat-option>
    <mat-option value="dinner">Dinner</mat-option>
  </mat-select>
</mat-form-field>
```

---

### Checkbox

**Specs:**
- **Size:** 20x20px
- **Border:** 2px solid `$border-default` (unchecked)
- **Background:** Transparent (unchecked), `$accent-teal` (checked)
- **Checkmark color:** `#0f172a` (dark slate)
- **Border-radius:** 4px
- **Label:** 400 14px/20px, 8px margin-left, `$text-primary`

**States:**
- **Hover:** Border → `$accent-teal-hover`
- **Checked:** Background → `$accent-teal`, border → `$accent-teal`, checkmark visible
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Border → `$text-disabled`, background → `rgba(51, 65, 85, 0.3)`, cursor not-allowed

**Angular Material:**
```html
<mat-checkbox color="accent">Include prep time</mat-checkbox>
```

---

### Radio Button

**Specs:**
- **Size:** 20x20px (outer circle)
- **Border:** 2px solid `$border-default` (unselected)
- **Inner circle:** 10x10px, `$accent-teal` (selected only)
- **Label:** 400 14px/20px, 8px margin-left, `$text-primary`

**States:**
- **Hover:** Border → `$accent-teal-hover`
- **Selected:** Border → `$accent-teal`, inner circle visible
- **Focus:** 2px solid `$accent-teal` outline, 2px offset
- **Disabled:** Border → `$text-disabled`, cursor not-allowed

**Angular Material:**
```html
<mat-radio-group color="accent">
  <mat-radio-button value="free">Free Tier</mat-radio-button>
  <mat-radio-button value="premium">Premium Tier</mat-radio-button>
</mat-radio-group>
```

---

### Toggle Switch

**Use for:** Boolean settings (Public/Private, Enable Notifications)

**Specs:**
- **Track width:** 36px
- **Track height:** 20px
- **Thumb size:** 16x16px (circular)
- **Track color (off):** `$surface-hover` (#475569)
- **Track color (on):** `$accent-teal` (#14b8a6)
- **Thumb color:** `#f1f5f9` (always light)
- **Border-radius:** `$radius-full` (track and thumb fully rounded)

**States:**
- **Off:** Thumb positioned left, track color `$surface-hover`
- **On:** Thumb positioned right, track color `$accent-teal`
- **Hover:** Track color slightly brighter (add 10% lightness)
- **Focus:** 2px solid `$accent-teal` outline around track, 2px offset
- **Disabled:** Track color `rgba(51, 65, 85, 0.3)`, thumb opacity 0.5, cursor not-allowed

**Angular Material:**
```html
<mat-slide-toggle color="accent">Make recipe public</mat-slide-toggle>
```

---

## Cards

### Recipe Card (List View)

**Use for:** Recipe grid/list on dashboard

**Specs:**
- **Background:** `$surface-default` (#334155)
- **Border-radius:** 12px
- **Shadow:** `$shadow-1` at rest, `$shadow-2` on hover
- **Padding:** 16px
- **Width:** 100% (mobile), 320px (desktop grid)
- **Image aspect ratio:** 16:9
- **Image position:** Top of card
- **Image border-radius:** 8px (top corners only if no padding, or all corners if inside padding)

**Layout Structure:**
1. **Image** (optional) — 16:9 aspect ratio, 8px border-radius, `object-fit: cover`
2. **Title** — 500 18px/24px, `$text-primary`, margin-top 12px
3. **Metadata** — 400 12px/16px, `$text-hint`, margin-top 4px (e.g., "45 min · 4 servings")
4. **Tags** (optional) — Chip components, margin-top 8px
5. **Actions** — Icon buttons (Edit, Delete, Share), bottom-right or inline at bottom

**States:**
- **Hover:** Shadow → `$shadow-2`, transform → `translateY(-4px)`, transition 250ms
- **Active/Pressed:** Shadow → `$shadow-1`, transform → `translateY(0)`
- **Focus:** 2px solid `$accent-teal` outline, 2px offset

**Example HTML:**
```html
<mat-card class="recipe-card">
  <img mat-card-image src="..." alt="Recipe photo" />
  <mat-card-header>
    <mat-card-title>Grandma's Apple Pie</mat-card-title>
    <mat-card-subtitle>45 min · 8 servings</mat-card-subtitle>
  </mat-card-header>
  <mat-card-content>
    <div class="tags">
      <mat-chip>Dessert</mat-chip>
      <mat-chip>Holiday</mat-chip>
    </div>
  </mat-card-content>
  <mat-card-actions align="end">
    <button mat-icon-button aria-label="Edit"><mat-icon>edit</mat-icon></button>
    <button mat-icon-button aria-label="Delete"><mat-icon>delete</mat-icon></button>
  </mat-card-actions>
</mat-card>
```

**Custom SCSS:**
```scss
.recipe-card {
  background-color: var(--color-surface-default);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-1);
  padding: 16px;
  transition: all var(--transition-normal);
  cursor: pointer;
  
  img {
    border-radius: var(--radius-md);
    width: 100%;
    height: auto;
    aspect-ratio: 16 / 9;
    object-fit: cover;
  }
  
  .mat-mdc-card-title {
    font: var(--font-h4);
    color: var(--color-text-primary);
    margin-top: 12px;
  }
  
  .mat-mdc-card-subtitle {
    font: var(--font-caption);
    color: var(--color-text-hint);
    margin-top: 4px;
  }
  
  .tags {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 8px;
  }
  
  &:hover {
    box-shadow: var(--shadow-2);
    transform: translateY(-4px);
  }
  
  &:active {
    transform: translateY(0);
  }
  
  &:focus-visible {
    outline: 2px solid var(--color-accent);
    outline-offset: 2px;
  }
}
```

---

### Content Card (Generic)

**Use for:** Settings sections, informational panels, dialogs

**Specs:**
- **Background:** `$surface-default` (#334155)
- **Border-radius:** 12px
- **Shadow:** `$shadow-1`
- **Padding:** 16px (mobile), 24px (desktop)
- **Width:** 100% (constrained by container)

**Layout Structure:**
1. **Header** (optional) — Title (500 20px/28px) + optional actions
2. **Content** — Body text (400 14px/20px), forms, lists, etc.
3. **Footer** (optional) — Actions, buttons

**Example HTML:**
```html
<mat-card class="content-card">
  <mat-card-header>
    <mat-card-title>Account Settings</mat-card-title>
  </mat-card-header>
  <mat-card-content>
    <p>Manage your profile and preferences.</p>
  </mat-card-content>
  <mat-card-actions align="end">
    <button mat-button color="accent">Cancel</button>
    <button mat-raised-button color="primary">Save</button>
  </mat-card-actions>
</mat-card>
```

---

## Chips / Tags

**Use for:** Tags, filters, removable selections

**Specs:**
- **Background:** `rgba(20, 184, 166, 0.2)` (teal with opacity)
- **Text color:** `$accent-teal` (#14b8a6)
- **Font:** 500 12px/16px, letter-spacing 0.02em
- **Padding:** 4px 12px
- **Border-radius:** 16px (pill shape)
- **Height:** 24px

**States:**
- **Hover:** Background → `rgba(20, 184, 166, 0.3)`
- **Active/Pressed:** Background → `rgba(20, 184, 166, 0.4)`
- **Removable:** Include X icon (16x16px) on right, 4px margin-left

**Angular Material:**
```html
<mat-chip-set>
  <mat-chip>Vegetarian</mat-chip>
  <mat-chip removable (removed)="remove()">
    Gluten-Free
    <mat-icon matChipRemove>cancel</mat-icon>
  </mat-chip>
</mat-chip-set>
```

**Custom SCSS:**
```scss
.mat-mdc-chip {
  background-color: rgba(20, 184, 166, 0.2);
  color: var(--color-accent);
  font: 500 12px/16px var(--font-family-base);
  letter-spacing: 0.02em;
  padding: 4px 12px;
  border-radius: 16px;
  height: 24px;
  transition: background-color var(--transition-fast);
  
  &:hover {
    background-color: rgba(20, 184, 166, 0.3);
  }
  
  mat-icon {
    font-size: 16px;
    width: 16px;
    height: 16px;
    margin-left: 4px;
  }
}
```

---

## Navigation

### Toolbar (Top App Bar)

**Specs:**
- **Background:** `$background-primary` (#0f172a)
- **Height:** 56px (mobile), 64px (desktop)
- **Shadow:** `$shadow-2`
- **Padding:** 0 16px (mobile), 0 24px (desktop)
- **Logo/Title:** 500 20px/28px, `$text-primary`, left-aligned
- **Actions:** Icon buttons, right-aligned

**Layout:**
- **Left:** Menu icon (mobile) or logo/title
- **Center:** Title (optional, mobile only)
- **Right:** Search, account menu, notifications (icon buttons)

**Angular Material:**
```html
<mat-toolbar color="primary">
  <button mat-icon-button (click)="toggleSidenav()">
    <mat-icon>menu</mat-icon>
  </button>
  <span class="toolbar-title">RecipeVault</span>
  <span class="spacer"></span>
  <button mat-icon-button aria-label="Search">
    <mat-icon>search</mat-icon>
  </button>
  <button mat-icon-button [matMenuTriggerFor]="accountMenu" aria-label="Account">
    <mat-icon>account_circle</mat-icon>
  </button>
</mat-toolbar>
```

**Custom SCSS:**
```scss
.mat-toolbar {
  background-color: var(--color-bg-primary);
  box-shadow: var(--shadow-2);
  height: 56px;
  padding: 0 16px;
  
  @include breakpoint-md {
    height: 64px;
    padding: 0 24px;
  }
  
  .toolbar-title {
    font: var(--font-h3);
    color: var(--color-text-primary);
  }
  
  .spacer {
    flex: 1 1 auto;
  }
}
```

---

### Side Navigation (Drawer)

**Specs:**
- **Width:** 280px (desktop), 100% or 80vw (mobile)
- **Background:** `$background-secondary` (#1e293b)
- **Shadow:** `$shadow-4`
- **Padding:** 16px

**Nav Items:**
- **Height:** 48px
- **Padding:** 12px 16px
- **Border-radius:** 8px
- **Icon size:** 24x24px, 16px margin-right
- **Font:** 400 14px/20px, `$text-primary`

**States:**
- **Hover:** Background → `$surface-default`
- **Active/Selected:** Background → `rgba(20, 184, 166, 0.15)`, text → `$accent-teal`, icon → `$accent-teal`
- **Focus:** 2px solid `$accent-teal` outline, 2px offset

**Angular Material:**
```html
<mat-sidenav-container>
  <mat-sidenav mode="side" opened>
    <mat-nav-list>
      <a mat-list-item routerLink="/recipes" routerLinkActive="active">
        <mat-icon>restaurant</mat-icon>
        <span>Recipes</span>
      </a>
      <a mat-list-item routerLink="/meal-plans" routerLinkActive="active">
        <mat-icon>calendar_today</mat-icon>
        <span>Meal Plans</span>
      </a>
      <a mat-list-item routerLink="/settings" routerLinkActive="active">
        <mat-icon>settings</mat-icon>
        <span>Settings</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>
  <mat-sidenav-content>
    <router-outlet></router-outlet>
  </mat-sidenav-content>
</mat-sidenav-container>
```

**Custom SCSS:**
```scss
.mat-sidenav {
  width: 280px;
  background-color: var(--color-bg-secondary);
  box-shadow: var(--shadow-4);
  padding: 16px;
  
  @media (max-width: 959px) {
    width: 80vw;
  }
}

.mat-mdc-list-item {
  height: 48px;
  padding: 12px 16px;
  border-radius: var(--radius-md);
  font: var(--font-body);
  color: var(--color-text-primary);
  transition: background-color var(--transition-fast);
  
  mat-icon {
    margin-right: 16px;
    font-size: 24px;
    width: 24px;
    height: 24px;
  }
  
  &:hover {
    background-color: var(--color-surface-default);
  }
  
  &.active {
    background-color: rgba(20, 184, 166, 0.15);
    color: var(--color-accent);
    
    mat-icon {
      color: var(--color-accent);
    }
  }
  
  &:focus-visible {
    outline: 2px solid var(--color-accent);
    outline-offset: 2px;
  }
}
```

---

## Dialogs / Modals

**Specs:**
- **Background:** `$surface-default` (#334155)
- **Border-radius:** 16px (desktop), 16px top corners only (mobile)
- **Shadow:** `$shadow-3`
- **Padding:** 24px
- **Max-width:** 560px (desktop), 100% (mobile)
- **Mobile position:** Bottom sheet style (slides up from bottom)

**Layout Structure:**
1. **Header** — Title (500 20px/28px), close button (icon, top-right)
2. **Content** — Scrollable, 400 14px/20px
3. **Actions** — Buttons, right-aligned (Cancel left, primary action right)

**Angular Material:**
```html
<h2 mat-dialog-title>Delete Recipe</h2>
<mat-dialog-content>
  <p>Are you sure you want to delete "Grandma's Apple Pie"? This action cannot be undone.</p>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button [mat-dialog-close]="false">Cancel</button>
  <button mat-raised-button color="warn" [mat-dialog-close]="true">Delete</button>
</mat-dialog-actions>
```

**Custom SCSS:**
```scss
.mat-mdc-dialog-container {
  background-color: var(--color-surface-default);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-3);
  padding: 24px;
  max-width: 560px;
  
  @media (max-width: 599px) {
    border-radius: var(--radius-xl) var(--radius-xl) 0 0;
    position: fixed;
    bottom: 0;
    width: 100%;
    max-width: 100%;
  }
}

.mat-mdc-dialog-title {
  font: var(--font-h3);
  color: var(--color-text-primary);
  margin-bottom: 16px;
}

.mat-mdc-dialog-content {
  font: var(--font-body);
  color: var(--color-text-primary);
  max-height: 60vh;
  overflow-y: auto;
}

.mat-mdc-dialog-actions {
  margin-top: 24px;
  gap: 8px;
}
```

---

## Snackbar / Toast Notifications

**Specs:**
- **Background:** `$surface-default` (#334155)
- **Text color:** `$text-primary` (#f1f5f9)
- **Font:** 400 14px/20px
- **Padding:** 12px 16px
- **Border-radius:** 8px
- **Shadow:** `$shadow-3`
- **Position:** Bottom-center (mobile), bottom-left (desktop)
- **Max-width:** 560px
- **Duration:** 3000ms (default), 5000ms (error), 2000ms (success)

**Variants:**
- **Success:** Left border 4px solid `$success`, icon `check_circle`
- **Error:** Left border 4px solid `$error`, icon `error`
- **Warning:** Left border 4px solid `$warning`, icon `warning`
- **Info:** Left border 4px solid `$info`, icon `info`

**Angular Material:**
```typescript
this.snackBar.open('Recipe saved successfully!', 'Dismiss', {
  duration: 3000,
  panelClass: ['snackbar-success']
});
```

**Custom SCSS:**
```scss
.mat-mdc-snack-bar-container {
  background-color: var(--color-surface-default);
  color: var(--color-text-primary);
  font: var(--font-body);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-3);
  padding: 12px 16px;
  max-width: 560px;
  
  &.snackbar-success {
    border-left: 4px solid var(--color-success);
  }
  
  &.snackbar-error {
    border-left: 4px solid var(--color-error);
  }
  
  &.snackbar-warning {
    border-left: 4px solid var(--color-warning);
  }
  
  &.snackbar-info {
    border-left: 4px solid var(--color-info);
  }
}
```

---

## Loading States

### Spinner (Indeterminate Progress)

**Specs:**
- **Size:** 40x40px (default), 24x24px (inline)
- **Color:** `$accent-teal` (#14b8a6)
- **Stroke width:** 4px
- **Animation:** Rotate 1.4s linear infinite

**Angular Material:**
```html
<mat-spinner color="accent" diameter="40"></mat-spinner>
```

---

### Progress Bar (Determinate Progress)

**Use for:** File uploads, data processing

**Specs:**
- **Height:** 4px
- **Background:** `$surface-hover` (#475569)
- **Fill color:** `$accent-teal` (#14b8a6)
- **Border-radius:** 2px
- **Animation:** Smooth fill transition (250ms ease-in-out)

**Angular Material:**
```html
<mat-progress-bar mode="determinate" [value]="uploadProgress" color="accent"></mat-progress-bar>
```

---

### Skeleton Loader (Content Placeholder)

**Use for:** Recipe cards, list items while loading

**Specs:**
- **Background:** Gradient shimmer animation
- **Base color:** `$surface-default` (#334155)
- **Highlight color:** `$surface-hover` (#475569)
- **Animation:** 1.5s ease-in-out infinite (shimmer effect)
- **Border-radius:** Match content (8px for text lines, 12px for cards)

**Example:**
```html
<div class="skeleton-card">
  <div class="skeleton-image"></div>
  <div class="skeleton-title"></div>
  <div class="skeleton-subtitle"></div>
</div>
```

**Custom SCSS:**
```scss
@keyframes shimmer {
  0% {
    background-position: -1000px 0;
  }
  100% {
    background-position: 1000px 0;
  }
}

.skeleton-card {
  background-color: var(--color-surface-default);
  border-radius: var(--radius-lg);
  padding: 16px;
  
  .skeleton-image,
  .skeleton-title,
  .skeleton-subtitle {
    background: linear-gradient(
      90deg,
      var(--color-surface-default) 25%,
      var(--color-surface-hover) 50%,
      var(--color-surface-default) 75%
    );
    background-size: 1000px 100%;
    animation: shimmer 1.5s ease-in-out infinite;
    border-radius: var(--radius-md);
  }
  
  .skeleton-image {
    width: 100%;
    height: 180px;
    margin-bottom: 12px;
  }
  
  .skeleton-title {
    width: 70%;
    height: 24px;
    margin-bottom: 8px;
  }
  
  .skeleton-subtitle {
    width: 50%;
    height: 16px;
  }
}
```

---

## Empty States

**Use for:** No recipes, no meal plans, search with no results

**Specs:**
- **Container:** Center-aligned, max-width 400px
- **Icon:** 64x64px Material icon, `$text-hint` color, margin-bottom 16px
- **Title:** 500 20px/28px, `$text-primary`, margin-bottom 8px
- **Description:** 400 14px/20px, `$text-secondary`, margin-bottom 24px
- **Action:** Primary button (e.g., "Create Recipe")

**Example:**
```html
<div class="empty-state">
  <mat-icon>restaurant</mat-icon>
  <h3>No Recipes Yet</h3>
  <p>Start building your recipe collection by adding your first recipe.</p>
  <button mat-raised-button color="primary">Add Recipe</button>
</div>
```

**Custom SCSS:**
```scss
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  max-width: 400px;
  margin: 0 auto;
  padding: var(--space-xl);
  
  mat-icon {
    font-size: 64px;
    width: 64px;
    height: 64px;
    color: var(--color-text-hint);
    margin-bottom: 16px;
  }
  
  h3 {
    font: var(--font-h3);
    color: var(--color-text-primary);
    margin-bottom: 8px;
  }
  
  p {
    font: var(--font-body);
    color: var(--color-text-secondary);
    margin-bottom: 24px;
  }
}
```

---

## Next Steps

- See `03-page-layouts.md` for full page designs (login, dashboard, recipe detail, cooking mode, etc.)
- Kovacs: implement these components in Angular Material, override Material theme with custom SCSS where needed
