# Frontend Tag Detail Refactor - TODO

## Status
Backend refactor complete. Frontend needs updating to remove UserTagAlias concepts and use per-recipe Detail.

## Changes Needed

### 1. Models (✅ Partially Done)
- [x] Remove UserTagAlias interface
- [x] Remove SetAliasRequest/SetAliasResponse
- [x] Update RecipeTag to include detail, normalizedEntityId, normalizedEntityType
- [x] Remove isOwnerAlias from RecipeTag
- [x] Remove alias fields from Tag
- [x] Update AssignTagItem to use detail instead of alias

### 2. Services (✅ Done)
- [x] Remove setAlias, removeAlias, getAliases methods from TagService
- [x] Remove imports of removed interfaces

### 3. Components (⏳ In Progress)

#### tag-selector.component.ts
**Current issues:**
- Still has alias UI (inline editing)
- References isOwnerAlias
- Has showAliasPrompt, saveAlias, cancelAlias methods
- displayFn uses tag.alias

**Needed changes:**
- Remove all alias editing UI
- Simplify to just show displayName from RecipeTag
- For Source tags, allow specifying detail when adding (optional input field)
- Remove alias-related properties and methods
- Update getTagTooltip to show globalName when detail is present

#### tag-selector.component.html
**Current issues:**
- Inline alias editor UI
- isOwnerAlias conditionals
- Alias add button

**Needed changes:**
- Remove inline alias editor
- Simplify chip display to show displayName
- Add optional detail input when adding Source tags
- Remove alias-related icons and buttons

#### recipe-list.component.ts
**Current issues:**
- References tag.alias in template

**Needed changes:**
- Update template to use appropriate field from RecipeTag

### 4. User Flow Changes

**Old flow (per-user alias):**
1. User adds "Chef" tag to recipe
2. User sets alias "Bobby Flay" for their account
3. All recipes with "Chef" show "Bobby Flay" for this user

**New flow (per-recipe detail):**
1. User adds "Chef" tag to Recipe A
2. Optionally enters detail "Bobby Flay" for this recipe
3. User adds "Chef" tag to Recipe B
4. Optionally enters detail "Gordon Ramsay" for this recipe
5. Each recipe shows its own chef name

### 5. UI Recommendations

For Source-type tags (Chef, Restaurant, Cookbook, Website):
- Show optional "Detail" input field in tag selector
- Placeholder text based on source type:
  - Chef: "e.g., Bobby Flay"
  - Restaurant: "e.g., Olive Garden"
  - Cookbook: "e.g., Joy of Cooking"
  - Website: "e.g., AllRecipes.com"
- Detail is stored per-recipe
- Gemini normalization happens automatically on save

## Testing Checklist
- [ ] Can add tags without detail
- [ ] Can add Source tags with detail
- [ ] Detail displays correctly in recipe view
- [ ] Gemini normalization works for recognized entities
- [ ] No errors related to removed UserTagAlias
- [ ] Tag autocomplete works
- [ ] Tag removal works
- [ ] AI-assigned tags display correctly

## Files to Update
- [ ] `ui/src/app/components/shared/tag-selector/tag-selector.component.ts`
- [ ] `ui/src/app/components/shared/tag-selector/tag-selector.component.html`
- [ ] `ui/src/app/components/shared/tag-selector/tag-selector.component.scss` (if needed)
- [ ] `ui/src/app/components/recipe-list/recipe-list.component.html` (check for tag.alias)
- [ ] Any other components displaying tags

## Notes
- displayName comes from backend (Detail || Tag.Name)
- Gemini normalization is automatic server-side
- No need for frontend normalization logic
- globalName always contains the original tag name
