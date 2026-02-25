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
            // PostgreSQL cannot ALTER int to uuid automatically
            // Drop and recreate columns with correct types
            
            // CircleRecipe.SharedBySubjectId
            migrationBuilder.DropColumn(
                name: "SharedBySubjectId",
                schema: "public",
                table: "CircleRecipe");
            
            migrationBuilder.AddColumn<Guid>(
                name: "SharedBySubjectId",
                schema: "public",
                table: "CircleRecipe",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));
            
            // CircleMember.SubjectId
            migrationBuilder.DropColumn(
                name: "SubjectId",
                schema: "public",
                table: "CircleMember");
            
            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                schema: "public",
                table: "CircleMember",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));
            
            // CircleInvite.InvitedBySubjectId
            migrationBuilder.DropColumn(
                name: "InvitedBySubjectId",
                schema: "public",
                table: "CircleInvite");
            
            migrationBuilder.AddColumn<Guid>(
                name: "InvitedBySubjectId",
                schema: "public",
                table: "CircleInvite",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));
            
            // Circle.OwnerSubjectId
            migrationBuilder.DropColumn(
                name: "OwnerSubjectId",
                schema: "public",
                table: "Circle");
            
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerSubjectId",
                schema: "public",
                table: "Circle",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));

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
