using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_MealPlan_MealPlanId",
                schema: "public",
                table: "MealPlanEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_Recipe_RecipeId",
                schema: "public",
                table: "MealPlanEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Recipe_RecipeId",
                schema: "public",
                table: "RecipeIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeInstruction_Recipe_RecipeId",
                schema: "public",
                table: "RecipeInstruction");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Recipe_RecipeId",
                schema: "public",
                table: "RecipeTag");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_MealPlan_MealPlanId",
                schema: "public",
                table: "MealPlanEntry",
                column: "MealPlanId",
                principalSchema: "public",
                principalTable: "MealPlan",
                principalColumn: "MealPlanId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_Recipe_RecipeId",
                schema: "public",
                table: "MealPlanEntry",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Recipe_RecipeId",
                schema: "public",
                table: "RecipeIngredient",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeInstruction_Recipe_RecipeId",
                schema: "public",
                table: "RecipeInstruction",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Recipe_RecipeId",
                schema: "public",
                table: "RecipeTag",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_MealPlan_MealPlanId",
                schema: "public",
                table: "MealPlanEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_Recipe_RecipeId",
                schema: "public",
                table: "MealPlanEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Recipe_RecipeId",
                schema: "public",
                table: "RecipeIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeInstruction_Recipe_RecipeId",
                schema: "public",
                table: "RecipeInstruction");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Recipe_RecipeId",
                schema: "public",
                table: "RecipeTag");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_MealPlan_MealPlanId",
                schema: "public",
                table: "MealPlanEntry",
                column: "MealPlanId",
                principalSchema: "public",
                principalTable: "MealPlan",
                principalColumn: "MealPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_Recipe_RecipeId",
                schema: "public",
                table: "MealPlanEntry",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Recipe_RecipeId",
                schema: "public",
                table: "RecipeIngredient",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeInstruction_Recipe_RecipeId",
                schema: "public",
                table: "RecipeInstruction",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Recipe_RecipeId",
                schema: "public",
                table: "RecipeTag",
                column: "RecipeId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId");
        }
    }
}
