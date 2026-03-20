using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSourceVideoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceVideoUrl",
                schema: "public",
                table: "Recipe",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceVideoUrl",
                schema: "public",
                table: "Recipe");
        }
    }
}
