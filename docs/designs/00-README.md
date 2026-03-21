# RecipeVault Design System — Complete Spec

**Version:** 2.0 (UI Redesign)  
**Designer:** Vega  
**Date:** 2026-03-21  
**Status:** ✅ Ready for Implementation

---

## What This Is

Complete visual design system for RecipeVault UI redesign. Every color, font size, spacing value, component state, and page layout fully specified. No guessing. No "just make it look good." Pixel-perfect implementation-ready.

---

## File Structure

1. **[01-foundation.md](01-foundation.md)** — Color palette, typography, spacing, shadows, borders, breakpoints, design tokens (CSS variables)
2. **[02-components.md](02-components.md)** — Buttons, forms, cards, navigation, dialogs, chips, loading states, empty states
3. **[03-page-layouts.md](03-page-layouts.md)** — Login, dashboard, recipe detail, edit, import, meal plans, grocery list, settings, cooking mode

---

## Implementation Order (Kovacs)

### Phase 1: Foundation (Do This First)

1. **Read `01-foundation.md`** — Understand the design system
2. **Create `theme-tokens.scss`** — Copy all CSS custom properties from foundation doc
3. **Update `theme.scss`** — Override Material theme with our color palette
4. **Create `_mixins.scss`** — Breakpoint mixins, utility classes
5. **Test foundation:** Create a single test page with all foundation styles applied (colors, typography, spacing visible)

### Phase 2: Components (Build These Next)

1. **Read `02-components.md`** — Component specs
2. **Create component-specific SCSS files:**
   - `_buttons.scss` — All button variants
   - `_forms.scss` — Inputs, textareas, selects, checkboxes, toggles
   - `_cards.scss` — Recipe cards, content cards
   - `_navigation.scss` — Toolbar, side nav
   - `_dialogs.scss` — Modals, snackbars
   - `_loading.scss` — Spinners, progress bars, skeletons
   - `_empty-states.scss` — Empty state component
3. **Test components:** Create a component library page (like a style guide) showing all component states
4. **Material overrides:** Use `::ng-deep` or ViewEncapsulation.None to override Material components where needed

### Phase 3: Pages (Build in This Order)

1. **Login** (`/login`) — Simple, test auth flow
2. **Dashboard** (`/recipes`) — Recipe list/grid, search, filter, empty state
3. **Recipe Detail** (`/recipes/:id`) — View recipe, actions (edit, delete, share)
4. **Recipe Edit** (`/recipes/:id/edit`, `/recipes/new`) — Form with ingredient/instruction arrays
5. **Settings** (`/settings`) — Account, preferences, data export
6. **Meal Plans** (`/meal-plans`) — Calendar view, add recipes to slots
7. **Grocery List** (`/meal-plans/:id/grocery-list`) — Consolidated list, print
8. **Import Recipe** (`/recipes/import`) — Upload image, AI parse, review
9. **Cooking Mode** (`/recipes/:id/cook`) — Step-by-step (future phase)

### Phase 4: Polish

1. **Responsive testing:** Test all pages on mobile (375px), tablet (768px), desktop (1280px+)
2. **Accessibility audit:** Keyboard navigation, focus states, screen reader labels
3. **Animation polish:** Ensure transitions are smooth (check hover/focus/loading states)
4. **Print styles:** Test grocery list print layout
5. **Performance:** Lazy load images, optimize bundle size

---

## Design Principles (Read This Before You Start)

### 1. **Use Design Tokens (CSS Variables)**

Every color, spacing, font, shadow, and border-radius is defined as a CSS custom property in `01-foundation.md`. **Use them.** Do not hard-code values.

**Good:**
```scss
.button {
  background-color: var(--color-primary);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
}
```

**Bad:**
```scss
.button {
  background-color: #f59e0b;  // ❌ Hard-coded color
  padding: 8px 16px;          // ❌ Magic numbers
  border-radius: 8px;         // ❌ Not using token
}
```

### 2. **Mobile-First Responsive Design**

Base styles are for mobile (< 600px). Use `@media (min-width: ...)` to scale up for tablet/desktop.

**Good:**
```scss
.card {
  width: 100%;  // Mobile default
  
  @media (min-width: 960px) {
    width: 320px;  // Desktop grid
  }
}
```

**Bad:**
```scss
.card {
  width: 320px;  // ❌ Desktop-first
  
  @media (max-width: 959px) {
    width: 100%;  // ❌ Scaling down (harder to maintain)
  }
}
```

### 3. **Design Every State**

Do not assume Material Design handles all states. Explicitly define:
- **Default** (at rest)
- **Hover** (mouse over)
- **Active/Pressed** (click/tap)
- **Focus** (keyboard navigation)
- **Disabled** (not interactive)
- **Loading** (async operations)
- **Error** (validation failures)
- **Empty** (no data)

### 4. **Accessibility Is Non-Negotiable**

- **Contrast:** All text must meet WCAG 2.1 AA (4.5:1 for normal text, 3:1 for large text)
- **Focus states:** 2px solid `$accent-teal` outline, 2px offset, visible on all interactive elements
- **Touch targets:** Minimum 44x44px (48x48px preferred)
- **Keyboard navigation:** Tab order logical, Enter/Space activate buttons/links
- **Screen readers:** Use semantic HTML, ARIA labels where needed
- **Font size:** Minimum 14px on mobile (prevents iOS zoom on focus)

### 5. **Component Reuse Over Custom CSS**

Use Angular Material components where possible. Override with SCSS when Material doesn't match our design. Don't reinvent the wheel.

**Good:**
```html
<button mat-raised-button color="primary">Save</button>
```
```scss
.mat-mdc-raised-button.mat-primary {
  border-radius: var(--radius-md);  // Override Material's radius
}
```

**Bad:**
```html
<div class="custom-button">Save</div>
```
```scss
.custom-button {
  /* Rebuilding Material button from scratch */
}
```

### 6. **Consistency Over Creativity**

If the spec says 16px padding, use 16px. Not 15px, not 18px. Consistency is more valuable than "looks slightly better."

If you think a spec is wrong, **ask Vega** (post in #agent-comms). Do not improvise.

---

## What Success Looks Like

When implementation is complete:

1. **Pixel-perfect match** — Screenshots of your implementation should be indistinguishable from the specs (allowing for Material rendering differences)
2. **No magic numbers** — All spacing, colors, font sizes use design tokens
3. **Responsive everywhere** — All pages work on mobile (375px), tablet (768px), desktop (1280px+)
4. **Accessible** — WCAG 2.1 AA compliance, keyboard navigation works, focus states visible
5. **Performant** — No jank, smooth animations, fast load times
6. **Maintainable** — Clean SCSS structure, reusable components, clear file organization

---

## Questions?

Post in **#agent-comms** (Discord channel: 1478183841096142979). Tag **@Vega** for design clarifications.

Do not guess. Do not improvise. Do not "make it look better." Follow the specs. If something is unclear, **ask.**

Now go build something worth looking at. 🎨
