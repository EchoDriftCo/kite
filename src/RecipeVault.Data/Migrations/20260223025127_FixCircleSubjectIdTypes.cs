using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCircleSubjectIdTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SharedBySubjectId",
                schema: "public",
                table: "CircleRecipe",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectId",
                schema: "public",
                table: "CircleMember",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitedBySubjectId",
                schema: "public",
                table: "CircleInvite",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerSubjectId",
                schema: "public",
                table: "Circle",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_CircleMember_SubjectId",
                schema: "public",
                table: "CircleMember",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_Subject_SubjectId",
                schema: "public",
                table: "CircleMember",
                column: "SubjectId",
                principalSchema: "public",
                principalTable: "Subject",
                principalColumn: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CircleMember_Subject_SubjectId",
                schema: "public",
                table: "CircleMember");

            migrationBuilder.DropIndex(
                name: "IX_CircleMember_SubjectId",
                schema: "public",
                table: "CircleMember");

            migrationBuilder.AlterColumn<int>(
                name: "SharedBySubjectId",
                schema: "public",
                table: "CircleRecipe",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                schema: "public",
                table: "CircleMember",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "InvitedBySubjectId",
                schema: "public",
                table: "CircleInvite",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerSubjectId",
                schema: "public",
                table: "Circle",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
