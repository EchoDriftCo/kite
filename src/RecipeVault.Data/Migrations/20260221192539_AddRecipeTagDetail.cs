using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeTagDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTagAlias",
                schema: "public");

            migrationBuilder.AddColumn<string>(
                name: "Detail",
                schema: "public",
                table: "RecipeTag",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEntityId",
                schema: "public",
                table: "RecipeTag",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NormalizedEntityType",
                schema: "public",
                table: "RecipeTag",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Detail",
                schema: "public",
                table: "RecipeTag");

            migrationBuilder.DropColumn(
                name: "NormalizedEntityId",
                schema: "public",
                table: "RecipeTag");

            migrationBuilder.DropColumn(
                name: "NormalizedEntityType",
                schema: "public",
                table: "RecipeTag");

            migrationBuilder.CreateTable(
                name: "UserTagAlias",
                schema: "public",
                columns: table => new
                {
                    UserTagAliasId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    Alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    NormalizedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NormalizedEntityType = table.Column<int>(type: "integer", nullable: true),
                    ShowAliasPublicly = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTagAlias", x => x.UserTagAliasId);
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Tag_TagId",
                        column: x => x.TagId,
                        principalSchema: "public",
                        principalTable: "Tag",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_CreatedSubjectId",
                schema: "public",
                table: "UserTagAlias",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_LastModifiedSubjectId",
                schema: "public",
                table: "UserTagAlias",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_TagId",
                schema: "public",
                table: "UserTagAlias",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_UserId",
                schema: "public",
                table: "UserTagAlias",
                column: "UserId");

#pragma warning disable CA1861
            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_UserId_TagId",
                schema: "public",
                table: "UserTagAlias",
                columns: new[] { "UserId", "TagId" },
                unique: true);
#pragma warning restore CA1861
        }
    }
}
