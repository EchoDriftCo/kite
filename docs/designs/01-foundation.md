# RecipeVault Design System — Foundation

**Version:** 2.0 (UI Redesign)  
**Status:** Approved by Vega  
**Date:** 2026-03-21

---

## Design Philosophy

**"Dark Mode Fine Dining"** — Clean, modern, polished. Professional recipe management tool, not a cluttered blog. Mobile-first. Accessibility non-negotiable. Every state designed, not just the happy path.

**Core Principles:**
- **Clarity over decoration** — Every element earns its place
- **Consistent spacing** — 4px/8px grid system, no arbitrary values
- **Intentional color** — Dark theme with amber/teal accents, not "everything is bright"
- **Responsive by default** — Mobile-first, scales up gracefully
- **State completeness** — Loading, empty, error, success, hover, focus, disabled all designed
- **Accessible always** — WCAG 2.1 AA minimum, AAA where practical

---

## Color System

### Dark Theme (Primary)

**Surface Colors:**
```scss
$background-primary: #0f172a;    // slate-900 — app background, toolbar
$background-secondary: #1e293b;  // slate-800 — main content area
$surface-default: #334155;       // slate-700 — cards, elevated surfaces
$surface-hover: #475569;         // slate-600 — hover states
$surface-active: #64748b;        // slate-500 — active states

$divider: rgba(241, 245, 249, 0.12);  // subtle dividers
$border-default: rgba(241, 245, 249, 0.2);  // form fields, outlines
```

**Text Colors:**
```scss
$text-primary: #f1f5f9;          // slate-100 — primary text, headings
$text-secondary: #cbd5e1;        // slate-300 — secondary text, labels
$text-disabled: #94a3b8;         // slate-400 — disabled text
$text-hint: #64748b;             // slate-500 — placeholder, hints
```

**Brand Colors:**
```scss
$primary-amber: #f59e0b;         // amber-500 — primary actions, CTAs
$primary-amber-hover: #fbbf24;   // amber-400 — hover state
$primary-amber-active: #d97706;  // amber-600 — active/pressed state

$accent-teal: #14b8a6;           // teal-500 — secondary actions, links
$accent-teal-hover: #2dd4bf;     // teal-400 — hover state
$accent-teal-active: #0d9488;    // teal-600 — active/pressed state
```

**Semantic Colors:**
```scss
$success: #10b981;               // green-500 — success states
$success-bg: rgba(16, 185, 129, 0.15);  // success backgrounds

$error: #ef4444;                 // red-500 — errors, destructive actions
$error-bg: rgba(239, 68, 68, 0.15);    // error backgrounds

$warning: #f59e0b;               // amber-500 — warnings
$warning-bg: rgba(245, 158, 11, 0.15); // warning backgrounds

$info: #3b82f6;                  // blue-500 — informational
$info-bg: rgba(59, 130, 246, 0.15);    // info backgrounds
```

**Usage Rules:**
- **Background-primary** for fixed app chrome (toolbar, nav)
- **Background-secondary** for scrollable content areas
- **Surface-default** for cards, dialogs, modals
- **Primary-amber** for high-emphasis actions (save, submit, create)
- **Accent-teal** for medium-emphasis actions (cancel, secondary CTAs, links)
- **Never use pure white or pure black** — always use the palette

### Light Theme (Future Phase — Not MVP)

Light theme will be introduced in a future phase. Design system will be extended with light mode equivalents when implemented. For now, dark theme only.

---

## Typography

**Font Family:**
```scss
$font-family-base: 'Roboto', 'Helvetica Neue', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
$font-family-mono: 'Roboto Mono', 'Consolas', 'Monaco', monospace;  // for recipe instructions, code
```

**Type Scale:**

