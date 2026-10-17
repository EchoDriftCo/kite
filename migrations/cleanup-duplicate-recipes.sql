-- =====================================================================
-- Cleanup Duplicate Recipes (PostgreSQL / Supabase)
-- =====================================================================
-- Identifies and removes duplicate recipes (same Title + same CreatedSubjectId)
-- keeping the OLDEST one (smallest RecipeId) and deleting the rest.
--
-- Run Step 1 first (DRY RUN) to see what will be deleted.
-- Then run Step 2 in a transaction. Review results before COMMIT.
-- =====================================================================

-- =====================================================================
-- STEP 1: DRY RUN — Preview duplicates (run this first!)
-- =====================================================================
SELECT
  'WILL DELETE' AS action,
  d."RecipeId",
  d."Title",
  d."CreatedSubjectId",
  d."CreatedDate",
  keeper."RecipeId" AS keep_recipe_id,
  keeper."CreatedDate" AS keep_created_date,
  dup_info.duplicate_count
FROM public."Recipe" d
INNER JOIN (
  SELECT
    "Title",
    "CreatedSubjectId",
    MIN("RecipeId") AS oldest_recipe_id,
    COUNT(*) AS duplicate_count
  FROM public."Recipe"
  GROUP BY "Title", "CreatedSubjectId"
  HAVING COUNT(*) > 1
) dup_info
  ON d."Title" = dup_info."Title"
  AND d."CreatedSubjectId" = dup_info."CreatedSubjectId"
INNER JOIN public."Recipe" keeper
  ON keeper."RecipeId" = dup_info.oldest_recipe_id
WHERE d."RecipeId" != dup_info.oldest_recipe_id
ORDER BY d."Title", d."RecipeId" DESC;


-- =====================================================================
-- STEP 2: DELETE DUPLICATES (run after reviewing Step 1)
-- =====================================================================
-- Uncomment everything below to execute. Review, then COMMIT or ROLLBACK.

/*
BEGIN;

-- Build a temp table of recipe IDs to delete
CREATE TEMP TABLE duplicate_recipe_ids AS
SELECT d."RecipeId"
FROM public."Recipe" d
INNER JOIN (
  SELECT
    "Title",
    "CreatedSubjectId",
    MIN("RecipeId") AS oldest_recipe_id
  FROM public."Recipe"
  GROUP BY "Title", "CreatedSubjectId"
  HAVING COUNT(*) > 1
) dup_info
  ON d."Title" = dup_info."Title"
  AND d."CreatedSubjectId" = dup_info."CreatedSubjectId"
WHERE d."RecipeId" != dup_info.oldest_recipe_id;

-- Show count before deleting
SELECT COUNT(*) AS recipes_to_delete FROM duplicate_recipe_ids;

-- Delete child rows first (FK constraints)
DELETE FROM public."RecipeIngredient"   WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeInstruction"  WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeTag"          WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeEquipment"    WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeNutrition"    WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeLink"         WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."RecipeLink"         WHERE "LinkedRecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."CollectionRecipe"   WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."CircleRecipe"       WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."MealPlanEntry"      WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);
DELETE FROM public."CookingLog"         WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);

-- Delete the duplicate recipes
DELETE FROM public."Recipe" WHERE "RecipeId" IN (SELECT "RecipeId" FROM duplicate_recipe_ids);

-- Clean up temp table
DROP TABLE duplicate_recipe_ids;

-- Review, then:
-- COMMIT;
-- or ROLLBACK;
*/
