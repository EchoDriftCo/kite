using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeMixingFields : Migration
    {
        private static readonly string[] AvoidedIngredientIndexColumns = new[] { "DietaryProfileId", "IngredientName" };
        private static readonly string[] DietaryRestrictionIndexColumns = new[] { "DietaryProfileId", "RestrictionCode" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MixIntent",
                schema: "public",
                table: "Recipe",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MixedFromRecipeAId",
                schema: "public",
                table: "Recipe",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MixedFromRecipeBId",
                schema: "public",
                table: "Recipe",
                type: "integer",
                nullable: true);

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
                name: "IX_Recipe_MixedFromRecipeAId",
                schema: "public",
                table: "Recipe",
                column: "MixedFromRecipeAId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_MixedFromRecipeBId",
                schema: "public",
                table: "Recipe",
                column: "MixedFromRecipeBId");

            migrationBuilder.CreateIndex(
                name: "IX_AvoidedIngredient_CreatedSubjectId",
                schema: "public",
                table: "AvoidedIngredient",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AvoidedIngredient_DietaryProfileId_IngredientName",
                schema: "public",
                table: "AvoidedIngredient",
                columns: AvoidedIngredientIndexColumns);

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
                columns: DietaryRestrictionIndexColumns,
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

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_Recipe_MixedFromRecipeAId",
                schema: "public",
                table: "Recipe",
                column: "MixedFromRecipeAId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_Recipe_MixedFromRecipeBId",
                schema: "public",
                table: "Recipe",
                column: "MixedFromRecipeBId",
                principalSchema: "public",
                principalTable: "Recipe",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_Recipe_MixedFromRecipeAId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_Recipe_MixedFromRecipeBId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropTable(
                name: "AvoidedIngredient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CollectionRecipe",
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
                name: "DietaryProfile",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_MixedFromRecipeAId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_MixedFromRecipeBId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "MixIntent",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "MixedFromRecipeAId",
                schema: "public",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "MixedFromRecipeBId",
                schema: "public",
                table: "Recipe");
        }
    }
}
