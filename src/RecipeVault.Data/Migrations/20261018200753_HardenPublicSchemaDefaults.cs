using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <summary>
    /// Closes two gaps left after <see cref="LockDownPublicSchemaAccess"/>.
    ///
    /// Functions: that migration revoked the client roles' default privileges on tables and
    /// sequences, but not on functions. Postgres grants EXECUTE on every new function to PUBLIC,
    /// and Supabase also grants it to <c>anon</c> and <c>authenticated</c> in <c>public</c>, so a
    /// function added later would be callable by anyone through Supabase's REST API
    /// (<c>/rest/v1/rpc/...</c>) with the anon key that ships in the frontend. A SECURITY DEFINER
    /// function would also bypass RLS. This revokes both defaults for functions created by the
    /// role that runs migrations.
    ///
    /// Indexes: the Circle tables in production were created by hand-run SQL and then recorded as
    /// migrated, so four indexes the model defines were never built (Supabase's performance advisor
    /// flagged the unindexed CircleInvite foreign key). They're created here if missing; databases
    /// built by <c>AddSocialCircles</c> already have them.
    ///
    /// Idempotent, and a no-op for the role statements on plain Postgres, where the Supabase roles
    /// don't exist.
    /// </summary>
    public partial class HardenPublicSchemaDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER DEFAULT PRIVILEGES REVOKE EXECUTE ON FUNCTIONS FROM PUBLIC;

                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon')
                       AND EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE EXECUTE ON FUNCTIONS FROM anon, authenticated;
                    END IF;
                END $$;

                CREATE INDEX IF NOT EXISTS ""IX_Circle_CreatedSubjectId"" ON public.""Circle"" (""CreatedSubjectId"");
                CREATE INDEX IF NOT EXISTS ""IX_Circle_LastModifiedSubjectId"" ON public.""Circle"" (""LastModifiedSubjectId"");
                CREATE INDEX IF NOT EXISTS ""IX_CircleInvite_CircleId"" ON public.""CircleInvite"" (""CircleId"");
                CREATE INDEX IF NOT EXISTS ""IX_CircleMember_SubjectId"" ON public.""CircleMember"" (""SubjectId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately empty: rolling back must not reopen functions to the public roles, and
            // the indexes belong to the model.
        }
    }
}