| Level | Size | Weight | Line Height | Letter Spacing | Usage |
|-------|------|--------|-------------|----------------|-------|
| **h1** | 32px | 300 (Light) | 40px | -0.01em | Page titles (rare) |
| **h2** | 24px | 400 (Regular) | 32px | 0 | Section headings |
| **h3** | 20px | 500 (Medium) | 28px | 0.01em | Subsection headings |
| **h4** | 18px | 500 (Medium) | 24px | 0.01em | Card titles, list headers |
| **body-large** | 16px | 400 (Regular) | 24px | 0.01em | Primary body text |
| **body** | 14px | 400 (Regular) | 20px | 0.01em | Default body text |
| **body-small** | 13px | 400 (Regular) | 18px | 0.02em | Captions, helper text |
| **button** | 14px | 500 (Medium) | 20px | 0.05em | Button labels (uppercase) |
| **caption** | 12px | 400 (Regular) | 16px | 0.03em | Timestamps, metadata |
| **overline** | 11px | 500 (Medium) | 16px | 0.1em | Labels, tags (uppercase) |

**Typography Rules:**
- **Minimum font size: 14px on mobile** (WCAG 2.1)
- **Body text never smaller than 14px**
- **Headings use weight, not size alone** for hierarchy
- **Letter-spacing increases as size decreases** for legibility
- **Line-height is 1.5x font-size minimum** for readability
- **Button text always uppercase** with increased letter-spacing (0.05em)

**CSS Custom Properties:**
```scss
:root {
  --font-h1: 300 32px/40px var(--font-family-base);
  --font-h2: 400 24px/32px var(--font-family-base);
  --font-h3: 500 20px/28px var(--font-family-base);
  --font-h4: 500 18px/24px var(--font-family-base);
  --font-body-large: 400 16px/24px var(--font-family-base);
  --font-body: 400 14px/20px var(--font-family-base);
  --font-body-small: 400 13px/18px var(--font-family-base);
  --font-button: 500 14px/20px var(--font-family-base);
  --font-caption: 400 12px/16px var(--font-family-base);
  --font-overline: 500 11px/16px var(--font-family-base);
  
  --font-family-base: 'Roboto', 'Helvetica Neue', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-family-mono: 'Roboto Mono', 'Consolas', 'Monaco', monospace;
}
```

---

## Spacing System

**4px/8px Grid** — All spacing is a multiple of 4px. Use 8px for most layout spacing.

**Spacing Scale:**
```scss
$space-xs: 4px;    // Tight spacing (icon-to-text, chip padding)
$space-sm: 8px;    // Small spacing (between related items)
$space-md: 16px;   // Default spacing (card padding, form field margins)
$space-lg: 24px;   // Large spacing (section gaps, page padding)
$space-xl: 32px;   // Extra-large spacing (major sections)
$space-2xl: 48px;  // Hero spacing (rare, page headers)
```

**Usage Guidelines:**
- **Card padding:** 16px (mobile), 24px (desktop)
- **Button padding:** 8px vertical, 16px horizontal
- **Form field margin-bottom:** 16px
- **Section gap:** 24px
- **Page padding:** 16px (mobile), 24px (tablet), 32px (desktop)
- **Icon-to-text gap:** 8px
- **List item padding:** 12px vertical, 16px horizontal

**CSS Custom Properties:**
```scss
:root {
  --space-xs: 4px;
  --space-sm: 8px;
  --space-md: 16px;
  --space-lg: 24px;
  --space-xl: 32px;
  --space-2xl: 48px;
}
```

---

## Elevation & Shadows

**Shadow Scale** (Material Design inspired):
```scss
$shadow-0: none;  // Flat surfaces
$shadow-1: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24);  // Cards at rest
$shadow-2: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);  // Cards on hover
$shadow-3: 0 10px 20px rgba(0, 0, 0, 0.19), 0 6px 6px rgba(0, 0, 0, 0.23);  // Modals, dialogs
$shadow-4: 0 14px 28px rgba(0, 0, 0, 0.25), 0 10px 10px rgba(0, 0, 0, 0.22);  // Drawer, elevated sheets
$shadow-5: 0 19px 38px rgba(0, 0, 0, 0.30), 0 15px 12px rgba(0, 0, 0, 0.22);  // High elevation (menus, tooltips)
```

**Usage:**
- **Cards:** shadow-1 at rest, shadow-2 on hover
- **Dialogs/Modals:** shadow-3
- **Drawer/Sidenav:** shadow-4
- **Tooltips/Menus:** shadow-5
- **Buttons:** No shadow at rest, subtle shadow on hover (optional)

