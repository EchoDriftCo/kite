using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeShareToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                schema: "public",
                table: "Recipe",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_ShareToken",
                schema: "public",
                table: "Recipe",
                column: "ShareToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipe_ShareToken",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                schema: "public",
                table: "Recipe");
        }
    }
}
