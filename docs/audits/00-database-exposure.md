# Section 0: Database exposure

Audited 2026-10-18 against the live Supabase project `RecipeVault` (`umwycxfebintkenehqlj`, us-west-2), after PR #7 deployed.

## Scope

- Table, sequence and function privileges for Supabase's `anon` and `authenticated` roles, in every schema
- Row level security and policies
- Default privileges, meaning what new objects will get
- Data API (PostgREST) exposure, Realtime publications, GraphQL
- Storage buckets, objects and policies
- Auth: providers, anonymous users, Supabase's security advisor
- Supabase's performance advisor, and production's indexes compared with the EF model
- Code: `LockDownPublicSchemaAccess`, `SupabaseService` (frontend), `SupabaseStorageClient`, `deploy.yml`

## Confirmed safe

| Check | Evidence |
|-------|----------|
| All 36 `public` tables have RLS on, and neither client role has any table privilege | `has_table_privilege` for both roles on every table: all false |
| `LockDownPublicSchemaAccess` is applied and recorded in production | `__EFMigrationsHistory` lists it after the PR #7 deploy |
| No functions in `public` | `pg_proc` is empty for `public`, so there's nothing callable through `/rpc` today |
| GraphQL is off | `pg_graphql` isn't installed |
| No tables are published to Realtime | `pg_publication_tables` for `supabase_realtime` is empty |
| Storage can't be listed or written with the anon key | RLS is on for `storage.objects` and there are no storage policies, so all 13 objects were written by the API's service key |
| The frontend uses Supabase only for sign-in | `ui/src` calls only `supabase.client.auth.*`; no `.from()`, `.rpc()` or `.storage`. The same holds on `feature/browser-extension` and `feature/pwa`. |
| Tables outside `public` that `anon` can read | Only Supabase's own `extensions.pg_stat_statements` and `realtime.subscription`. The Data API doesn't serve `extensions`, and Postgres hides other roles' query text. No action. |

## Findings

### D0-1 · Medium · New functions would be callable by anyone, with RLS bypassed if `SECURITY DEFINER` (fixed in this PR)

`LockDownPublicSchemaAccess` revoked the client roles' default privileges on tables and sequences, but not on functions. Two defaults remain:

- **Postgres itself:** grants `EXECUTE` on every new function to `PUBLIC`.
- **Supabase:** also grants it to `anon` and `authenticated` in `public`.

So the first function anyone adds to `public`, through a migration, the SQL editor or an extension, is callable at `/rest/v1/rpc/<name>` with the anon key. A `SECURITY DEFINER` function also bypasses RLS. Nothing is exposed today, because there are no functions.

- **Proof:** `pg_default_acl` holds `{postgres=X, anon=X, authenticated=X, service_role=X}` for functions in `public`. `PublicSchemaLockdownTests.FunctionCreatedAfterLockdown_IsNotExecutableByClientRoles` creates a function after both lockdown migrations. **It fails without this PR's migration.**
- **Fix:** migration `HardenPublicSchemaDefaults` runs `ALTER DEFAULT PRIVILEGES REVOKE EXECUTE ON FUNCTIONS FROM PUBLIC`, plus the same for `anon` and `authenticated` in `public`, for the role that runs migrations.
- **Remaining:** objects that `supabase_admin` creates still get Supabase's grants, because only that role can change its own defaults. Only Supabase internals run as it. Settings item S1 closes this class of problem entirely.

### D0-2 · Low · Four model indexes were never built on the hand-made Circle tables (fixed in this PR)

Production's Circle tables were created by hand-run SQL (`migrations/20260223010000_AddSocialCircles.sql`) and then recorded as migrated. That SQL left out four indexes that the model defines:

- `IX_CircleInvite_CircleId`
- `IX_CircleMember_SubjectId`
- `IX_Circle_CreatedSubjectId`
- `IX_Circle_LastModifiedSubjectId`

Supabase's performance advisor flagged the first one as an unindexed foreign key.

- **Proof:** the index names in `RecipeVaultDbContextModelSnapshot.cs` compared with production's `pg_indexes`. These four are the only ones missing. `HandBuiltCircleTables_GetTheModelsIndexes` **fails without the fix**.
- **Fix:** the same migration runs `CREATE INDEX IF NOT EXISTS` for all four. That's a no-op on databases built by EF's `AddSocialCircles`. The tables are tiny, so the non-concurrent build takes no noticeable lock.

### D0-3 · Medium · The Data API is on and serves `public`, though nothing uses it (settings)

The lockdown makes PostgREST return nothing, so this isn't exploitable today. But the anon key is public, so a single mistaken `GRANT` or a new table that someone gives a policy would expose that table again. The app doesn't need the Data API (see "Confirmed safe"). → Settings item **S1**.

