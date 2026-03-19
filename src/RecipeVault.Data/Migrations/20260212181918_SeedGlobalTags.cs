using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedGlobalTags : Migration
    {
        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime SeedDate = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc);

        // TagCategory enum values
        private const int Dietary = 1;
        private const int Cuisine = 2;
        private const int MealType = 3;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert system subject if not exists
            migrationBuilder.Sql($@"
                INSERT INTO public.""Subject"" (""SubjectId"", ""Name"", ""CreatedDate"")
                VALUES ('{SystemSubjectId}', 'System', '{SeedDate:yyyy-MM-dd HH:mm:ss}')
                ON CONFLICT (""SubjectId"") DO NOTHING;
            ");

            // Dietary tags
            InsertTag(migrationBuilder, 1,  "Vegan",          Dietary);
            InsertTag(migrationBuilder, 2,  "Vegetarian",     Dietary);
            InsertTag(migrationBuilder, 3,  "Gluten-Free",    Dietary);
            InsertTag(migrationBuilder, 4,  "Dairy-Free",     Dietary);
            InsertTag(migrationBuilder, 5,  "Nut-Free",       Dietary);
            InsertTag(migrationBuilder, 6,  "Keto",           Dietary);
            InsertTag(migrationBuilder, 7,  "Paleo",          Dietary);
            InsertTag(migrationBuilder, 8,  "Low-Carb",       Dietary);
            InsertTag(migrationBuilder, 9,  "Sugar-Free",     Dietary);
            InsertTag(migrationBuilder, 10, "Whole30",        Dietary);
            InsertTag(migrationBuilder, 11, "Halal",          Dietary);
            InsertTag(migrationBuilder, 12, "Kosher",         Dietary);

            // Cuisine tags
            InsertTag(migrationBuilder, 13, "Italian",        Cuisine);
            InsertTag(migrationBuilder, 14, "Mexican",        Cuisine);
            InsertTag(migrationBuilder, 15, "Chinese",        Cuisine);
            InsertTag(migrationBuilder, 16, "Japanese",       Cuisine);
            InsertTag(migrationBuilder, 17, "Thai",           Cuisine);
            InsertTag(migrationBuilder, 18, "Indian",         Cuisine);
            InsertTag(migrationBuilder, 19, "French",         Cuisine);
            InsertTag(migrationBuilder, 20, "Mediterranean",  Cuisine);
            InsertTag(migrationBuilder, 21, "American",       Cuisine);
            InsertTag(migrationBuilder, 22, "Korean",         Cuisine);
            InsertTag(migrationBuilder, 23, "Vietnamese",     Cuisine);
            InsertTag(migrationBuilder, 24, "Middle Eastern", Cuisine);
            InsertTag(migrationBuilder, 25, "Greek",          Cuisine);
            InsertTag(migrationBuilder, 26, "Spanish",        Cuisine);
            InsertTag(migrationBuilder, 27, "Caribbean",      Cuisine);

            // MealType tags
            InsertTag(migrationBuilder, 28, "Breakfast",  MealType);
            InsertTag(migrationBuilder, 29, "Lunch",      MealType);
            InsertTag(migrationBuilder, 30, "Dinner",     MealType);
            InsertTag(migrationBuilder, 31, "Snack",      MealType);
            InsertTag(migrationBuilder, 32, "Dessert",    MealType);
            InsertTag(migrationBuilder, 33, "Appetizer",  MealType);
            InsertTag(migrationBuilder, 34, "Side Dish",  MealType);
            InsertTag(migrationBuilder, 35, "Beverage",   MealType);
            InsertTag(migrationBuilder, 36, "Soup",       MealType);
            InsertTag(migrationBuilder, 37, "Salad",      MealType);

            // Reset sequence so new user-created tags start at 101+
            migrationBuilder.Sql(@"SELECT setval('public.""Tag_TagId_seq""', 100, false);");
        }

        private static void InsertTag(MigrationBuilder migrationBuilder, int tagId, string name, int category)
        {
            var resourceId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO public.""Tag"" (""TagId"", ""TagResourceId"", ""Name"", ""Category"", ""IsGlobal"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                VALUES ({tagId}, '{resourceId}', '{name}', {category}, true, '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}', '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""Tag"" WHERE ""IsGlobal"" = true;");
        }
    }
}
