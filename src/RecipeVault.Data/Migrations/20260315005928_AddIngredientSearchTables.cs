using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments - EF Core generated code

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientSearchTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientSynonym",
                schema: "public",
                columns: table => new
                {
                    IngredientSynonymId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanonicalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Synonym = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientSynonym", x => x.IngredientSynonymId);
                });

            migrationBuilder.CreateTable(
                name: "UserPantryItem",
                schema: "public",
                columns: table => new
                {
                    UserPantryItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsStaple = table.Column<bool>(type: "boolean", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPantryItem", x => x.UserPantryItemId);
                    table.ForeignKey(
                        name: "FK_UserPantryItem_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserPantryItem_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSynonym_CanonicalName",
                schema: "public",
                table: "IngredientSynonym",
                column: "CanonicalName");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSynonym_Synonym",
                schema: "public",
                table: "IngredientSynonym",
                column: "Synonym",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPantryItem_CreatedSubjectId",
                schema: "public",
                table: "UserPantryItem",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPantryItem_LastModifiedSubjectId",
                schema: "public",
                table: "UserPantryItem",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPantryItem_SubjectId_IngredientName",
                schema: "public",
                table: "UserPantryItem",
                columns: new[] { "SubjectId", "IngredientName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPantryItem_SubjectId_IsStaple",
                schema: "public",
                table: "UserPantryItem",
                columns: new[] { "SubjectId", "IsStaple" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientSynonym",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserPantryItem",
                schema: "public");
        }
    }
}
