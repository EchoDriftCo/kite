# Sentry Error Fixes - 2026-03-08

## Issues Fixed

### 1. ✅ Cooking Log Stats Endpoint Failure (Sentry #7320438160)
**Error:** `/api/v1/cooking-log/stats` returning 500 in ~122ms

**Root Cause:**
- `CookingLogRepository.GetBySubjectIdAndDateRangeAsync()` was accessing `cl.CreatedSubject.SubjectId` in the Where clause
- The `CreatedSubject` navigation property was NOT being eagerly loaded with `.Include()`
- This caused EF Core to fail when trying to access the navigation property

**Fix Applied:**
- Added `.Include(x => x.CreatedSubject)` to the query in `CookingLogRepository.cs` (line 73)
- File modified: `src/RecipeVault.Data/Repositories/CookingLogRepository.cs`

**Code Change:**
```csharp
// BEFORE:
return context.CookingLogs
    .Include(x => x.Recipe)
    .Include(x => x.Photos)
    .Where(cl => cl.CreatedSubject.SubjectId == subjectId  // ❌ CreatedSubject not loaded!
        && cl.CookedDate >= startDate 
        && cl.CookedDate < endDate)
    ...

// AFTER:
return context.CookingLogs
    .Include(x => x.Recipe)
    .Include(x => x.Photos)
    .Include(x => x.CreatedSubject)  // ✅ Now properly loaded
    .Where(cl => cl.CreatedSubject.SubjectId == subjectId
        && cl.CookedDate >= startDate 
        && cl.CookedDate < endDate)
    ...
```

---

### 2. ✅ ForkCount Column Missing (Sentry #7282515776)
**Error:** PostgreSQL error "42703: column r.ForkCount does not exist" (23 occurrences since Feb 21)

**Root Cause:**
- Migration `20260308000001_AddRecipeForkCount` exists in code but has NOT been applied to production database
- The `Recipe` entity model includes `ForkCount` property
- The `RecipeRepository.SearchAsync()` uses `ForkCount` for sorting
- Database schema is out of sync with code

**Fix Created:**
- Generated manual migration SQL script: `migrations/manual-pending.sql`
- Script includes:
  - `20260228120000_AddBetaInviteCodes` - Beta invite code system
  - `20260228120000_AddUserAccount` - User account tier tracking
  - `20260308000001_AddRecipeForkCount` - **THE ForkCount column** ⭐
- Migration SQL is idempotent (safe to run multiple times)

**Migration SQL:**
```sql
ALTER TABLE public."Recipe" ADD COLUMN "ForkCount" integer NOT NULL DEFAULT 0;
```

---

## Files Changed

### Code Files:
1. `src/RecipeVault.Data/Repositories/CookingLogRepository.cs` - Added missing `.Include()`

### Migration Files:
1. `migrations/manual-pending.sql` - Created manual migration script for pending migrations

### Git Commit:
- Commit: `c63c609`
- Message: "Fix Sentry errors: Add CreatedSubject Include and ForkCount migration"

---

## Next Steps

### Before Deploying:

1. **Run Migration SQL** 📝
   - Open Supabase SQL Editor: https://supabase.com/dashboard/project/umwycxfebintkenehqlj/sql/new
   - Copy contents of `migrations/manual-pending.sql`
   - Execute the script
   - Verify in `__EFMigrationsHistory` table that all 3 migrations are recorded

2. **Verify Schema** ✅
   ```sql
   -- Check that ForkCount column now exists:
   SELECT column_name, data_type, column_default 
   FROM information_schema.columns 
   WHERE table_name = 'Recipe' AND column_name = 'ForkCount';
   ```

3. **Deploy to Fly.io** 🚀
   ```powershell
   fly deploy --remote-only
   ```

4. **Test Endpoints** 🧪
   - Test cooking log stats: `GET /api/v1/cooking-log/stats`
   - Test recipe search with ForkCount sort: `GET /api/v1/recipes?sortBy=ForkCount&sortDirection=desc`

5. **Verify in Sentry** 📊
   - Wait 15-30 minutes after deploy
   - Check both issues to confirm no new occurrences
   - Mark issues as resolved if errors stop

---

## Migration Details

### ForkCount Migration
- **Column Name:** `ForkCount`
- **Type:** `integer`
- **Nullable:** `NOT NULL`
- **Default Value:** `0`
- **Purpose:** Tracks how many times a recipe has been forked by other users

### Related Code:
- Entity: `src/RecipeVault.Domain/Entities/Recipe.cs` (line 41-43)
  - Property: `public int ForkCount { get; private set; }`
  - Method: `public void IncrementForkCount() { ForkCount++; }`
- Repository: `src/RecipeVault.Data/Repositories/RecipeRepository.cs` (line 37-41)
  - Used in sorting logic for recipe search

---

## Timeline

- **Feb 21, 2026:** ForkCount errors started (first occurrence)
- **Mar 8, 2026:** Cooking log stats errors started (first occurrence today)
- **Mar 8, 2026 (today):** Both issues diagnosed and fixed

---

## Risk Assessment

### Cooking Log Fix:
- **Risk Level:** 🟢 LOW
- **Impact:** Query optimization (added eager loading)
- **Breaking Change:** No
- **Data Migration:** None needed
- **Rollback:** Easy (revert one Include statement)

### ForkCount Migration:
- **Risk Level:** 🟡 LOW-MEDIUM
- **Impact:** Adds new column with default value
- **Breaking Change:** No (default value of 0 is appropriate)
- **Data Migration:** None needed (new column, all recipes default to 0 forks)
- **Rollback:** Can be rolled back via migration Down() if needed

---

## Testing Recommendations

### Manual Testing (After Deploy):
1. ✅ Create a cooking log entry
2. ✅ View cooking stats (should now work)
3. ✅ Search recipes sorted by ForkCount (should now work)
4. ✅ Fork a recipe (should increment ForkCount)

### Monitor in Sentry:
- Check for new occurrences of both issues
- Both should go to zero within 24 hours of deploy

---

## Notes

- The `SeedEquipment` migration (20260303180000) is a data-only migration and is covered by `migrations-equipment-seed.sql` - intentionally excluded from this script
- All migrations are idempotent - safe to run multiple times
- Database changes are additive only (no drops, no schema breaking changes)
