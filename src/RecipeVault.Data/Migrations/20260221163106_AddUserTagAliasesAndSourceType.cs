using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTagAliasesAndSourceType : Migration
    {
        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime SeedDate = new DateTime(2026, 2, 21, 0, 0, 0, DateTimeKind.Utc);

        // TagCategory enum values
        private const int Source = 4;

        // SourceType enum values
        private const int Family = 1;
        private const int Chef = 2;
        private const int Restaurant = 3;
        private const int Cookbook = 4;
        private const int Website = 5;
        private const int Original = 6;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemTag",
                schema: "public",
                table: "Tag",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                schema: "public",
                table: "Tag",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTagAlias",
                schema: "public",
                columns: table => new
                {
                    UserTagAliasId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    Alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NormalizedEntityType = table.Column<int>(type: "integer", nullable: true),
                    ShowAliasPublicly = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was created"),
                    CreatedSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date and time entity was last modified"),
                    LastModifiedSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTagAlias", x => x.UserTagAliasId);
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Subject_CreatedSubjectId",
                        column: x => x.CreatedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Subject_LastModifiedSubjectId",
                        column: x => x.LastModifiedSubjectId,
                        principalSchema: "public",
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_UserTagAlias_Tag_TagId",
                        column: x => x.TagId,
                        principalSchema: "public",
                        principalTable: "Tag",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_CreatedSubjectId",
                schema: "public",
                table: "UserTagAlias",
                column: "CreatedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_LastModifiedSubjectId",
                schema: "public",
                table: "UserTagAlias",
                column: "LastModifiedSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_TagId",
                schema: "public",
                table: "UserTagAlias",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_UserId",
                schema: "public",
                table: "UserTagAlias",
                column: "UserId");

#pragma warning disable CA1861 // Avoid constant arrays as arguments
            migrationBuilder.CreateIndex(
                name: "IX_UserTagAlias_UserId_TagId",
                schema: "public",
                table: "UserTagAlias",
                columns: new[] { "UserId", "TagId" },
                unique: true);
#pragma warning restore CA1861

            // Backfill IsSystemTag for existing global tags
            migrationBuilder.Sql(@"UPDATE public.""Tag"" SET ""IsSystemTag"" = true WHERE ""IsGlobal"" = true;");

            // Insert new Source system tags
            InsertSourceTag(migrationBuilder, 38, "Family Recipe", Family);
            InsertSourceTag(migrationBuilder, 39, "Chef", Chef);
            InsertSourceTag(migrationBuilder, 40, "Restaurant", Restaurant);
            InsertSourceTag(migrationBuilder, 41, "Cookbook", Cookbook);
            InsertSourceTag(migrationBuilder, 42, "Website", Website);
            InsertSourceTag(migrationBuilder, 43, "Original Creation", Original);

            // Reset sequence so new user-created tags start at 101+
            migrationBuilder.Sql(@"SELECT setval('public.""Tag_TagId_seq""', 100, false);");
        }

        private static void InsertSourceTag(MigrationBuilder migrationBuilder, int tagId, string name, int sourceType)
        {
            var resourceId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO public.""Tag"" (""TagId"", ""TagResourceId"", ""Name"", ""Category"", ""IsGlobal"", ""IsSystemTag"", ""SourceType"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                VALUES ({tagId}, '{resourceId}', '{name}', {Source}, true, true, {sourceType}, '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}', '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}')
                ON CONFLICT (""TagId"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTagAlias",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IsSystemTag",
                schema: "public",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "Tag");
        }
    }
}
