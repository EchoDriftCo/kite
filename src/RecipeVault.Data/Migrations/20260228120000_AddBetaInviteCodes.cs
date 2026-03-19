using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments - EF Core generated code

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBetaInviteCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BetaInviteCode",
                schema: "public",
                columns: table => new
                {
                    BetaInviteCodeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BetaInviteCodeResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: false),
                    UseCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetaInviteCode", x => x.BetaInviteCodeId);
                    table.ForeignKey(
                        name: "FK_BetaInviteCode_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_BetaInviteCode_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "BetaInviteCodeRedemption",
                schema: "public",
                columns: table => new
                {
                    BetaInviteCodeRedemptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BetaInviteCodeId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedeemedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetaInviteCodeRedemption", x => x.BetaInviteCodeRedemptionId);
                    table.ForeignKey(
                        name: "FK_BetaInviteCodeRedemption_BetaInviteCode_BetaInviteCodeId",
                        column: x => x.BetaInviteCodeId,
                        principalSchema: "public",
                        principalTable: "BetaInviteCode",
                        principalColumn: "BetaInviteCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCode_BetaInviteCodeResourceId",
                schema: "public",
                table: "BetaInviteCode",
                column: "BetaInviteCodeResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCode_Code",
                schema: "public",
                table: "BetaInviteCode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCode_CreatedSubjectId",
                schema: "public",
                table: "BetaInviteCode",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCode_LastModifiedSubjectId",
                schema: "public",
                table: "BetaInviteCode",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BetaInviteCodeRedemption_BetaInviteCodeId",
                schema: "public",
                table: "BetaInviteCodeRedemption",
                column: "BetaInviteCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetaInviteCodeRedemption",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BetaInviteCode",
                schema: "public");
        }
    }
}
