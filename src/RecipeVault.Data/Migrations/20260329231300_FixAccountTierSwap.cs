using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAccountTierSwap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous enum-to-string migration used wrong mapping: 1=Premium, 2=Beta
            // But the enum is actually: 1=Beta, 2=Premium
            // This migration fixes the database values by swapping Premium and Beta

            migrationBuilder.Sql(@"UPDATE public.""UserAccount"" SET ""AccountTier"" = CASE 
                WHEN ""AccountTier"" = 'Premium' THEN 'Beta'
                WHEN ""AccountTier"" = 'Beta' THEN 'Premium'
                ELSE ""AccountTier""
            END;");

            // Also fix BetaInviteCodeRedemption.PreviousTier
            migrationBuilder.Sql(@"UPDATE public.""BetaInviteCodeRedemption"" SET ""PreviousTier"" = CASE 
                WHEN ""PreviousTier"" = 'Premium' THEN 'Beta'
                WHEN ""PreviousTier"" = 'Beta' THEN 'Premium'
                ELSE ""PreviousTier""
            END;");

            // Also fix BetaInviteCodeRedemption.NewTier
            migrationBuilder.Sql(@"UPDATE public.""BetaInviteCodeRedemption"" SET ""NewTier"" = CASE 
                WHEN ""NewTier"" = 'Premium' THEN 'Beta'
                WHEN ""NewTier"" = 'Beta' THEN 'Premium'
                ELSE ""NewTier""
            END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the swap
            migrationBuilder.Sql(@"UPDATE public.""UserAccount"" SET ""AccountTier"" = CASE 
                WHEN ""AccountTier"" = 'Beta' THEN 'Premium'
                WHEN ""AccountTier"" = 'Premium' THEN 'Beta'
                ELSE ""AccountTier""
            END;");

            migrationBuilder.Sql(@"UPDATE public.""BetaInviteCodeRedemption"" SET ""PreviousTier"" = CASE 
                WHEN ""PreviousTier"" = 'Beta' THEN 'Premium'
                WHEN ""PreviousTier"" = 'Premium' THEN 'Beta'
                ELSE ""PreviousTier""
            END;");

            migrationBuilder.Sql(@"UPDATE public.""BetaInviteCodeRedemption"" SET ""NewTier"" = CASE 
                WHEN ""NewTier"" = 'Beta' THEN 'Premium'
                WHEN ""NewTier"" = 'Premium' THEN 'Beta'
                ELSE ""NewTier""
            END;");
        }
    }
}
