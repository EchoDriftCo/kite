-- Migration: 20260223103003_VerifySubjectIdTypes
-- Description: Verify all SubjectId columns are uuid type
-- Run this last to confirm everything is correct

BEGIN;

DO $$
DECLARE
    wrong_count INT;
    wrong_cols TEXT;
BEGIN
    -- Check if migration already applied
    IF EXISTS (
        SELECT 1 FROM public."__EFMigrationsHistory" 
        WHERE "MigrationId" = '20260223103003_VerifySubjectIdTypes'
    ) THEN
        RAISE NOTICE '⏭️  Migration 20260223103003_VerifySubjectIdTypes already applied - skipping';
        RETURN;
    END IF;

    RAISE NOTICE '🔍 Verifying SubjectId column types...';

    -- Check for any remaining int SubjectId columns
    SELECT COUNT(*), string_agg(table_name || '.' || column_name, ', ')
    INTO wrong_count, wrong_cols
    FROM information_schema.columns
    WHERE table_schema = 'public'
      AND column_name LIKE '%SubjectId%'
      AND data_type IN ('integer', 'bigint');

    IF wrong_count > 0 THEN
        RAISE EXCEPTION '❌ VERIFICATION FAILED: % columns still have wrong type: %', wrong_count, wrong_cols;
    END IF;

    RAISE NOTICE '   ✓ All SubjectId columns are uuid';

    -- Record this migration
    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223103003_VerifySubjectIdTypes', '8.0.0');

    RAISE NOTICE '✅ Migration 20260223103003_VerifySubjectIdTypes applied';
END $$;

-- Show final state
SELECT 
    table_name,
    column_name,
    data_type,
    CASE 
        WHEN data_type = 'uuid' THEN '✓'
        WHEN data_type IN ('integer', 'bigint') THEN '✗ WRONG'
        ELSE '?'
    END as status
FROM information_schema.columns
WHERE table_schema = 'public'
  AND column_name LIKE '%SubjectId%'
ORDER BY table_name, column_name;

COMMIT;