### D0-4 · Medium · The `recipe-images` bucket has no size limit or file-type restriction (settings)

The bucket is public, with `file_size_limit` and `allowed_mime_types` both unset. The largest object is 7.9 MB. Only the API can upload, so these limits are a backstop. The API's own upload checks are part of Section 2. Public read is intended: the images are served by URL. → Settings item **S2**.

### D0-5 · Low · Leaked-password protection is off (settings)

This comes from Supabase's security advisor (`auth_leaked_password_protection`). The local `supabase/config.toml` also sets `minimum_password_length = 6`. I can't read production's auth settings with the tools I have, so that one needs confirming. → Settings item **S3**.

### D0-6 · Low · Guest (anonymous) sign-ins have no CAPTCHA (settings + code; lead for Section 2)

Anonymous sign-ins are on, and the frontend sends no captcha token. Supabase's default rate limit is 30 anonymous sign-ins per hour per IP, and each one gives a session the API accepts, with its own `GuestQuota` allowance. A script rotating IPs can therefore mint guests to multiply the Gemini quota.

There's no abuse today: there is 1 anonymous user out of 12. Turning on CAPTCHA (Turnstile) needs a settings change and a frontend change that passes `captchaToken`, so it belongs with Section 2's cost controls. → Settings item **S4**.

### Not acted on

- **55 "unused index" notices:** with 12 users, the usage statistics mean nothing. Don't drop them.
- **"Auth DB connection strategy is absolute":** only matters once the instance size grows.

## Found in passing: the migrations can't build a fresh database (for Section 3)

To compare against production, I ran `dotnet ef migrations script --idempotent` on an empty Postgres 17. It fails, and `ON_ERROR_STOP` rolls the whole script back. These are the first three breaks; there may be more:

1. **`20260212181918_SeedGlobalTags`:** a bare `SELECT setval(...)` inside EF's `DO` block fails with "query has no destination for result data" (script line 883).
2. **`20260221192539_AddRecipeTagDetail`:** runs `DROP TABLE "UserTagAlias"`, a table no EF migration creates; only hand-run SQL did.
3. **`20261018150500_SeedSampleRecipes`:** inserts `'Free'` into `UserAccount.AccountTier`, which the EF migrations create as `integer`. Production has `varchar`, because the enum-to-string change was applied by hand and its migration was later removed (f8fe48a). Other enum columns differ the same way.

Production deploys are unaffected, because every one of these migrations is already recorded there and gets skipped. But there's no working path to a new database: disaster recovery, staging, a QA database or a new developer. PR #7's claim that "a fresh database starts the same way" doesn't hold yet.

Also seen: the hand-built Circle tables are missing the model's foreign keys to `Subject`.

**Section 3 should start here:** decide whether to rebaseline the migrations from production's schema, then add a CI job that builds a database from scratch.

## This PR

- `src/RecipeVault.Data/Migrations/20261018200753_HardenPublicSchemaDefaults.cs`: fixes D0-1 and D0-2.
- `src/RecipeVault.WebApi.IntegrationTests/Database/PublicSchemaLockdownTests.cs`: 5 tests against Postgres 17 in a container via Testcontainers, so they need Docker; GitHub's Ubuntu runners have it.
  - **Fail without the fix:** the function test and the index test.
  - **Regression tests for PR #7:** a table and sequence created after the lockdown stay closed.
  - **Idempotence:** the migrations rerun cleanly on Supabase-like Postgres and on plain Postgres.
- **Deploy path:** the idempotent script from `SeedSampleRecipes` onward, applied twice with `psql -v ON_ERROR_STOP=1` to a database shaped like production. Both runs exit 0, both migrations are recorded, the four indexes exist, and `anon` can't execute a function created afterwards.

## Settings only you can change

| # | Where | Change | Why |
|---|-------|--------|-----|
| S1 | Dashboard → Project Settings → Data API | Turn the Data API off, or at least remove `public` (and `graphql_public`) from the exposed schemas. Sign-in keeps working; it's a separate service. | D0-3, and closes the rest of D0-1 |
| S2 | Dashboard → Storage → `recipe-images` → Edit bucket | Set a file size limit (10 MB covers the largest existing object) and allowed types `image/jpeg, image/png, image/gif, image/webp`, the types `RecipesController` maps. Optionally delete the two test objects, `test-upload.png` and `test-20260220184938.png`. | D0-4 |
| S3 | Dashboard → Authentication → Providers → Email (password settings) | Turn on leaked-password protection, which may need the Pro plan. Set the minimum password length to at least 8 if it's currently 6. | D0-5 |
| S4 | Dashboard → Authentication → Attack Protection | Turn on CAPTCHA (Turnstile) only together with the frontend change; turning it on first would break sign-in. Section 2 can pair them. | D0-6 |