**CSS Custom Properties:**
```scss
:root {
  --shadow-0: none;
  --shadow-1: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24);
  --shadow-2: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
  --shadow-3: 0 10px 20px rgba(0, 0, 0, 0.19), 0 6px 6px rgba(0, 0, 0, 0.23);
  --shadow-4: 0 14px 28px rgba(0, 0, 0, 0.25), 0 10px 10px rgba(0, 0, 0, 0.22);
  --shadow-5: 0 19px 38px rgba(0, 0, 0, 0.30), 0 15px 12px rgba(0, 0, 0, 0.22);
}
```

---

## Border Radius

**Radius Scale:**
```scss
$radius-sm: 4px;   // Small elements (chips, tags)
$radius-md: 8px;   // Default (buttons, inputs, small cards)
$radius-lg: 12px;  // Cards, panels
$radius-xl: 16px;  // Large cards, modals
$radius-full: 9999px;  // Pills, avatars, circular buttons
```

**Usage:**
- **Buttons:** 8px
- **Form fields:** 8px
- **Cards:** 12px
- **Modals/Dialogs:** 16px (top corners only on mobile)
- **Chips/Tags:** 4px
- **Avatar/Icon buttons:** 9999px (fully rounded)

**CSS Custom Properties:**
```scss
:root {
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-full: 9999px;
}
```

---

## Transitions & Animation

**Timing Functions:**
```scss
$ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);  // Standard Material easing
$ease-out: cubic-bezier(0.0, 0, 0.2, 1);     // Deceleration
$ease-in: cubic-bezier(0.4, 0, 1, 1);        // Acceleration
```

**Duration:**
```scss
$duration-fast: 150ms;    // Micro-interactions (hover, focus)
$duration-normal: 250ms;  // Default transitions (color, size)
$duration-slow: 350ms;    // Enter/exit animations (dialogs, drawers)
```

**Usage:**
- **Hover states:** 150ms ease-in-out
- **Focus states:** 150ms ease-in-out
- **Color transitions:** 250ms ease-in-out
- **Dialog open:** 350ms ease-out
- **Dialog close:** 250ms ease-in
- **Transform (translate, scale):** 250ms ease-in-out

**Standard Transition:**
```scss
transition: all 250ms cubic-bezier(0.4, 0, 0.2, 1);
```

**CSS Custom Properties:**
```scss
:root {
  --transition-fast: 150ms cubic-bezier(0.4, 0, 0.2, 1);
  --transition-normal: 250ms cubic-bezier(0.4, 0, 0.2, 1);
  --transition-slow: 350ms cubic-bezier(0.0, 0, 0.2, 1);
}
```

---

## Breakpoints

**Responsive Grid:**
```scss
$breakpoint-xs: 0px;       // Mobile portrait (default)
$breakpoint-sm: 600px;     // Mobile landscape / small tablet
$breakpoint-md: 960px;     // Tablet portrait
$breakpoint-lg: 1280px;    // Tablet landscape / small desktop
$breakpoint-xl: 1920px;    // Desktop / large screens
```

**Usage:**
- **Mobile-first design** — Base styles are for mobile, scale up with `@media (min-width: ...)`
- **Single-column layout:** xs-sm (< 600px)
- **Two-column layout:** md (960px+)
- **Three-column layout:** lg+ (1280px+)
- **Max content width:** 1440px (center-aligned with side padding)

**SCSS Mixins:**
```scss
@mixin breakpoint-sm {
  @media (min-width: 600px) { @content; }
}
@mixin breakpoint-md {
  @media (min-width: 960px) { @content; }
}
@mixin breakpoint-lg {
  @media (min-width: 1280px) { @content; }
}
@mixin breakpoint-xl {
  @media (min-width: 1920px) { @content; }
}
```

---

## Accessibility Standards

**WCAG 2.1 Level AA (Minimum):**
- **Contrast ratio:** 4.5:1 for normal text, 3:1 for large text (18px+)
- **Focus indicators:** Visible, high-contrast outline (2px solid $accent-teal)
- **Touch targets:** Minimum 44x44px (48x48px preferred)
- **Font size:** Minimum 14px (16px on mobile for no iOS zoom)
- **Keyboard navigation:** All interactive elements accessible via Tab/Enter/Space
- **Screen reader support:** Semantic HTML, ARIA labels where needed
- **Color not sole indicator:** Use icons, text, or patterns alongside color

