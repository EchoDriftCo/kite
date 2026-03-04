# Pantry Seed Data Plan

*Companion to ROADMAP.md Section 4B (Pantry & "What Can I Make?")*

## Overview

When the Pantry feature is built, we need seed data for the quick-select onboarding UI. Items should be ranked by frequency in the global RecipeVault recipe database (once populated). Until then, use a curated list of common kitchen staples.

## Seed Categories & Items

### Dairy & Eggs
| Item | Category |
|------|----------|
| Butter | Dairy |
| Eggs | Dairy |
| Milk (whole) | Dairy |
| Heavy cream | Dairy |
| Sour cream | Dairy |
| Cream cheese | Dairy |
| Cheddar cheese | Dairy |
| Parmesan cheese | Dairy |
| Mozzarella cheese | Dairy |
| Greek yogurt | Dairy |
| Half and half | Dairy |

### Proteins
| Item | Category |
|------|----------|
| Chicken breast | Protein |
| Chicken thighs | Protein |
| Ground beef | Protein |
| Ground turkey | Protein |
| Bacon | Protein |
| Pork chops | Protein |
| Sausage (Italian) | Protein |
| Shrimp | Protein |
| Salmon | Protein |
| Tofu | Protein |
| Canned tuna | Protein |

### Produce (Fresh)
| Item | Category |
|------|----------|
| Onions (yellow) | Produce |
| Garlic | Produce |
| Potatoes | Produce |
| Carrots | Produce |
| Celery | Produce |
| Bell peppers | Produce |
| Tomatoes | Produce |
| Lemons | Produce |
| Limes | Produce |
| Mushrooms | Produce |
| Green onions | Produce |
| Fresh ginger | Produce |
| Jalapeños | Produce |
| Lettuce/greens | Produce |
| Broccoli | Produce |
| Spinach | Produce |
| Avocado | Produce |
| Cilantro | Produce |
| Fresh basil | Produce |
| Fresh parsley | Produce |

### Pantry Staples
| Item | Category |
|------|----------|
| Olive oil | Pantry |
| Vegetable oil | Pantry |
| Salt | Pantry |
| Black pepper | Pantry |
| All-purpose flour | Pantry |
| Granulated sugar | Pantry |
| Brown sugar | Pantry |
| Baking powder | Pantry |
| Baking soda | Pantry |
| Vanilla extract | Pantry |
| Chicken broth | Pantry |
| Beef broth | Pantry |
| Canned tomatoes (diced) | Pantry |
| Tomato paste | Pantry |
| Soy sauce | Pantry |
| Worcestershire sauce | Pantry |
| Hot sauce | Pantry |
| Honey | Pantry |
| Apple cider vinegar | Pantry |
| Red wine vinegar | Pantry |
| Rice vinegar | Pantry |
| Dijon mustard | Pantry |
| Ketchup | Pantry |
| Mayonnaise | Pantry |
| Sriracha | Pantry |

### Grains & Pasta
| Item | Category |
|------|----------|
| White rice | Grain |
| Brown rice | Grain |
| Pasta (spaghetti) | Grain |
| Pasta (penne) | Grain |
| Bread (sliced) | Grain |
| Tortillas (flour) | Grain |
| Tortillas (corn) | Grain |
| Breadcrumbs | Grain |
| Oats | Grain |
| Quinoa | Grain |

### Spices & Seasonings
| Item | Category |
|------|----------|
| Garlic powder | Spice |
| Onion powder | Spice |
| Paprika | Spice |
| Smoked paprika | Spice |
| Cumin | Spice |
| Chili powder | Spice |
| Italian seasoning | Spice |
| Oregano (dried) | Spice |
| Thyme (dried) | Spice |
| Cinnamon | Spice |
| Red pepper flakes | Spice |
| Bay leaves | Spice |
| Cayenne pepper | Spice |
| Nutmeg | Spice |

### Canned & Jarred
| Item | Category |
|------|----------|
| Canned black beans | Canned |
| Canned chickpeas | Canned |
| Canned kidney beans | Canned |
| Canned coconut milk | Canned |
| Canned corn | Canned |
| Salsa | Canned |
| Peanut butter | Canned |
| Jam/jelly | Canned |

### Frozen
| Item | Category |
|------|----------|
| Frozen peas | Frozen |
| Frozen corn | Frozen |
| Frozen mixed vegetables | Frozen |
| Frozen berries | Frozen |
| Frozen spinach | Frozen |

## Total: ~120 items across 8 categories

## Implementation Notes

- Seed via EF migration (same pattern as Equipment and Tags)
- Each item gets a `Code` (slugified name) for matching
- `IsCommon` flag for items nearly everyone has (salt, pepper, oil, etc.)
- Quick-select UI groups by category with checkboxes
- Future: rank by frequency in recipe database once we have enough data
- `IngredientAlias` table maps variations (e.g., "chicken breasts" → "chicken breast")

## Recipe Matching Strategy

When checking "Can I Make This?":
1. Parse recipe ingredients → extract core item names
2. Normalize (lowercase, depluralize, remove preparations)
3. Check `IngredientAlias` for known mappings
4. Fuzzy match against pantry items (Levenshtein distance)
5. AI fallback for complex cases ("boneless skinless chicken thighs" → "chicken thighs")

Confidence levels:
- **Exact match** (100%): "garlic" ↔ "garlic"
- **Alias match** (95%): "chicken breasts" ↔ "chicken breast"
- **Fuzzy match** (80%+): "sweet onion" ↔ "onions (yellow)"
- **AI match** (variable): "arborio rice" ↔ "white rice" (close but not exact)

Items below 70% confidence should show as "uncertain" rather than "missing".
