-- Migration: 20260223103001_FixRecipeTagSubjectIdType
-- Description: Fix RecipeTag.AssignedBySubjectId from int to uuid
-- Issue: AlterColumn() in EF Core silently fails on PostgreSQL for int→uuid

BEGIN;

DO $$
DECLARE
    col_type TEXT;
    tag_count INT;
BEGIN
    -- Check if migration already applied
    IF EXISTS (
        SELECT 1 FROM public."__EFMigrationsHistory" 
        WHERE "MigrationId" = '20260223103001_FixRecipeTagSubjectIdType'
    ) THEN
        RAISE NOTICE '⏭️  Migration 20260223103001_FixRecipeTagSubjectIdType already applied - skipping';
        RETURN;
    END IF;

    RAISE NOTICE '🔧 Applying migration: 20260223103001_FixRecipeTagSubjectIdType';

    -- Check column type
    SELECT data_type INTO col_type
    FROM information_schema.columns
    WHERE table_schema = 'public'
      AND table_name = 'RecipeTag'
      AND column_name = 'AssignedBySubjectId';

    IF col_type IS NULL THEN
        RAISE NOTICE '   RecipeTag.AssignedBySubjectId does not exist - skipping';
    ELSIF col_type = 'uuid' THEN
        RAISE NOTICE '   RecipeTag.AssignedBySubjectId is already uuid - skipping';
    ELSIF col_type IN ('integer', 'bigint') THEN
        RAISE NOTICE '   RecipeTag.AssignedBySubjectId is % - needs fix', col_type;

        -- Check for data
        SELECT COUNT(*) INTO tag_count FROM public."RecipeTag";
        
        IF tag_count > 0 THEN
            RAISE EXCEPTION '❌ RecipeTag has % rows - manual data migration required', tag_count;
        END IF;

        RAISE NOTICE '   Table is empty - fixing column type...';
        
        ALTER TABLE public."RecipeTag" DROP COLUMN "AssignedBySubjectId";
        ALTER TABLE public."RecipeTag" ADD COLUMN "AssignedBySubjectId" uuid NOT NULL;

        RAISE NOTICE '   ✓ RecipeTag.AssignedBySubjectId fixed to uuid';
    ELSE
        RAISE WARNING '   RecipeTag.AssignedBySubjectId has unexpected type: %', col_type;
    END IF;

    -- Record migration
    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223103001_FixRecipeTagSubjectIdType', '8.0.0');

    RAISE NOTICE '✅ Migration 20260223103001_FixRecipeTagSubjectIdType applied';
END $$;

COMMIT;
