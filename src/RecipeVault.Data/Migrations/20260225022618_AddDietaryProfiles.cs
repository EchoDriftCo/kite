using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDietaryProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DietaryProfile",
                schema: "public",
                columns: table => new
                {
                    DietaryProfileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DietaryProfileResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryProfile", x => x.DietaryProfileId);
                    table.ForeignKey(
                        name: "FK_DietaryProfile_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_DietaryProfile_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "AvoidedIngredient",
                schema: "public",
                columns: table => new
                {
                    AvoidedIngredientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DietaryProfileId = table.Column<int>(type: "integer", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvoidedIngredient", x => x.AvoidedIngredientId);
                    table.ForeignKey(
                        name: "FK_AvoidedIngredient_DietaryProfile_DietaryProfileId",
                        column: x => x.DietaryProfileId,
                        principalSchema: "public",
                        principalTable: "DietaryProfile",
                        principalColumn: "DietaryProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvoidedIngredient_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_AvoidedIngredient_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "DietaryRestriction",
                schema: "public",
                columns: table => new
                {
                    DietaryRestrictionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DietaryProfileId = table.Column<int>(type: "integer", nullable: false),
                    RestrictionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RestrictionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryRestriction", x => x.DietaryRestrictionId);
                    table.ForeignKey(
                        name: "FK_DietaryRestriction_DietaryProfile_DietaryProfileId",
                        column: x => x.DietaryProfileId,
                        principalSchema: "public",
                        principalTable: "DietaryProfile",
                        principalColumn: "DietaryProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DietaryRestriction_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_DietaryRestriction_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvoidedIngredient_CreatedSubjectId",
                schema: "public",
                table: "AvoidedIngredient",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AvoidedIngredient_DietaryProfileId_IngredientName",
                schema: "public",
                table: "AvoidedIngredient",
                columns: new[] { "DietaryProfileId", "IngredientName" });

            migrationBuilder.CreateIndex(
                name: "IX_AvoidedIngredient_LastModifiedSubjectId",
                schema: "public",
                table: "AvoidedIngredient",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryProfile_CreatedSubjectId",
                schema: "public",
                table: "DietaryProfile",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryProfile_DietaryProfileResourceId",
                schema: "public",
                table: "DietaryProfile",
                column: "DietaryProfileResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietaryProfile_LastModifiedSubjectId",
                schema: "public",
                table: "DietaryProfile",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryRestriction_CreatedSubjectId",
                schema: "public",
                table: "DietaryRestriction",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryRestriction_DietaryProfileId_RestrictionCode",
                schema: "public",
                table: "DietaryRestriction",
                columns: new[] { "DietaryProfileId", "RestrictionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietaryRestriction_LastModifiedSubjectId",
                schema: "public",
                table: "DietaryRestriction",
                column: "LastModifiedSubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvoidedIngredient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DietaryRestriction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DietaryProfile",
                schema: "public");
        }
    }
}
