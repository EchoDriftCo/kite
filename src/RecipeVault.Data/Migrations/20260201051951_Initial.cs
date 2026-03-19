using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Subject",
                schema: "public",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Subject primary key"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Subject primary key"),
                    GivenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Subject primary key"),
                    FamilyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Subject Surname ()"),
                    UserPrincipalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Username (upn claim)"),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                schema: "public",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Yield = table.Column<int>(type: "integer", nullable: false),
                    PrepTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    CookTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.RecipeId);
                    table.ForeignKey(
                        name: "FK_Recipe_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Recipe_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredient",
                schema: "public",
                columns: table => new
                {
                    RecipeIngredientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Item = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Preparation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RawText = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredient", x => x.RecipeIngredientId);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeInstruction",
                schema: "public",
                columns: table => new
                {
                    RecipeInstructionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeInstruction", x => x.RecipeInstructionId);
                    table.ForeignKey(
                        name: "FK_RecipeInstruction_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "public",
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                    table.ForeignKey(
                        name: "FK_RecipeInstruction_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_RecipeInstruction_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_CreatedSubjectId",
                schema: "public",
                table: "Recipe",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_LastModifiedSubjectId",
                schema: "public",
                table: "Recipe",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_RecipeResourceId",
                schema: "public",
                table: "Recipe",
                column: "RecipeResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_CreatedSubjectId",
                schema: "public",
                table: "RecipeIngredient",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_LastModifiedSubjectId",
                schema: "public",
                table: "RecipeIngredient",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_RecipeId",
                schema: "public",
                table: "RecipeIngredient",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeInstruction_CreatedSubjectId",
                schema: "public",
                table: "RecipeInstruction",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeInstruction_LastModifiedSubjectId",
                schema: "public",
                table: "RecipeInstruction",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeInstruction_RecipeId",
                schema: "public",
                table: "RecipeInstruction",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeIngredient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RecipeInstruction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Recipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Subject",
                schema: "public");
        }
    }
}
