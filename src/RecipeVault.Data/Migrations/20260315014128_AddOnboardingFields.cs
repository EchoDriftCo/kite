using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedOnboarding",
                schema: "public",
                table: "UserAccount",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedDate",
                schema: "public",
                table: "UserAccount",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingProgressJson",
                schema: "public",
                table: "UserAccount",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSampleRecipe",
                schema: "public",
                table: "Recipe",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShowcaseFeature",
                schema: "public",
                table: "Recipe",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompletedOnboarding",
                schema: "public",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedDate",
                schema: "public",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "OnboardingProgressJson",
                schema: "public",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "IsSampleRecipe",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ShowcaseFeature",
                schema: "public",
                table: "Recipe");
        }
    }
}
