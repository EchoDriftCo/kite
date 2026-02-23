-- Migration: 20260223103002_RecordEFCoreMigrations
-- Description: Ensure EF Core migrations are recorded in history
-- Note: The EF migrations had schema issues but should be marked as "applied"
--       since we've manually fixed the schema

BEGIN;

DO $$
BEGIN
    -- Check if migration already applied
    IF EXISTS (
        SELECT 1 FROM public."__EFMigrationsHistory" 
        WHERE "MigrationId" = '20260223103002_RecordEFCoreMigrations'
    ) THEN
        RAISE NOTICE '⏭️  Migration 20260223103002_RecordEFCoreMigrations already applied - skipping';
        RETURN;
    END IF;

    RAISE NOTICE '🔧 Applying migration: 20260223103002_RecordEFCoreMigrations';

    -- Record the EF Core migrations that had issues
    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223023905_AddSocialCircles', '8.0.0')
    ON CONFLICT ("MigrationId") DO NOTHING;

    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223025127_FixCircleSubjectIdTypes', '8.0.0')
    ON CONFLICT ("MigrationId") DO NOTHING;

    RAISE NOTICE '   ✓ EF Core migrations recorded';

    -- Record this migration
    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223103002_RecordEFCoreMigrations', '8.0.0');

    RAISE NOTICE '✅ Migration 20260223103002_RecordEFCoreMigrations applied';
END $$;

COMMIT;
