-- Migration: AddSocialCircles
-- Date: 2026-02-23 01:00:00
-- Description: Adds Circle, CircleMember, CircleRecipe, and CircleInvite tables for Social Circles feature

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223010000_AddSocialCircles') THEN
    -- Create Circle table
    CREATE TABLE public."Circle" (
        "CircleId" SERIAL PRIMARY KEY,
        "CircleResourceId" uuid NOT NULL,
        "Name" varchar(100) NOT NULL,
        "Description" varchar(500),
        "OwnerSubjectId" integer NOT NULL,
        "CreateDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
        "CreatedSubjectId" uuid NOT NULL,
        "LastModifiedDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
        "LastModifiedSubjectId" uuid NOT NULL
    );

    CREATE UNIQUE INDEX "IX_Circle_CircleResourceId" ON public."Circle" ("CircleResourceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223010000_AddSocialCircles') THEN
    -- Create CircleMember table
    CREATE TABLE public."CircleMember" (
        "CircleMemberId" SERIAL PRIMARY KEY,
        "CircleId" integer NOT NULL,
        "SubjectId" integer NOT NULL,
        "Role" integer NOT NULL,
        "Status" integer NOT NULL,
        "JoinedDate" timestamp,
        "InvitedDate" timestamp,
        CONSTRAINT "FK_CircleMember_Circle_CircleId" FOREIGN KEY ("CircleId") 
            REFERENCES public."Circle" ("CircleId") ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX "IX_CircleMember_CircleId_SubjectId" ON public."CircleMember" ("CircleId", "SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223010000_AddSocialCircles') THEN
    -- Create CircleRecipe table
    CREATE TABLE public."CircleRecipe" (
        "CircleRecipeId" SERIAL PRIMARY KEY,
        "CircleId" integer NOT NULL,
        "RecipeId" integer NOT NULL,
        "SharedBySubjectId" integer NOT NULL,
        "SharedDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
        CONSTRAINT "FK_CircleRecipe_Circle_CircleId" FOREIGN KEY ("CircleId") 
            REFERENCES public."Circle" ("CircleId") ON DELETE CASCADE,
        CONSTRAINT "FK_CircleRecipe_Recipe_RecipeId" FOREIGN KEY ("RecipeId") 
            REFERENCES public."Recipe" ("RecipeId") ON DELETE RESTRICT
    );

    CREATE UNIQUE INDEX "IX_CircleRecipe_CircleId_RecipeId" ON public."CircleRecipe" ("CircleId", "RecipeId");
    CREATE INDEX "IX_CircleRecipe_RecipeId" ON public."CircleRecipe" ("RecipeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223010000_AddSocialCircles') THEN
    -- Create CircleInvite table
    CREATE TABLE public."CircleInvite" (
        "CircleInviteId" SERIAL PRIMARY KEY,
        "InviteToken" uuid NOT NULL,
        "CircleId" integer NOT NULL,
        "InviteeEmail" varchar(255),
        "InvitedBySubjectId" integer NOT NULL,
        "CreatedDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
        "ExpiresDate" timestamp NOT NULL,
        "Status" integer NOT NULL,
        CONSTRAINT "FK_CircleInvite_Circle_CircleId" FOREIGN KEY ("CircleId") 
            REFERENCES public."Circle" ("CircleId") ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX "IX_CircleInvite_InviteToken" ON public."CircleInvite" ("InviteToken");
    CREATE INDEX "IX_CircleInvite_CircleId" ON public."CircleInvite" ("CircleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223010000_AddSocialCircles') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223010000_AddSocialCircles', '9.0.3');
    END IF;
END $EF$;
