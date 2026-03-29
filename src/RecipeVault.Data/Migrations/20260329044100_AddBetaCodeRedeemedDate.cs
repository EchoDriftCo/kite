using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations {
    /// <inheritdoc />
    public partial class AddBetaCodeRedeemedDate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<DateTime>(
                name: "BetaCodeRedeemedDate",
                table: "UserAccount",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "BetaCodeRedeemedDate",
                table: "UserAccount");
        }
    }
}
