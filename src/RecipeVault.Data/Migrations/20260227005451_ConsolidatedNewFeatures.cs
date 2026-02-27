using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments - EF Core generated code

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatedNewFeatures : Migration
    {
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
                name: "Equipment",
                schema: "public",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCommon = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentId);
                    table.ForeignKey(
                        name: "FK_Equipment_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Equipment_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeLink",
                schema: "public",
                columns: table => new
                {
                    RecipeLinkId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeLinkResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentRecipeId = table.Column<int>(type: "integer", nullable: false),
                    LinkedRecipeId = table.Column<int>(type: "integer", nullable: false),
                    IngredientIndex = table.Column<int>(type: "integer", nullable: true),
                    DisplayText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IncludeInTotalTime = table.Column<bool>(type: "boolean", nullable: false),
                    PortionUsed = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeLink", x => x.RecipeLinkId);
                    table.ForeignKey(
                        name: "FK_RecipeLink_Recipe_LinkedRecipeId",
                        column: x => x.LinkedRecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeLink_Recipe_ParentRecipeId",
                        column: x => x.ParentRecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeLink_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_RecipeLink_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
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
                name: "RecipeEquipment",
                schema: "public",
                columns: table => new
                {
                    RecipeEquipmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeEquipment", x => x.RecipeEquipmentId);
                    table.ForeignKey(
                        name: "FK_RecipeEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeEquipment_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeEquipment_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_RecipeEquipment_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "UserEquipment",
                schema: "public",
                columns: table => new
                {
                    UserEquipmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEquipment", x => x.UserEquipmentId);
                    table.ForeignKey(
                        name: "FK_UserEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserEquipment_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserEquipment_Subject_LastModifiedSubjectId",
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
                name: "IX_Equipment_Code",
                schema: "public",
                table: "Equipment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CreatedSubjectId",
                schema: "public",
                table: "Equipment",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_LastModifiedSubjectId",
                schema: "public",
                table: "Equipment",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEquipment_CreatedSubjectId",
                schema: "public",
                table: "RecipeEquipment",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEquipment_EquipmentId",
                schema: "public",
                table: "RecipeEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEquipment_LastModifiedSubjectId",
                schema: "public",
                table: "RecipeEquipment",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEquipment_RecipeId_EquipmentId",
                schema: "public",
                table: "RecipeEquipment",
                columns: new[] { "RecipeId", "EquipmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLink_CreatedSubjectId",
                schema: "public",
                table: "RecipeLink",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLink_LastModifiedSubjectId",
                schema: "public",
                table: "RecipeLink",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLink_LinkedRecipeId",
                schema: "public",
                table: "RecipeLink",
                column: "LinkedRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLink_ParentRecipeId",
                schema: "public",
                table: "RecipeLink",
                column: "ParentRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLink_RecipeLinkResourceId",
                schema: "public",
                table: "RecipeLink",
                column: "RecipeLinkResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipment_CreatedSubjectId",
                schema: "public",
                table: "UserEquipment",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipment_EquipmentId",
                schema: "public",
                table: "UserEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipment_LastModifiedSubjectId",
                schema: "public",
                table: "UserEquipment",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipment_SubjectId_EquipmentId",
                schema: "public",
                table: "UserEquipment",
                columns: new[] { "SubjectId", "EquipmentId" },
                unique: true);

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
                name: "CookingLogPhoto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RecipeEquipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RecipeLink",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserEquipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CookingLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Equipment",
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
