using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeRatingAndFavorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                schema: "public",
                table: "Recipe",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                schema: "public",
                table: "Recipe",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Rating",
                schema: "public",
                table: "Recipe");
        }
    }
}
