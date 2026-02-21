# Database Migrations with Supabase

## The Problem

EF Core migrations can't run directly against Supabase from local dev:
- Connection pooler timeouts kill long-running migration commands
- Direct connections from outside Supabase's network are unreliable
- Manual SQL execution in Supabase SQL Editor is error-prone (we've had prod outages from missed columns)

## Solution: Idempotent Migration Scripts + Nora

**When Nora is available (browser relay attached):** She can run migrations directly in Supabase SQL Editor. Just ask her to deploy and she'll handle everything.

**When Nora is unavailable:** Use `deploy.ps1` which guides you through the manual process.

### Workflow

1. **Generate migration** (as usual):
   ```powershell
   cd src\RecipeVault.Data
   dotnet ef migrations add <MigrationName> -s ..\RecipeVault.WebApi
   ```

2. **Generate idempotent SQL script**:
   ```powershell
   .\generate-migration-sql.ps1
   ```
   This creates `migrations/pending.sql` with all unapplied migrations.

3. **Review the SQL** — sanity check before running

4. **Run in Supabase SQL Editor**:
   - Go to Supabase Dashboard → SQL Editor
   - Paste contents of `migrations/pending.sql`
   - Execute

5. **Deploy code** — now the schema matches

### Key Files

- `migrations/pending.sql` — Generated SQL for pending migrations (gitignored)
- `migrations/applied/` — Archive of applied migration scripts (for reference)
- `generate-migration-sql.ps1` — Helper script to generate SQL

### Safety Features

- **Idempotent scripts**: Safe to run multiple times (uses `IF NOT EXISTS` checks)
- **Transaction wrapped**: All-or-nothing application
- **EF history tracking**: Records applied migrations in `__EFMigrationsHistory`

### Deploying (Recommended)

Use the deploy script which handles everything:

```powershell
.\deploy.ps1
```

This will:
1. Check for pending migrations
2. Generate SQL and offer to copy to clipboard
3. Require confirmation that you ran the SQL
4. Build and deploy to Fly.io

### Manual Pre-Deploy Checklist

If deploying manually (`fly deploy`):
- [ ] Run `.\generate-migration-sql.ps1`
- [ ] Check if `migrations/pending.sql` has content
- [ ] If yes, run SQL in Supabase BEFORE deploying
- [ ] Then deploy to Fly.io

### Emergency Rollback

If a migration breaks prod:
1. Check the migration's `Down()` method
2. Generate rollback SQL: `dotnet ef migrations script <CurrentMigration> <PreviousMigration> -i`
3. Run in Supabase SQL Editor
4. Redeploy previous code version

## Why Not Alternatives?

| Approach | Problem |
|----------|---------|
| Run migrations on app startup | Risky for prod, multiple instances race |
| GitHub Actions | Needs secure DB credentials in CI, complex setup |
| Supabase CLI | Doesn't integrate with EF Core |
| Direct connection | Timeouts, unreliable |

The manual SQL approach with idempotent scripts is the most reliable for our setup.
