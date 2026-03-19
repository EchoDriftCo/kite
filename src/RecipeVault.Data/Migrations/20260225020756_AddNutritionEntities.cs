using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientNutrition",
                schema: "public",
                columns: table => new
                {
                    IngredientNutritionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeIngredientId = table.Column<int>(type: "integer", nullable: false),
                    FdcId = table.Column<int>(type: "integer", nullable: true),
                    MatchedFoodName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MatchConfidence = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Calories = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CarbsGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    FiberGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SugarGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SodiumMg = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    GramsUsed = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    CalculatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsManualOverride = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientNutrition", x => x.IngredientNutritionId);
                    table.ForeignKey(
                        name: "FK_IngredientNutrition_RecipeIngredient_RecipeIngredientId",
                        column: x => x.RecipeIngredientId,
                        principalSchema: "public",
                        principalTable: "RecipeIngredient",
                        principalColumn: "RecipeIngredientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientNutrition_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_IngredientNutrition_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeNutrition",
                schema: "public",
                columns: table => new
                {
                    RecipeNutritionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    CaloriesPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ProteinPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CarbsPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    FatPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    FiberPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SugarPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SodiumPerServing = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    IngredientsMatched = table.Column<int>(type: "integer", nullable: false),
                    IngredientsTotal = table.Column<int>(type: "integer", nullable: false),
                    CoveragePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CalculatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsStale = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeNutrition", x => x.RecipeNutritionId);
                    table.ForeignKey(
                        name: "FK_RecipeNutrition_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeNutrition_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_RecipeNutrition_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientNutrition_CreatedSubjectId",
                schema: "public",
                table: "IngredientNutrition",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientNutrition_LastModifiedSubjectId",
                schema: "public",
                table: "IngredientNutrition",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientNutrition_RecipeIngredientId",
                schema: "public",
                table: "IngredientNutrition",
                column: "RecipeIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeNutrition_CreatedSubjectId",
                schema: "public",
                table: "RecipeNutrition",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeNutrition_LastModifiedSubjectId",
                schema: "public",
                table: "RecipeNutrition",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeNutrition_RecipeId",
                schema: "public",
                table: "RecipeNutrition",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientNutrition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RecipeNutrition",
                schema: "public");
        }
    }
}
