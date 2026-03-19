using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments - EF Core generated code

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialCircles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Circle",
                schema: "public",
                columns: table => new
                {
                    CircleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CircleResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerSubjectId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circle", x => x.CircleId);
                    table.ForeignKey(
                        name: "FK_Circle_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Circle_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "CircleInvite",
                schema: "public",
                columns: table => new
                {
                    CircleInviteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InviteToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<int>(type: "integer", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InvitedBySubjectId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleInvite", x => x.CircleInviteId);
                    table.ForeignKey(
                        name: "FK_CircleInvite_Circle_CircleId",
                        column: x => x.CircleId,
                        principalSchema: "public",
                        principalTable: "Circle",
                        principalColumn: "CircleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CircleMember",
                schema: "public",
                columns: table => new
                {
                    CircleMemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CircleId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleMember", x => x.CircleMemberId);
                    table.ForeignKey(
                        name: "FK_CircleMember_Circle_CircleId",
                        column: x => x.CircleId,
                        principalSchema: "public",
                        principalTable: "Circle",
                        principalColumn: "CircleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CircleRecipe",
                schema: "public",
                columns: table => new
                {
                    CircleRecipeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CircleId = table.Column<int>(type: "integer", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    SharedBySubjectId = table.Column<int>(type: "integer", nullable: false),
                    SharedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleRecipe", x => x.CircleRecipeId);
                    table.ForeignKey(
                        name: "FK_CircleRecipe_Circle_CircleId",
                        column: x => x.CircleId,
                        principalSchema: "public",
                        principalTable: "Circle",
                        principalColumn: "CircleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CircleRecipe_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Circle_CircleResourceId",
                schema: "public",
                table: "Circle",
                column: "CircleResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Circle_CreatedSubjectId",
                schema: "public",
                table: "Circle",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Circle_LastModifiedSubjectId",
                schema: "public",
                table: "Circle",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleInvite_CircleId",
                schema: "public",
                table: "CircleInvite",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleInvite_InviteToken",
                schema: "public",
                table: "CircleInvite",
                column: "InviteToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleMember_CircleId_SubjectId",
                schema: "public",
                table: "CircleMember",
                columns: new[] { "CircleId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleRecipe_CircleId_RecipeId",
                schema: "public",
                table: "CircleRecipe",
                columns: new[] { "CircleId", "RecipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleRecipe_RecipeId",
                schema: "public",
                table: "CircleRecipe",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CircleInvite",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CircleMember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CircleRecipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Circle",
                schema: "public");
        }
    }
}
