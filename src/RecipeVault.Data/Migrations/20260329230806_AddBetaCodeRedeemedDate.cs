using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBetaCodeRedeemedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add BetaCodeRedeemedDate column if it doesn't exist
            // (snapshot drift: EnumToStringConversions snapshot included it but no migration added it)
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name='UserAccount' AND column_name='BetaCodeRedeemedDate'
                    ) THEN
                        ALTER TABLE public.""UserAccount"" ADD COLUMN ""BetaCodeRedeemedDate"" timestamp with time zone NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.""UserAccount"" DROP COLUMN IF EXISTS ""BetaCodeRedeemedDate"";
            ");
        }
    }
}
