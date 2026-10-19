# RecipeVault Design System — Foundation

**Version:** 2.1 (light + dark themes, accessible colour roles)  
**Status:** Current  
**Date:** 2026-10-18 (v2.0: 2026-03-21)

> **What changed in 2.1:** the app ships a light theme by default with a dark theme toggle (not dark-only), the brand colours are warm orange `#ff8c42` and sage `#7c9885` (not amber/teal), and every colour now has a role that says whether it may be used for text. Source of truth for values: [`ui/src/_tokens.scss`](../../ui/src/_tokens.scss). Rationale: [tasks/5.0](tasks/5.0-design-system-foundation.md).

---

## Design Philosophy

**"Dark Mode Fine Dining"** — Clean, modern, polished. Professional recipe management tool, not a cluttered blog. Mobile-first. Accessibility non-negotiable. Every state designed, not just the happy path.

**Core Principles:**
- **Clarity over decoration** — Every element earns its place
- **Consistent spacing** — 4px/8px grid system, no arbitrary values
- **Intentional color** — Warm neutrals with orange and sage accents; bright brand colours are for fills, darker variants for text
- **Responsive by default** — Mobile-first, scales up gracefully
- **State completeness** — Loading, empty, error, success, hover, focus, disabled all designed
- **Accessible always** — WCAG 2.1 AA minimum, AAA where practical

---

## Color System

Two themes: **light** (default, follows the OS preference until the person picks one) and **dark** (`.dark-theme` on `<body>`). Components use tokens only, so they work in both.

### Surfaces and text

| Token | Light | Dark | Use |
|---|---|---|---|
| `--color-bg-primary` | `#fdfcfa` | `#0f172a` | App chrome |
| `--color-bg-secondary` | `#f8f6f3` | `#1e293b` | Page background |
| `--color-surface-default` | `#ffffff` | `#334155` | Cards, dialogs |
| `--color-surface-hover` / `-active` | `#f5f3f0` / `#eeebe7` | `#475569` / `#64748b` | Hover and pressed surfaces, filled inputs |
| `--color-border-default` | `#e5e5e5` | `rgba(241,245,249,.2)` | Outlines, outlined buttons |
| `--color-text-primary` | `#1a1a1a` | `#f1f5f9` | Body text, headings |
| `--color-text-secondary` | `#6b6b6b` (5.3:1) | `#cbd5e1` (7.0:1) | Descriptions, metadata, dates, counts, helper text |
| `--color-text-hint` / `-disabled` | `#9ca3af` | `#64748b` / `#94a3b8` | **Only** placeholders and disabled controls |

### Colour roles

Bright brand colours fail as small text (orange `#ff8c42` is 2.3:1 on white), so each colour has a fill role and a text role.

| Role | Token | Light | Dark | Use for |
|---|---|---|---|---|
| Brand orange (fill) | `--color-primary` | `#ff8c42` | `#ff8c42` | Primary buttons, decorative icons |
| Text on orange | `--color-on-primary` | `#1a1a1a` | `#1a2230` | Labels on orange fills |
| Orange as text | `--color-primary-text` | `#a34b15` | `#ffa366` | Prices, "Step 1", orange labels |
| Rating stars | `--color-rating` | `#d9691f` | `#ffa366` | Filled stars |
| Orange tint | `--color-primary-subtle-bg` | 12% orange | 18% orange | Highlights such as leftovers |
| Sage (decorative) | `--color-accent` | `#7c9885` | `#7c9885` | Borders, focus ring, decorative icons |
| Sage as text | `--color-accent-text` | `#4a6354` | `#c1d4c7` | **Links**, active nav, step numbers |
| Sage fill | `--color-accent-fill` | `#5a7363` | `#a8c4b2` | Selected toggles, switches, accent buttons |
| Text on sage fill | `--color-on-accent` | `#ffffff` | `#1a2230` | Labels on sage fills |
| Sage tint | `--color-accent-subtle-bg` / `-border` | 10% / 35% sage | 20% / 30% sage | Chips, selected rows |
| Error as text | `--color-error-text` | `#c62828` | `#fca5a5` | Destructive labels, error messages |

**Rules**
- Never use `--color-primary` or `--color-accent` for text, and never use `--color-text-hint` for text people need to read.
- Links are `--color-accent-text`.
- Material palettes follow the same rule: accent and warn use hue 700 in light mode (white label) and hue 300 in dark mode (dark label), so Material buttons pass without overrides.

**Semantic:** `--color-success`, `--color-error`, `--color-warning`, `--color-info`, each with a `-bg` tint. See `_tokens.scss` for values.

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
- **Focus indicators:** Visible outline (2px solid `--color-accent`)
- **Touch targets:** Minimum 44x44px (48x48px preferred)
- **Font size:** Body text minimum 14px; 13px (`--font-body-small`) for helper sentences; 12px (`--font-caption`) only for metadata such as timestamps, counts and badges. Inputs are 16px on mobile so iOS doesn't zoom.
- **Keyboard navigation:** All interactive elements accessible via Tab/Enter/Space
- **Screen reader support:** Semantic HTML, ARIA labels where needed
- **Color not sole indicator:** Use icons, text, or patterns alongside color

**Contrast validation:** every text/background pair above was measured; all body text is at least 4.5:1 in both themes. Run `docs/designs/reviews/2026-10-18-ui-review/capture.mjs` against `npm start` to re-check every screen; it writes each failing text node to `contrast.txt`.

**Focus State:**
```scss
:focus-visible {
  outline: 2px solid var(--color-accent);
  outline-offset: 2px;
}
```

---

## Design Tokens (CSS Variables)

All values are CSS custom properties defined in [`ui/src/_tokens.scss`](../../ui/src/_tokens.scss), with the dark overrides under `.dark-theme`. That file is the source of truth. It isn't copied here, so it can't drift.

| Group | Prefix |
|---|---|
| Colour | `--color-*` (see the tables above) |
| Spacing | `--space-xs` … `--space-2xl` (4, 8, 16, 24, 32, 48px) |
| Type (shorthand) | `--font-h1` … `--font-overline` |
| Type (size only) | `--font-size-caption` (12px), `--font-size-body-small` (13px), `--font-size-body` (14px) |
| Radius | `--radius-sm` … `--radius-full` |
| Elevation | `--shadow-0` … `--shadow-5` (softer in light mode) |
| Motion | `--transition-fast` / `-normal` / `-slow` |
| Angular Material | `--mat-sys-*`, kept in sync with the colour roles |

---

## Next Steps

See component specs in:
- `02-components.md` — Buttons, forms, cards, navigation
- `03-page-layouts.md` — Login, dashboard, recipe detail, cooking mode, etc.

Kovacs: implement these tokens first, then move to components.
