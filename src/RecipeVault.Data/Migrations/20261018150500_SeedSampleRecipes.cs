using System;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <summary>
    /// Seeds the onboarding sample recipes under the system subject, replacing
    /// OnboardingSeedService, which ran on every app startup.
    ///
    /// That service checked for existing samples owned by the system subject, but
    /// Cortside's auditable interceptor stamps CreatedSubject from the request
    /// principal — and at startup there is no request, so every seeded recipe was
    /// owned by Guid.Empty. The check never matched, and each cold start on Fly
    /// (auto_stop_machines, min_machines_running = 0) inserted another 6 recipes
    /// with their ingredients and instructions.
    ///
    /// Every statement here is idempotent:
    ///   1. ensure the system Subject and UserAccount exist
    ///   2. adopt the oldest existing Guid.Empty-owned copy of each sample, so a
    ///      database that already has them is not seeded a second time
    ///   3. insert any sample that is still missing (a fresh database)
    ///   4. a unique index so the system-owned set cannot multiply again
    /// </summary>
    public partial class SeedSampleRecipes : Migration
    {
        private static readonly Guid SystemSubjectId = SampleRecipeSeedData.SystemSubjectId;
        private const string SeedDate = "2026-10-18 00:00:00+00";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var definitions = SampleRecipeSeedData.GetSampleRecipes();

            migrationBuilder.Sql($@"
                INSERT INTO public.""Subject"" (""SubjectId"", ""Name"", ""CreatedDate"")
                VALUES ('{SystemSubjectId}', 'Sample Recipes', '{SeedDate}')
                ON CONFLICT (""SubjectId"") DO NOTHING;
            ");

            migrationBuilder.Sql($@"
                INSERT INTO public.""UserAccount"" (""UserAccountResourceId"", ""SubjectId"", ""AccountTier"",
                    ""HasCompletedOnboarding"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                SELECT gen_random_uuid(), '{SystemSubjectId}', 'Free', false,
                    '{SeedDate}', '{SystemSubjectId}', '{SeedDate}', '{SystemSubjectId}'
                WHERE NOT EXISTS (SELECT 1 FROM public.""UserAccount"" WHERE ""SubjectId"" = '{SystemSubjectId}');
            ");

            // The keeper is the lowest RecipeId, matching the duplicate cleanup script.
            var titles = string.Join(", ", definitions.Select(d => Literal(d.Title)));
            migrationBuilder.Sql($@"
                WITH keepers AS (
                    SELECT DISTINCT ON (""Title"") ""RecipeId"", ""Title""
                    FROM public.""Recipe""
                    WHERE ""IsSampleRecipe""
                      AND ""CreatedSubjectId"" = '{Guid.Empty}'
                      AND ""Title"" IN ({titles})
                    ORDER BY ""Title"", ""RecipeId""
                )
                UPDATE public.""Recipe"" r
                SET ""CreatedSubjectId"" = '{SystemSubjectId}', ""LastModifiedSubjectId"" = '{SystemSubjectId}'
                FROM keepers k
                WHERE r.""RecipeId"" = k.""RecipeId""
                  AND NOT EXISTS (
                      SELECT 1 FROM public.""Recipe"" s
                      WHERE s.""IsSampleRecipe"" AND s.""CreatedSubjectId"" = '{SystemSubjectId}' AND s.""Title"" = k.""Title"");
            ");

            foreach (var def in definitions)
            {
                InsertSampleRecipe(migrationBuilder, def);
            }

            migrationBuilder.Sql($@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""UX_Recipe_SystemSampleTitle""
                ON public.""Recipe"" (""Title"")
                WHERE ""IsSampleRecipe"" AND ""CreatedSubjectId"" = '{SystemSubjectId}';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The recipes stay: users fork them, and their forks point back at them.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS public.""UX_Recipe_SystemSampleTitle"";");
        }

        private static void InsertSampleRecipe(MigrationBuilder migrationBuilder, SampleRecipeDefinition def)
        {
            var title = Literal(def.Title);
            var owned = $@"r.""IsSampleRecipe"" AND r.""CreatedSubjectId"" = '{SystemSubjectId}' AND r.""Title"" = {title}";

            migrationBuilder.Sql($@"
                INSERT INTO public.""Recipe"" (""RecipeResourceId"", ""Title"", ""Description"", ""Yield"",
                    ""PrepTimeMinutes"", ""CookTimeMinutes"", ""ForkCount"", ""IsFavorite"", ""IsPublic"",
                    ""IsSampleRecipe"", ""ShowcaseFeature"",
                    ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                SELECT gen_random_uuid(), {title}, {Literal(def.Description)}, {def.Yield},
                    {def.PrepTimeMinutes}, {def.CookTimeMinutes}, 0, false, true,
                    true, {Literal(def.Showcases)},
                    '{SeedDate}', '{SystemSubjectId}', '{SeedDate}', '{SystemSubjectId}'
                WHERE NOT EXISTS (SELECT 1 FROM public.""Recipe"" r WHERE {owned});
            ");

            // Children are only added to a recipe that has none, so an adopted copy
            // keeps the ingredients and instructions it already has.
            var ingredients = string.Join(",\n                    ", def.Ingredients.Select(i =>
                $"({i.SortOrder}, {Numeric(i.Quantity)}, {Literal(i.Unit)}::text, {Literal(i.Item)}::text, " +
                $"{Literal(i.Preparation)}::text, {Literal(i.RawText)}::text)"));
            migrationBuilder.Sql($@"
                INSERT INTO public.""RecipeIngredient"" (""RecipeId"", ""SortOrder"", ""Quantity"", ""Unit"", ""Item"",
                    ""Preparation"", ""RawText"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                SELECT r.""RecipeId"", v.sort_order, v.quantity, v.unit, v.item, v.preparation, v.raw_text,
                    '{SeedDate}', '{SystemSubjectId}', '{SeedDate}', '{SystemSubjectId}'
                FROM public.""Recipe"" r
                CROSS JOIN (VALUES
                    {ingredients}
                ) AS v(sort_order, quantity, unit, item, preparation, raw_text)
                WHERE {owned}
                  AND NOT EXISTS (SELECT 1 FROM public.""RecipeIngredient"" x WHERE x.""RecipeId"" = r.""RecipeId"");
            ");

            // RawText repeats the instruction, as the startup seeder did.
            var instructions = string.Join(",\n                    ", def.Instructions.Select(i =>
                $"({i.StepNumber}, {Literal(i.Instruction)}::text)"));
            migrationBuilder.Sql($@"
                INSERT INTO public.""RecipeInstruction"" (""RecipeId"", ""StepNumber"", ""Instruction"", ""RawText"",
                    ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                SELECT r.""RecipeId"", v.step_number, v.instruction, v.instruction,
                    '{SeedDate}', '{SystemSubjectId}', '{SeedDate}', '{SystemSubjectId}'
                FROM public.""Recipe"" r
                CROSS JOIN (VALUES
                    {instructions}
                ) AS v(step_number, instruction)
                WHERE {owned}
                  AND NOT EXISTS (SELECT 1 FROM public.""RecipeInstruction"" x WHERE x.""RecipeId"" = r.""RecipeId"");
            ");
        }

        private static string Literal(string value) =>
            value == null ? "NULL" : "'" + value.Replace("'", "''") + "'";

        private static string Numeric(decimal? value) =>
            value == null ? "NULL::numeric" : value.Value.ToString(CultureInfo.InvariantCulture) + "::numeric";
    }
}
