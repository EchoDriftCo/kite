using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccount",
                schema: "public",
                columns: table => new
                {
                    UserAccountId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserAccountResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountTier = table.Column<int>(type: "integer", nullable: false),
                    TierChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccount", x => x.UserAccountId);
                    table.ForeignKey(
                        name: "FK_UserAccount_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserAccount_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_CreatedSubjectId",
                schema: "public",
                table: "UserAccount",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_LastModifiedSubjectId",
                schema: "public",
                table: "UserAccount",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_SubjectId",
                schema: "public",
                table: "UserAccount",
                column: "SubjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_UserAccountResourceId",
                schema: "public",
                table: "UserAccount",
                column: "UserAccountResourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccount",
                schema: "public");
        }
    }
}
