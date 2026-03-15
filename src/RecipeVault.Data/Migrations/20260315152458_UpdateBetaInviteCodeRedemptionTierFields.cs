using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBetaInviteCodeRedemptionTierFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewTier",
                schema: "public",
                table: "BetaInviteCodeRedemption",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreviousTier",
                schema: "public",
                table: "BetaInviteCodeRedemption",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCodeRedemption_SubjectId",
                schema: "public",
                table: "BetaInviteCodeRedemption",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BetaInviteCodeRedemption_SubjectId",
                schema: "public",
                table: "BetaInviteCodeRedemption");

            migrationBuilder.DropColumn(
                name: "NewTier",
                schema: "public",
                table: "BetaInviteCodeRedemption");

            migrationBuilder.DropColumn(
                name: "PreviousTier",
                schema: "public",
                table: "BetaInviteCodeRedemption");
        }
    }
}
