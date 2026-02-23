-- Migration: AddTagIsSystemTag
-- Date: 2026-02-22 18:09:00
-- Description: Adds IsSystemTag column to Tag table to track system-managed tags

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260222180900_AddTagIsSystemTag') THEN
    -- Add IsSystemTag column with default value of false
    ALTER TABLE public."Tag" ADD "IsSystemTag" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260222180900_AddTagIsSystemTag') THEN
    -- Set IsSystemTag to true for all existing global tags
    UPDATE public."Tag" SET "IsSystemTag" = TRUE WHERE "IsGlobal" = TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260222180900_AddTagIsSystemTag') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260222180900_AddTagIsSystemTag', '9.0.3');
    END IF;
END $EF$;
