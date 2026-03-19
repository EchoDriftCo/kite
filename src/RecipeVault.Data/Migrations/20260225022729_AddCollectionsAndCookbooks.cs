using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionsAndCookbooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collection",
                schema: "public",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => x.CollectionId);
                    table.ForeignKey(
                        name: "FK_Collection_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Collection_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "CollectionRecipe",
                schema: "public",
                columns: table => new
                {
                    CollectionRecipeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionRecipe", x => x.CollectionRecipeId);
                    table.ForeignKey(
                        name: "FK_CollectionRecipe_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "public",
                        principalTable: "Collection",
                        principalColumn: "CollectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionRecipe_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collection_CollectionResourceId",
                schema: "public",
                table: "Collection",
                column: "CollectionResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collection_CreatedSubjectId",
                schema: "public",
                table: "Collection",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_LastModifiedSubjectId",
                schema: "public",
                table: "Collection",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRecipe_CollectionId",
                schema: "public",
                table: "CollectionRecipe",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRecipe_RecipeId",
                schema: "public",
                table: "CollectionRecipe",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionRecipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Collection",
                schema: "public");
        }
    }
}
