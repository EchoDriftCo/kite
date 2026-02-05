using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUnitData : Migration
    {
        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime SeedDate = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert system subject if not exists
            migrationBuilder.Sql($@"
                INSERT INTO public.""Subject"" (""SubjectId"", ""Name"", ""CreatedDate"")
                VALUES ('{SystemSubjectId}', 'System', '{SeedDate:yyyy-MM-dd HH:mm:ss}')
                ON CONFLICT (""SubjectId"") DO NOTHING;
            ");

            // Seed Volume Units
            InsertUnit(migrationBuilder, 1, "tablespoon", "tbsp", "tablespoons", 1, 14.79m, null, 1);
            InsertUnitAliases(migrationBuilder, 1, "T", "Tbsp", "tbs", "tblsp");

            InsertUnit(migrationBuilder, 2, "teaspoon", "tsp", "teaspoons", 1, 4.93m, null, 2);
            InsertUnitAliases(migrationBuilder, 2, "t", "Tsp");

            InsertUnit(migrationBuilder, 3, "cup", "cup", "cups", 1, 236.59m, null, 3);
            InsertUnitAliases(migrationBuilder, 3, "c", "C");

            InsertUnit(migrationBuilder, 4, "fluid ounce", "fl oz", "fluid ounces", 1, 29.57m, null, 4);
            InsertUnitAliases(migrationBuilder, 4, "floz", "fl. oz.");

            InsertUnit(migrationBuilder, 5, "pint", "pt", "pints", 1, 473.18m, null, 5);

            InsertUnit(migrationBuilder, 6, "quart", "qt", "quarts", 1, 946.35m, null, 6);

            InsertUnit(migrationBuilder, 7, "gallon", "gal", "gallons", 1, 3785.41m, null, 7);

            InsertUnit(migrationBuilder, 8, "milliliter", "ml", "milliliters", 1, 1.0m, null, 8);
            InsertUnitAliases(migrationBuilder, 8, "mL", "millilitres");

            InsertUnit(migrationBuilder, 9, "liter", "l", "liters", 1, 1000.0m, null, 9);
            InsertUnitAliases(migrationBuilder, 9, "L", "litres", "litre");

            // Seed Weight Units
            InsertUnit(migrationBuilder, 10, "ounce", "oz", "ounces", 2, null, 28.35m, 10);

            InsertUnit(migrationBuilder, 11, "pound", "lb", "pounds", 2, null, 453.59m, 11);
            InsertUnitAliases(migrationBuilder, 11, "lbs");

            InsertUnit(migrationBuilder, 12, "gram", "g", "grams", 2, null, 1.0m, 12);
            InsertUnitAliases(migrationBuilder, 12, "gm", "grams");

            InsertUnit(migrationBuilder, 13, "kilogram", "kg", "kilograms", 2, null, 1000.0m, 13);
            InsertUnitAliases(migrationBuilder, 13, "kilo", "kilos");

            // Seed Count Units
            InsertUnit(migrationBuilder, 14, "piece", "piece", "pieces", 3, null, null, 14);
            InsertUnitAliases(migrationBuilder, 14, "pc", "pcs", "whole");

            InsertUnit(migrationBuilder, 15, "clove", "clove", "cloves", 3, null, null, 15);

            InsertUnit(migrationBuilder, 16, "slice", "slice", "slices", 3, null, null, 16);

            InsertUnit(migrationBuilder, 17, "sprig", "sprig", "sprigs", 3, null, null, 17);

            InsertUnit(migrationBuilder, 18, "bunch", "bunch", "bunches", 3, null, null, 18);

            InsertUnit(migrationBuilder, 19, "head", "head", "heads", 3, null, null, 19);

            InsertUnit(migrationBuilder, 20, "stalk", "stalk", "stalks", 3, null, null, 20);

            InsertUnit(migrationBuilder, 21, "can", "can", "cans", 3, null, null, 21);
            InsertUnitAliases(migrationBuilder, 21, "tin", "tins");

            InsertUnit(migrationBuilder, 22, "package", "pkg", "packages", 3, null, null, 22);
            InsertUnitAliases(migrationBuilder, 22, "packet", "packets");

            InsertUnit(migrationBuilder, 23, "dozen", "dozen", "dozens", 3, null, null, 23);
            InsertUnitAliases(migrationBuilder, 23, "doz");

            InsertUnit(migrationBuilder, 24, "large", "large", "large", 3, null, null, 24);
            InsertUnitAliases(migrationBuilder, 24, "lg");

            InsertUnit(migrationBuilder, 25, "medium", "medium", "medium", 3, null, null, 25);
            InsertUnitAliases(migrationBuilder, 25, "med");

            InsertUnit(migrationBuilder, 26, "small", "small", "small", 3, null, null, 26);
            InsertUnitAliases(migrationBuilder, 26, "sm");

            // Seed Descriptive Units
            InsertUnit(migrationBuilder, 27, "pinch", "pinch", "pinches", 4, null, null, 27);

            InsertUnit(migrationBuilder, 28, "dash", "dash", "dashes", 4, null, null, 28);

            InsertUnit(migrationBuilder, 29, "splash", "splash", "splashes", 4, null, null, 29);

            InsertUnit(migrationBuilder, 30, "handful", "handful", "handfuls", 4, null, null, 30);

            InsertUnit(migrationBuilder, 31, "drop", "drop", "drops", 4, null, null, 31);

            // Reset the sequence to continue after our seeded IDs
            migrationBuilder.Sql(@"SELECT setval('public.""Unit_UnitId_seq""', 100, false);");
            migrationBuilder.Sql(@"SELECT setval('public.""UnitAlias_UnitAliasId_seq""', 100, false);");
        }

        private static void InsertUnit(MigrationBuilder migrationBuilder, int unitId, string name, string abbreviation, string pluralName, int type, decimal? metricMl, decimal? metricG, int sortOrder)
        {
            var resourceId = Guid.NewGuid();
            var metricMlValue = metricMl.HasValue ? metricMl.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL";
            var metricGValue = metricG.HasValue ? metricG.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL";

            migrationBuilder.Sql($@"
                INSERT INTO public.""Unit"" (""UnitId"", ""UnitResourceId"", ""Name"", ""Abbreviation"", ""PluralName"", ""Type"", ""MetricEquivalentMl"", ""MetricEquivalentG"", ""SortOrder"", ""IsActive"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                VALUES ({unitId}, '{resourceId}', '{name}', '{abbreviation}', '{pluralName}', {type}, {metricMlValue}, {metricGValue}, {sortOrder}, true, '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}', '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}');
            ");
        }

        private static void InsertUnitAliases(MigrationBuilder migrationBuilder, int unitId, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                migrationBuilder.Sql($@"
                    INSERT INTO public.""UnitAlias"" (""UnitId"", ""Alias"", ""IsAutoGenerated"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                    VALUES ({unitId}, '{alias}', false, '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}', '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}');
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""UnitAlias"";");
            migrationBuilder.Sql(@"DELETE FROM public.""Unit"";");
        }
    }
}
