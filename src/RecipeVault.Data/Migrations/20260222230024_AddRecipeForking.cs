using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeForking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForkedFromRecipeId",
                schema: "public",
                table: "Recipe",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_ForkedFromRecipeId",
                schema: "public",
                table: "Recipe",
                column: "ForkedFromRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_Recipe_ForkedFromRecipeId",
                schema: "public",
                table: "Recipe",
                column: "ForkedFromRecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_Recipe_ForkedFromRecipeId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_ForkedFromRecipeId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ForkedFromRecipeId",
                schema: "public",
                table: "Recipe");
        }
    }
}
