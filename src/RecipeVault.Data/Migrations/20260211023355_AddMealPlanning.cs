using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealPlan",
                schema: "public",
                columns: table => new
                {
                    MealPlanId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MealPlanResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlan", x => x.MealPlanId);
                    table.ForeignKey(
                        name: "FK_MealPlan_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_MealPlan_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "MealPlanEntry",
                schema: "public",
                columns: table => new
                {
                    MealPlanEntryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MealPlanId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MealSlot = table.Column<int>(type: "integer", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    Servings = table.Column<int>(type: "integer", nullable: true),
                    IsLeftover = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanEntry", x => x.MealPlanEntryId);
                    table.ForeignKey(
                        name: "FK_MealPlanEntry_MealPlan_MealPlanId",
                        column: x => x.MealPlanId,
                        principalSchema: "public",
                        principalTable: "MealPlan",
                        principalColumn: "MealPlanId");
                    table.ForeignKey(
                        name: "FK_MealPlanEntry_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealPlan_CreatedSubjectId",
                schema: "public",
                table: "MealPlan",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlan_LastModifiedSubjectId",
                schema: "public",
                table: "MealPlan",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlan_MealPlanResourceId",
                schema: "public",
                table: "MealPlan",
                column: "MealPlanResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_MealPlanId",
                schema: "public",
                table: "MealPlanEntry",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_RecipeId",
                schema: "public",
                table: "MealPlanEntry",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealPlanEntry",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MealPlan",
                schema: "public");
        }
    }
}