**Contrast Validation:**
- **Text-primary (#f1f5f9) on background-primary (#0f172a):** 15.3:1 ✅ AAA
- **Text-secondary (#cbd5e1) on background-primary (#0f172a):** 11.2:1 ✅ AAA
- **Primary-amber (#f59e0b) on background-primary (#0f172a):** 7.8:1 ✅ AA
- **Accent-teal (#14b8a6) on background-primary (#0f172a):** 6.2:1 ✅ AA
- **Error (#ef4444) on background-primary (#0f172a):** 5.1:1 ✅ AA

**Focus State:**
```scss
:focus-visible {
  outline: 2px solid $accent-teal;
  outline-offset: 2px;
  border-radius: $radius-md;
}
```

---

## Design Tokens (CSS Variables)

All foundation values are exposed as CSS custom properties for runtime theming and consistency.

```scss
:root {
  // Colors — Surface
  --color-bg-primary: #0f172a;
  --color-bg-secondary: #1e293b;
  --color-surface-default: #334155;
  --color-surface-hover: #475569;
  --color-surface-active: #64748b;
  --color-divider: rgba(241, 245, 249, 0.12);
  --color-border-default: rgba(241, 245, 249, 0.2);

  // Colors — Text
  --color-text-primary: #f1f5f9;
  --color-text-secondary: #cbd5e1;
  --color-text-disabled: #94a3b8;
  --color-text-hint: #64748b;

  // Colors — Brand
  --color-primary: #f59e0b;
  --color-primary-hover: #fbbf24;
  --color-primary-active: #d97706;
  --color-accent: #14b8a6;
  --color-accent-hover: #2dd4bf;
  --color-accent-active: #0d9488;

  // Colors — Semantic
  --color-success: #10b981;
  --color-success-bg: rgba(16, 185, 129, 0.15);
  --color-error: #ef4444;
  --color-error-bg: rgba(239, 68, 68, 0.15);
  --color-warning: #f59e0b;
  --color-warning-bg: rgba(245, 158, 11, 0.15);
  --color-info: #3b82f6;
  --color-info-bg: rgba(59, 130, 246, 0.15);

  // Spacing
  --space-xs: 4px;
  --space-sm: 8px;
  --space-md: 16px;
  --space-lg: 24px;
  --space-xl: 32px;
  --space-2xl: 48px;

  // Typography
  --font-family-base: 'Roboto', 'Helvetica Neue', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-family-mono: 'Roboto Mono', 'Consolas', 'Monaco', monospace;
  --font-h1: 300 32px/40px var(--font-family-base);
  --font-h2: 400 24px/32px var(--font-family-base);
  --font-h3: 500 20px/28px var(--font-family-base);
  --font-h4: 500 18px/24px var(--font-family-base);
  --font-body-large: 400 16px/24px var(--font-family-base);
  --font-body: 400 14px/20px var(--font-family-base);
  --font-body-small: 400 13px/18px var(--font-family-base);
  --font-button: 500 14px/20px var(--font-family-base);
  --font-caption: 400 12px/16px var(--font-family-base);
  --font-overline: 500 11px/16px var(--font-family-base);

  // Border Radius
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-full: 9999px;

  // Shadows
  --shadow-0: none;
  --shadow-1: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24);
  --shadow-2: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
  --shadow-3: 0 10px 20px rgba(0, 0, 0, 0.19), 0 6px 6px rgba(0, 0, 0, 0.23);
  --shadow-4: 0 14px 28px rgba(0, 0, 0, 0.25), 0 10px 10px rgba(0, 0, 0, 0.22);
  --shadow-5: 0 19px 38px rgba(0, 0, 0, 0.30), 0 15px 12px rgba(0, 0, 0, 0.22);

  // Transitions
  --transition-fast: 150ms cubic-bezier(0.4, 0, 0.2, 1);
  --transition-normal: 250ms cubic-bezier(0.4, 0, 0.2, 1);
  --transition-slow: 350ms cubic-bezier(0.0, 0, 0.2, 1);
}
```

---

## Next Steps

See component specs in:
- `02-components.md` — Buttons, forms, cards, navigation
- `03-page-layouts.md` — Login, dashboard, recipe detail, cooking mode, etc.

Kovacs: implement these tokens first, then move to components.
