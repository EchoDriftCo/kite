-- ============================================================================
-- Ingredient-First Search: PostgreSQL Scoring Function
-- ============================================================================
-- Apply this migration manually to your PostgreSQL database.
-- This creates the scoring function used by IngredientSearchService.
-- ============================================================================

-- ============================================================================
-- 1. User Pantry Items Table
-- ============================================================================

CREATE TABLE IF NOT EXISTS "UserPantryItem" (
    "UserPantryItemId" SERIAL PRIMARY KEY,
    "SubjectId" UUID NOT NULL,
    "IngredientName" VARCHAR(250) NOT NULL,
    "IsStaple" BOOLEAN NOT NULL DEFAULT false,
    "ExpirationDate" TIMESTAMP NULL,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedSubjectId" UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "LastModifiedDate" TIMESTAMP NULL,
    "LastModifiedSubjectId" UUID NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserPantryItem_SubjectId_IngredientName"
    ON "UserPantryItem"("SubjectId", "IngredientName");

CREATE INDEX IF NOT EXISTS "IX_UserPantryItem_SubjectId_IsStaple"
    ON "UserPantryItem"("SubjectId", "IsStaple");

-- ============================================================================
-- 2. Ingredient Synonym Table
-- ============================================================================

CREATE TABLE IF NOT EXISTS "IngredientSynonym" (
    "IngredientSynonymId" SERIAL PRIMARY KEY,
    "CanonicalName" VARCHAR(250) NOT NULL,
    "Synonym" VARCHAR(250) NOT NULL,
    "Source" INT NOT NULL DEFAULT 0  -- 0=Manual, 1=Imported, 2=UserSuggested
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_IngredientSynonym_Synonym" ON "IngredientSynonym"("Synonym");
CREATE INDEX IF NOT EXISTS "IX_IngredientSynonym_CanonicalName" ON "IngredientSynonym"("CanonicalName");
CREATE INDEX IF NOT EXISTS "IX_IngredientSynonym_Source" ON "IngredientSynonym"("Source");

-- Seed synonym data
INSERT INTO "IngredientSynonym" ("CanonicalName", "Synonym", "Source") VALUES
    ('chicken breast', 'chicken breasts', 0),
    ('chicken breast', 'boneless skinless chicken breast', 0),
    ('chicken breast', 'chicken cutlet', 0),
    ('ground beef', 'hamburger meat', 0),
    ('ground beef', 'minced beef', 0),
    ('ground beef', 'beef mince', 0),
    ('bell pepper', 'sweet pepper', 0),
    ('bell pepper', 'capsicum', 0),
    ('heavy cream', 'heavy whipping cream', 0),
    ('heavy cream', 'whipping cream', 0),
    ('green onion', 'scallion', 0),
    ('green onion', 'spring onion', 0),
    ('cilantro', 'coriander leaves', 0),
    ('cilantro', 'chinese parsley', 0),
    ('eggplant', 'aubergine', 0),
    ('zucchini', 'courgette', 0),
    ('shrimp', 'prawns', 0),
    ('shrimp', 'prawn', 0),
    ('corn', 'maize', 0),
    ('snow peas', 'mangetout', 0),
    ('arugula', 'rocket', 0),
    ('lima beans', 'butter beans', 0),
    ('garbanzo beans', 'chickpeas', 0),
    ('chickpeas', 'garbanzo beans', 0),
    ('baking soda', 'bicarbonate of soda', 0),
    ('confectioners sugar', 'powdered sugar', 0),
    ('confectioners sugar', 'icing sugar', 0),
    ('molasses', 'black treacle', 0),
    ('cookie', 'biscuit', 0),
    ('eggplant', 'brinjal', 0),
    ('rutabaga', 'swede', 0),
    ('potato', 'potatoes', 0),
    ('tomato', 'tomatoes', 0),
    ('onion', 'onions', 0),
    ('carrot', 'carrots', 0),
    ('apple', 'apples', 0),
    ('banana', 'bananas', 0),
    ('egg', 'eggs', 0),
    ('garlic clove', 'clove of garlic', 0),
    ('garlic clove', 'garlic cloves', 0),
    ('soy sauce', 'soya sauce', 0),
    ('soy sauce', 'shoyu', 0),
    ('sesame oil', 'toasted sesame oil', 0),
    ('rice vinegar', 'rice wine vinegar', 0),
    ('white vinegar', 'distilled vinegar', 0),
    ('ground cinnamon', 'cinnamon powder', 0),
    ('red pepper flakes', 'crushed red pepper', 0),
    ('red pepper flakes', 'chili flakes', 0),
    ('cayenne pepper', 'cayenne', 0),
    ('black pepper', 'ground black pepper', 0),
    ('kosher salt', 'coarse salt', 0),
    ('sea salt', 'flaky sea salt', 0),
    ('peanut butter', 'smooth peanut butter', 0),
    ('peanut butter', 'crunchy peanut butter', 0),
    ('tahini', 'sesame paste', 0)
ON CONFLICT DO NOTHING;

-- ============================================================================
-- 3. GIN Trigram Index on RecipeIngredient.Item (with fallback)
-- ============================================================================

DO $$
BEGIN
    -- Try to create pg_trgm extension
    CREATE EXTENSION IF NOT EXISTS pg_trgm;

    -- Create trigram index if extension exists
    IF EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') THEN
        CREATE INDEX IF NOT EXISTS "IX_RecipeIngredient_Item_Trgm"
            ON "RecipeIngredient" USING GIN ("Item" gin_trgm_ops);
        RAISE NOTICE 'pg_trgm extension enabled, trigram index created';
    ELSE
        -- Fall back to standard B-tree index for prefix matching
        CREATE INDEX IF NOT EXISTS "IX_RecipeIngredient_Item_Prefix"
            ON "RecipeIngredient"("Item" varchar_pattern_ops);
        RAISE WARNING 'pg_trgm extension not available, using ILIKE prefix matching';
    END IF;
END $$;

-- ============================================================================
-- 4. PostgreSQL Function: Score Recipes by Ingredients
-- ============================================================================

CREATE OR REPLACE FUNCTION score_recipes_by_ingredients(
    p_subject_id UUID,
    p_user_ingredients TEXT[],
    p_pantry_staples TEXT[]
)
RETURNS TABLE (
    recipe_id INT,
    matched_ingredients TEXT[],
    missing_ingredients TEXT[],
    pantry_staples_used TEXT[],
    match_percentage NUMERIC,
    weighted_match_percentage NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    WITH
    -- Expand user ingredients through synonyms
    expanded_user_ingredients AS (
        SELECT DISTINCT LOWER(TRIM(syn."CanonicalName")) AS ingredient
        FROM UNNEST(p_user_ingredients) AS user_ing
        LEFT JOIN "IngredientSynonym" syn ON LOWER(TRIM(syn."Synonym")) = LOWER(TRIM(user_ing))
        UNION
        SELECT DISTINCT LOWER(TRIM(user_ing)) AS ingredient
        FROM UNNEST(p_user_ingredients) AS user_ing
    ),
    -- Get all available ingredients (user + pantry staples)
    all_available AS (
        SELECT ingredient FROM expanded_user_ingredients
        UNION
        SELECT LOWER(TRIM(staple)) AS ingredient FROM UNNEST(p_pantry_staples) AS staple
    ),
    -- Tokenize recipe ingredients into words for word-boundary matching
    recipe_ingredients_tokenized AS (
        SELECT
            ri."RecipeId",
            ri."RecipeIngredientId",
            LOWER(TRIM(ri."Item")) AS item,
            ri."Quantity",
            ri."Preparation",
            -- Split item into words, removing punctuation
            regexp_split_to_array(
                regexp_replace(LOWER(TRIM(ri."Item")), '[^a-z0-9\s]', ' ', 'g'),
                '\s+'
            ) AS item_words
        FROM "RecipeIngredient" ri
        WHERE ri."Item" IS NOT NULL AND TRIM(ri."Item") <> ''
    ),
    -- Calculate ingredient importance weight
    recipe_ingredients_weighted AS (
        SELECT
            "RecipeId",
            "RecipeIngredientId",
            item,
            item_words,
            CASE
                -- Secondary ingredients: tiny quantities or garnish/optional notes
                WHEN "Preparation" ILIKE '%to taste%' OR "Preparation" ILIKE '%for garnish%'
                    OR "Preparation" ILIKE '%optional%' OR "Quantity" < 0.25 THEN 0.5
                -- Primary ingredients: first 4 ingredients or large quantities
                WHEN ROW_NUMBER() OVER (PARTITION BY "RecipeId" ORDER BY "RecipeIngredientId") <= 4
                    OR "Quantity" >= 2.0 THEN 2.0
                -- Default weight
                ELSE 1.0
            END AS weight
        FROM recipe_ingredients_tokenized
    ),
    -- Match recipe ingredients against available ingredients
    recipe_matches AS (
        SELECT
            rw."RecipeId",
            rw.item,
            rw.weight,
            CASE
                -- Exact match (case-insensitive)
                WHEN EXISTS (
                    SELECT 1 FROM all_available aa WHERE aa.ingredient = rw.item
                ) THEN 'matched'
                -- Word-boundary match: any word in recipe item matches a user ingredient
                WHEN EXISTS (
                    SELECT 1 FROM all_available aa, UNNEST(rw.item_words) AS word
                    WHERE aa.ingredient = word
                ) THEN 'matched'
                -- Pantry staple match
                WHEN EXISTS (
                    SELECT 1 FROM UNNEST(p_pantry_staples) AS staple
                    WHERE LOWER(TRIM(staple)) = rw.item
                ) THEN 'pantry'
                -- Not matched
                ELSE 'missing'
            END AS match_status
        FROM recipe_ingredients_weighted rw
    ),
    -- Aggregate results per recipe
    recipe_scores AS (
        SELECT
            rm."RecipeId",
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'matched') AS matched,
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'missing') AS missing,
            ARRAY_AGG(rm.item ORDER BY rm.item) FILTER (WHERE rm.match_status = 'pantry') AS pantry,
            COALESCE(ARRAY_LENGTH(ARRAY_AGG(rm.item) FILTER (WHERE rm.match_status = 'matched'), 1), 0)::NUMERIC AS matched_count,
            COALESCE(ARRAY_LENGTH(ARRAY_AGG(rm.item) FILTER (WHERE rm.match_status = 'missing'), 1), 0)::NUMERIC AS missing_count,
            COALESCE(SUM(rm.weight) FILTER (WHERE rm.match_status = 'matched'), 0)::NUMERIC AS weighted_matched,
            COALESCE(SUM(rm.weight) FILTER (WHERE rm.match_status = 'missing'), 0)::NUMERIC AS weighted_missing
        FROM recipe_matches rm
        GROUP BY rm."RecipeId"
    )
    -- Final output
    SELECT
        rs."RecipeId" AS recipe_id,
        COALESCE(rs.matched, ARRAY[]::TEXT[]) AS matched_ingredients,
        COALESCE(rs.missing, ARRAY[]::TEXT[]) AS missing_ingredients,
        COALESCE(rs.pantry, ARRAY[]::TEXT[]) AS pantry_staples_used,
        CASE
            WHEN (rs.matched_count + rs.missing_count) = 0 THEN 0
            ELSE ROUND((rs.matched_count / (rs.matched_count + rs.missing_count)) * 100, 1)
        END AS match_percentage,
        CASE
            WHEN (rs.weighted_matched + rs.weighted_missing) = 0 THEN 0
            ELSE ROUND((rs.weighted_matched / (rs.weighted_matched + rs.weighted_missing)) * 100, 1)
        END AS weighted_match_percentage
    FROM recipe_scores rs
    -- Only return recipes owned by user or public
    INNER JOIN "Recipe" r ON r."RecipeId" = rs."RecipeId"
    WHERE r."CreatedSubjectId" = p_subject_id OR r."IsPublic" = true;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION score_recipes_by_ingredients IS
'Scores recipes based on available ingredients. Uses word-boundary matching to avoid false positives (e.g., "rice" does not match "licorice"). Returns matched, missing, and pantry-used ingredients with both simple and weighted match percentages.';
