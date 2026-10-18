using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScopeTagUniquenessByOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tag_CreatedSubjectId",
                schema: "public",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_Name_Category",
                schema: "public",
                table: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CreatedSubjectId_Name_Category",
                schema: "public",
                table: "Tag",
                columns: new[] { "CreatedSubjectId", "Name", "Category" },
                unique: true,
                filter: "\"IsGlobal\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name_Category",
                schema: "public",
                table: "Tag",
                columns: new[] { "Name", "Category" },
                unique: true,
                filter: "\"IsGlobal\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tag_CreatedSubjectId_Name_Category",
                schema: "public",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_Name_Category",
                schema: "public",
                table: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CreatedSubjectId",
                schema: "public",
                table: "Tag",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name_Category",
                schema: "public",
                table: "Tag",
                columns: new[] { "Name", "Category" },
                unique: true);
        }
    }
}
