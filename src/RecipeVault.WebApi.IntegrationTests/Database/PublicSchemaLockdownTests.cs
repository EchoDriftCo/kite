using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using RecipeVault.Data.Migrations;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace RecipeVault.WebApi.IntegrationTests.Database {
    /// <summary>
    /// Runs the schema lockdown migrations against real Postgres set up the way Supabase sets up
    /// a project: <c>anon</c> and <c>authenticated</c> roles that get table, sequence and function
    /// privileges on everything created in <c>public</c>. The anon key ships in the frontend, so
    /// anything those roles can reach is reachable by anyone through Supabase's REST API.
    /// Needs Docker.
    /// </summary>
    public class PublicSchemaLockdownTests : IAsyncLifetime {
        private const string SupabaseLikeSetup = @"
            CREATE ROLE anon NOLOGIN;
            CREATE ROLE authenticated NOLOGIN;
            GRANT USAGE ON SCHEMA public TO anon, authenticated;
            ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO anon, authenticated;
            ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO anon, authenticated;
            ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO anon, authenticated;";

        // The Circle tables as production's hand-run SQL built them: no secondary indexes.
        private const string HandBuiltCircleTables = @"
            CREATE TABLE public.""Circle"" (""CircleId"" serial PRIMARY KEY, ""CreatedSubjectId"" uuid, ""LastModifiedSubjectId"" uuid);
            CREATE TABLE public.""CircleInvite"" (""CircleInviteId"" serial PRIMARY KEY,
                ""CircleId"" integer NOT NULL REFERENCES public.""Circle"" ON DELETE CASCADE);
            CREATE TABLE public.""CircleMember"" (""CircleMemberId"" serial PRIMARY KEY, ""SubjectId"" uuid NOT NULL);";

        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();

        public Task InitializeAsync() => _postgres.StartAsync();

        public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

        [Fact]
        public async Task FunctionCreatedAfterLockdown_IsNotExecutableByClientRoles() {
            await using var db = await OpenSupabaseLikeDatabaseAsync();
            await ApplyLockdownAsync(db);

            await ExecuteAsync(db, "CREATE FUNCTION public.probe() RETURNS int LANGUAGE sql SECURITY DEFINER AS 'SELECT 1';");

            foreach (var role in new[] { "anon", "authenticated" }) {
                (await ScalarAsync<bool>(db, $"SELECT has_function_privilege('{role}', 'public.probe()', 'EXECUTE')"))
                    .ShouldBeFalse($"{role} can execute a function created after the lockdown, so it's callable through /rest/v1/rpc");
            }
        }

        [Fact]
        public async Task TableCreatedAfterLockdown_IsNotAccessibleByClientRoles() {
            await using var db = await OpenSupabaseLikeDatabaseAsync();
            await ApplyLockdownAsync(db);

            await ExecuteAsync(db, "CREATE TABLE public.\"Probe\" (\"ProbeId\" serial PRIMARY KEY);");

            foreach (var role in new[] { "anon", "authenticated" }) {
                (await ScalarAsync<bool>(db, $"SELECT has_table_privilege('{role}', 'public.\"Probe\"', 'SELECT,INSERT,UPDATE,DELETE')"))
                    .ShouldBeFalse($"{role} has privileges on a table created after the lockdown");
                (await ScalarAsync<bool>(db, $"SELECT has_sequence_privilege('{role}', 'public.\"Probe_ProbeId_seq\"', 'USAGE,SELECT,UPDATE')"))
                    .ShouldBeFalse($"{role} has privileges on a sequence created after the lockdown");
            }
        }

        [Fact]
        public async Task HandBuiltCircleTables_GetTheModelsIndexes() {
            await using var db = await OpenSupabaseLikeDatabaseAsync();
            await ApplyLockdownAsync(db);

            var indexes = await ListAsync(db, "SELECT indexname FROM pg_indexes WHERE schemaname = 'public'");

            indexes.ShouldContain("IX_Circle_CreatedSubjectId");
            indexes.ShouldContain("IX_Circle_LastModifiedSubjectId");
            indexes.ShouldContain("IX_CircleInvite_CircleId");
            indexes.ShouldContain("IX_CircleMember_SubjectId");
        }

        [Fact]
        public async Task Lockdown_RerunsCleanly_OnSupabaseLikePostgres() {
            await using var db = await OpenSupabaseLikeDatabaseAsync();
            await ApplyLockdownAsync(db);
            await ApplyLockdownAsync(db);
        }

        [Fact]
        public async Task Lockdown_RerunsCleanly_OnPlainPostgres() {
            // No anon or authenticated roles on the server
            await using var db = await OpenDatabaseAsync(HandBuiltCircleTables);
            await ApplyLockdownAsync(db);
            await ApplyLockdownAsync(db);

            (await ScalarAsync<bool>(db, "SELECT bool_and(relrowsecurity) FROM pg_class WHERE relnamespace = 'public'::regnamespace AND relkind = 'r'"))
                .ShouldBeTrue();
        }

        private static async Task ApplyLockdownAsync(NpgsqlConnection db) {
            foreach (var sql in SqlOf(new LockDownPublicSchemaAccess()).Concat(SqlOf(new HardenPublicSchemaDefaults()))) {
                await ExecuteAsync(db, sql);
            }
        }

        private static IEnumerable<string> SqlOf(Migration migration) =>
            migration.UpOperations.OfType<SqlOperation>().Select(operation => operation.Sql);

        private Task<NpgsqlConnection> OpenSupabaseLikeDatabaseAsync() =>
            OpenDatabaseAsync(SupabaseLikeSetup + HandBuiltCircleTables);

        // xunit builds a new instance, so a new container, for each test: roles are server-wide,
        // and a plain-Postgres test must not see the ones a Supabase-like test created.
        private async Task<NpgsqlConnection> OpenDatabaseAsync(string setup) {
            var db = new NpgsqlConnection(_postgres.GetConnectionString());
            await db.OpenAsync();
            await ExecuteAsync(db, setup);
            return db;
        }

        private static async Task ExecuteAsync(NpgsqlConnection db, string sql) {
            await using var command = new NpgsqlCommand(sql, db);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<T> ScalarAsync<T>(NpgsqlConnection db, string sql) {
            await using var command = new NpgsqlCommand(sql, db);
            return (T)Convert.ChangeType(await command.ExecuteScalarAsync(), typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }

        private static async Task<List<string>> ListAsync(NpgsqlConnection db, string sql) {
            await using var command = new NpgsqlCommand(sql, db);
            await using var reader = await command.ExecuteReaderAsync();
            var values = new List<string>();
            while (await reader.ReadAsync()) {
                values.Add(reader.GetString(0));
            }
            return values;
        }
    }
}
