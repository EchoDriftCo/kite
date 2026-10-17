using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumWaitlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PremiumWaitlist",
                schema: "public",
                columns: table => new
                {
                    PremiumWaitlistId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PremiumWaitlistResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumWaitlist", x => x.PremiumWaitlistId);
                    table.ForeignKey(
                        name: "FK_PremiumWaitlist_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_PremiumWaitlist_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PremiumWaitlist_CreatedSubjectId",
                schema: "public",
                table: "PremiumWaitlist",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumWaitlist_Email",
                schema: "public",
                table: "PremiumWaitlist",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PremiumWaitlist_LastModifiedSubjectId",
                schema: "public",
                table: "PremiumWaitlist",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumWaitlist_PremiumWaitlistResourceId",
                schema: "public",
                table: "PremiumWaitlist",
                column: "PremiumWaitlistResourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PremiumWaitlist",
                schema: "public");
        }
    }
}
