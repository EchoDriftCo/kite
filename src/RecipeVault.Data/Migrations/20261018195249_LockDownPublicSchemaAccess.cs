using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <summary>
    /// Closes the public schema to Supabase's client roles.
    ///
    /// Supabase grants its <c>anon</c> and <c>authenticated</c> roles full access to new tables in
    /// <c>public</c> and serves them over its REST API (PostgREST). The anon key ships in the
    /// frontend by design, so on any table without row level security anyone could read, insert,
    /// update or delete rows directly, bypassing the API. 26 tables were open that way
    /// (ApiToken, UserAccount, BetaInviteCode, CircleInvite, ...).
    ///
    /// The frontend only uses Supabase for sign-in and the API connects as the tables' owner,
    /// which bypasses RLS, so neither needs these grants. This enables RLS on every public table
    /// (with no policies, the client roles are denied) and revokes their privileges, including on
    /// tables created later. It was first applied by hand on 2026-10-18; this makes it part of
    /// the schema. Idempotent, and a no-op for the role statements on plain Postgres, where the
    /// Supabase roles don't exist.
    /// </summary>
    public partial class LockDownPublicSchemaAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE r record;
                BEGIN
                    FOR r IN SELECT c.relname FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                             WHERE n.nspname = 'public' AND c.relkind = 'r' AND NOT c.relrowsecurity LOOP
                        EXECUTE format('ALTER TABLE public.%I ENABLE ROW LEVEL SECURITY', r.relname);
                    END LOOP;

                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon')
                       AND EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
                        REVOKE ALL ON ALL TABLES IN SCHEMA public FROM anon, authenticated;
                        REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM anon, authenticated;
                        REVOKE EXECUTE ON ALL FUNCTIONS IN SCHEMA public FROM anon, authenticated;
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON TABLES FROM anon, authenticated;
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON SEQUENCES FROM anon, authenticated;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately empty: rolling back must not reopen the tables to the public roles.
        }
    }
}
