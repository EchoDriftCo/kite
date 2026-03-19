using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTagSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "public",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tag_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Tag_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeTag",
                schema: "public",
                columns: table => new
                {
                    RecipeTagId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    AssignedBySubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAiAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    IsOverridden = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeTag", x => x.RecipeTagId);
                    table.ForeignKey(
                        name: "FK_RecipeTag_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                    table.ForeignKey(
                        name: "FK_RecipeTag_Tag_TagId",
                        column: x => x.TagId,
                        principalSchema: "public",
                        principalTable: "Tag",
                        principalColumn: "TagId");
                });

#pragma warning disable CA1861
            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_RecipeId_TagId",
                schema: "public",
                table: "RecipeTag",
                columns: new[] { "RecipeId", "TagId" },
                unique: true);
#pragma warning restore CA1861

            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_TagId",
                schema: "public",
                table: "RecipeTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CreatedSubjectId",
                schema: "public",
                table: "Tag",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_LastModifiedSubjectId",
                schema: "public",
                table: "Tag",
                column: "LastModifiedSubjectId");

#pragma warning disable CA1861
            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name_Category",
                schema: "public",
                table: "Tag",
                columns: new[] { "Name", "Category" },
                unique: true);
#pragma warning restore CA1861

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagResourceId",
                schema: "public",
                table: "Tag",
                column: "TagResourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeTag",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "public");
        }
    }
}
