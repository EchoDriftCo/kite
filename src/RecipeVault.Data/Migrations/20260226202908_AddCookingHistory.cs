#pragma warning disable CA1861

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCookingHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collection",
                schema: "public",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => x.CollectionId);
                    table.ForeignKey(
                        name: "FK_Collection_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Collection_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "CookingLog",
                schema: "public",
                columns: table => new
                {
                    CookingLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CookingLogResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    CookedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScaleFactor = table.Column<decimal>(type: "numeric", nullable: true),
                    ServingsMade = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookingLog", x => x.CookingLogId);
                    table.ForeignKey(
                        name: "FK_CookingLog_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CookingLog_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_CookingLog_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

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
                name: "ImportJob",
                schema: "public",
                columns: table => new
                {
                    ImportJobId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImportJobResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalItems = table.Column<int>(type: "integer", nullable: false),
                    ProcessedItems = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultsJson = table.Column<string>(type: "text", maxLength: 2147483647, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJob", x => x.ImportJobId);
                    table.ForeignKey(
                        name: "FK_ImportJob_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_ImportJob_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "CollectionRecipe",
                schema: "public",
                columns: table => new
                {
                    CollectionRecipeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionRecipe", x => x.CollectionRecipeId);
                    table.ForeignKey(
                        name: "FK_CollectionRecipe_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "public",
                        principalTable: "Collection",
                        principalColumn: "CollectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionRecipe_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CookingLogPhoto",
                schema: "public",
                columns: table => new
                {
                    CookingLogPhotoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CookingLogId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookingLogPhoto", x => x.CookingLogPhotoId);
                    table.ForeignKey(
                        name: "FK_CookingLogPhoto_CookingLog_CookingLogId",
                        column: x => x.CookingLogId,
                        principalSchema: "public",
                        principalTable: "CookingLog",
                        principalColumn: "CookingLogId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Collection_CollectionResourceId",
                schema: "public",
                table: "Collection",
                column: "CollectionResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collection_CreatedSubjectId",
                schema: "public",
                table: "Collection",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_LastModifiedSubjectId",
                schema: "public",
                table: "Collection",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRecipe_CollectionId",
                schema: "public",
                table: "CollectionRecipe",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRecipe_RecipeId",
                schema: "public",
                table: "CollectionRecipe",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_CookingLog_CookedDate",
                schema: "public",
                table: "CookingLog",
                column: "CookedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CookingLog_CookingLogResourceId",
                schema: "public",
                table: "CookingLog",
                column: "CookingLogResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CookingLog_CreatedSubjectId",
                schema: "public",
                table: "CookingLog",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CookingLog_LastModifiedSubjectId",
                schema: "public",
                table: "CookingLog",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CookingLog_RecipeId",
                schema: "public",
                table: "CookingLog",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_CookingLogPhoto_CookingLogId",
                schema: "public",
                table: "CookingLogPhoto",
                column: "CookingLogId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ImportJob_CreatedSubjectId",
                schema: "public",
                table: "ImportJob",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJob_ImportJobResourceId",
                schema: "public",
                table: "ImportJob",
                column: "ImportJobResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportJob_LastModifiedSubjectId",
                schema: "public",
                table: "ImportJob",
                column: "LastModifiedSubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvoidedIngredient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CollectionRecipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CookingLogPhoto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DietaryRestriction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ImportJob",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Collection",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CookingLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DietaryProfile",
                schema: "public");
        }
    }
}
