-- Migration: AddUserTagAliasesAndSourceType
-- Run this in Supabase SQL Editor

-- Add new columns to Tag table
ALTER TABLE public."Tag" ADD COLUMN IF NOT EXISTS "IsSystemTag" boolean NOT NULL DEFAULT false;
ALTER TABLE public."Tag" ADD COLUMN IF NOT EXISTS "SourceType" integer NULL;

-- Create UserTagAlias table
CREATE TABLE IF NOT EXISTS public."UserTagAlias" (
    "UserTagAliasId" SERIAL PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "TagId" integer NOT NULL,
    "Alias" character varying(100) NOT NULL,
    "NormalizedEntityId" character varying(100) NULL,
    "NormalizedEntityType" integer NULL,
    "ShowAliasPublicly" boolean NOT NULL DEFAULT false,
    "CreatedDate" timestamp with time zone NOT NULL,
    "CreatedSubjectId" uuid NOT NULL,
    "LastModifiedDate" timestamp with time zone NOT NULL,
    "LastModifiedSubjectId" uuid NOT NULL,
    CONSTRAINT "FK_UserTagAlias_Tag_TagId" FOREIGN KEY ("TagId") 
        REFERENCES public."Tag"("TagId") ON DELETE CASCADE,
    CONSTRAINT "FK_UserTagAlias_Subject_CreatedSubjectId" FOREIGN KEY ("CreatedSubjectId")
        REFERENCES public."Subject"("SubjectId"),
    CONSTRAINT "FK_UserTagAlias_Subject_LastModifiedSubjectId" FOREIGN KEY ("LastModifiedSubjectId")
        REFERENCES public."Subject"("SubjectId")
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_UserTagAlias_CreatedSubjectId" ON public."UserTagAlias"("CreatedSubjectId");
CREATE INDEX IF NOT EXISTS "IX_UserTagAlias_LastModifiedSubjectId" ON public."UserTagAlias"("LastModifiedSubjectId");
CREATE INDEX IF NOT EXISTS "IX_UserTagAlias_TagId" ON public."UserTagAlias"("TagId");
CREATE INDEX IF NOT EXISTS "IX_UserTagAlias_UserId" ON public."UserTagAlias"("UserId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserTagAlias_UserId_TagId" ON public."UserTagAlias"("UserId", "TagId");

-- Backfill IsSystemTag for existing global tags
UPDATE public."Tag" SET "IsSystemTag" = true WHERE "IsGlobal" = true;

-- Insert new Source system tags (Category 4 = Source)
INSERT INTO public."Tag" ("TagId", "TagResourceId", "Name", "Category", "IsGlobal", "IsSystemTag", "SourceType", "CreatedDate", "CreatedSubjectId", "LastModifiedDate", "LastModifiedSubjectId")
VALUES 
    (38, gen_random_uuid(), 'Family Recipe', 4, true, true, 1, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001'),
    (39, gen_random_uuid(), 'Chef', 4, true, true, 2, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001'),
    (40, gen_random_uuid(), 'Restaurant', 4, true, true, 3, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001'),
    (41, gen_random_uuid(), 'Cookbook', 4, true, true, 4, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001'),
    (42, gen_random_uuid(), 'Website', 4, true, true, 5, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001'),
    (43, gen_random_uuid(), 'Original Creation', 4, true, true, 6, NOW(), '00000000-0000-0000-0000-000000000001', NOW(), '00000000-0000-0000-0000-000000000001')
ON CONFLICT ("TagId") DO NOTHING;

-- Reset sequence for future user-created tags
SELECT setval('public."Tag_TagId_seq"', 100, false);

-- Record migration in EF Core history
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260221163106_AddUserTagAliasesAndSourceType', '8.0.1')
ON CONFLICT DO NOTHING;
