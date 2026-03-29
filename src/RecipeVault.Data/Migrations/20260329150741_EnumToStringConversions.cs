using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnumToStringConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert all enum columns from integer to varchar(50) with proper value mapping.
            // Uses PostgreSQL ALTER COLUMN ... TYPE ... USING CASE to map int values to string names.

            // UserAccount.AccountTier: 0=Free, 1=Premium, 2=Beta
            migrationBuilder.Sql(@"ALTER TABLE public.""UserAccount"" ALTER COLUMN ""AccountTier"" TYPE varchar(50) USING CASE ""AccountTier"" WHEN 0 THEN 'Free' WHEN 1 THEN 'Premium' WHEN 2 THEN 'Beta' ELSE 'Free' END;");

            // Unit.Type: 1=Volume, 2=Weight, 3=Count, 4=Descriptive
            migrationBuilder.Sql(@"ALTER TABLE public.""Unit"" ALTER COLUMN ""Type"" TYPE varchar(50) USING CASE ""Type"" WHEN 1 THEN 'Volume' WHEN 2 THEN 'Weight' WHEN 3 THEN 'Count' WHEN 4 THEN 'Descriptive' ELSE 'Volume' END;");

            // Tag.SourceType (nullable): 1=Family, 2=Chef, 3=Restaurant, 4=Cookbook, 5=Website, 6=Original
            migrationBuilder.Sql(@"ALTER TABLE public.""Tag"" ALTER COLUMN ""SourceType"" TYPE varchar(50) USING CASE ""SourceType"" WHEN 1 THEN 'Family' WHEN 2 THEN 'Chef' WHEN 3 THEN 'Restaurant' WHEN 4 THEN 'Cookbook' WHEN 5 THEN 'Website' WHEN 6 THEN 'Original' ELSE NULL END;");

            // Tag.Category: 1=Dietary, 2=Cuisine, 3=MealType, 4=Source, 5=Custom
            migrationBuilder.Sql(@"ALTER TABLE public.""Tag"" ALTER COLUMN ""Category"" TYPE varchar(50) USING CASE ""Category"" WHEN 1 THEN 'Dietary' WHEN 2 THEN 'Cuisine' WHEN 3 THEN 'MealType' WHEN 4 THEN 'Source' WHEN 5 THEN 'Custom' ELSE 'Custom' END;");

            // RecipeTag.NormalizedEntityType (nullable): same as SourceType
            migrationBuilder.Sql(@"ALTER TABLE public.""RecipeTag"" ALTER COLUMN ""NormalizedEntityType"" TYPE varchar(50) USING CASE ""NormalizedEntityType"" WHEN 1 THEN 'Family' WHEN 2 THEN 'Chef' WHEN 3 THEN 'Restaurant' WHEN 4 THEN 'Cookbook' WHEN 5 THEN 'Website' WHEN 6 THEN 'Original' ELSE NULL END;");

            // MealPlanEntry.MealSlot: 1=Breakfast, 2=Lunch, 3=Dinner, 4=Snack
            migrationBuilder.Sql(@"ALTER TABLE public.""MealPlanEntry"" ALTER COLUMN ""MealSlot"" TYPE varchar(50) USING CASE ""MealSlot"" WHEN 1 THEN 'Breakfast' WHEN 2 THEN 'Lunch' WHEN 3 THEN 'Dinner' WHEN 4 THEN 'Snack' ELSE 'Dinner' END;");

            // ImportJob.Type: 1=Paprika, 2=UrlBatch, 3=MultiImage, 4=Export, 5=Video
            migrationBuilder.Sql(@"ALTER TABLE public.""ImportJob"" ALTER COLUMN ""Type"" TYPE varchar(50) USING CASE ""Type"" WHEN 1 THEN 'Paprika' WHEN 2 THEN 'UrlBatch' WHEN 3 THEN 'MultiImage' WHEN 4 THEN 'Export' WHEN 5 THEN 'Video' ELSE 'Paprika' END;");

            // ImportJob.Status: 1=Pending, 2=Processing, 3=Complete, 4=Failed
            migrationBuilder.Sql(@"ALTER TABLE public.""ImportJob"" ALTER COLUMN ""Status"" TYPE varchar(50) USING CASE ""Status"" WHEN 1 THEN 'Pending' WHEN 2 THEN 'Processing' WHEN 3 THEN 'Complete' WHEN 4 THEN 'Failed' ELSE 'Pending' END;");

            // Equipment.Category: 1=Appliance, 2=Cookware, 3=Bakeware, 4=Tool
            migrationBuilder.Sql(@"ALTER TABLE public.""Equipment"" ALTER COLUMN ""Category"" TYPE varchar(50) USING CASE ""Category"" WHEN 1 THEN 'Appliance' WHEN 2 THEN 'Cookware' WHEN 3 THEN 'Bakeware' WHEN 4 THEN 'Tool' ELSE 'Tool' END;");

            // CircleMember.Status: 0=Pending, 1=Active, 2=Left
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleMember"" ALTER COLUMN ""Status"" TYPE varchar(50) USING CASE ""Status"" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Active' WHEN 2 THEN 'Left' ELSE 'Active' END;");

            // CircleMember.Role: 0=Owner, 1=Admin, 2=Member
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleMember"" ALTER COLUMN ""Role"" TYPE varchar(50) USING CASE ""Role"" WHEN 0 THEN 'Owner' WHEN 1 THEN 'Admin' WHEN 2 THEN 'Member' ELSE 'Member' END;");

            // CircleInvite.Status: 0=Pending, 1=Accepted, 2=Expired, 3=Revoked
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleInvite"" ALTER COLUMN ""Status"" TYPE varchar(50) USING CASE ""Status"" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Accepted' WHEN 2 THEN 'Expired' WHEN 3 THEN 'Revoked' ELSE 'Pending' END;");

            // BetaInviteCodeRedemption.PreviousTier: 0=Free, 1=Premium, 2=Beta
            migrationBuilder.Sql(@"ALTER TABLE public.""BetaInviteCodeRedemption"" ALTER COLUMN ""PreviousTier"" TYPE varchar(50) USING CASE ""PreviousTier"" WHEN 0 THEN 'Free' WHEN 1 THEN 'Premium' WHEN 2 THEN 'Beta' ELSE 'Free' END;");

            // BetaInviteCodeRedemption.NewTier: 0=Free, 1=Premium, 2=Beta
            migrationBuilder.Sql(@"ALTER TABLE public.""BetaInviteCodeRedemption"" ALTER COLUMN ""NewTier"" TYPE varchar(50) USING CASE ""NewTier"" WHEN 0 THEN 'Free' WHEN 1 THEN 'Premium' WHEN 2 THEN 'Beta' ELSE 'Free' END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert string columns back to integer
            // Note: this loses the string values and maps back to ordinal positions
            migrationBuilder.Sql(@"ALTER TABLE public.""UserAccount"" ALTER COLUMN ""AccountTier"" TYPE integer USING CASE ""AccountTier"" WHEN 'Free' THEN 0 WHEN 'Premium' THEN 1 WHEN 'Beta' THEN 2 ELSE 0 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""Unit"" ALTER COLUMN ""Type"" TYPE integer USING CASE ""Type"" WHEN 'Volume' THEN 1 WHEN 'Weight' THEN 2 WHEN 'Count' THEN 3 WHEN 'Descriptive' THEN 4 ELSE 1 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""Tag"" ALTER COLUMN ""SourceType"" TYPE integer USING CASE ""SourceType"" WHEN 'Family' THEN 1 WHEN 'Chef' THEN 2 WHEN 'Restaurant' THEN 3 WHEN 'Cookbook' THEN 4 WHEN 'Website' THEN 5 WHEN 'Original' THEN 6 ELSE NULL END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""Tag"" ALTER COLUMN ""Category"" TYPE integer USING CASE ""Category"" WHEN 'Dietary' THEN 1 WHEN 'Cuisine' THEN 2 WHEN 'MealType' THEN 3 WHEN 'Source' THEN 4 WHEN 'Custom' THEN 5 ELSE 5 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""RecipeTag"" ALTER COLUMN ""NormalizedEntityType"" TYPE integer USING CASE ""NormalizedEntityType"" WHEN 'Family' THEN 1 WHEN 'Chef' THEN 2 WHEN 'Restaurant' THEN 3 WHEN 'Cookbook' THEN 4 WHEN 'Website' THEN 5 WHEN 'Original' THEN 6 ELSE NULL END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""MealPlanEntry"" ALTER COLUMN ""MealSlot"" TYPE integer USING CASE ""MealSlot"" WHEN 'Breakfast' THEN 1 WHEN 'Lunch' THEN 2 WHEN 'Dinner' THEN 3 WHEN 'Snack' THEN 4 ELSE 3 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""ImportJob"" ALTER COLUMN ""Type"" TYPE integer USING CASE ""Type"" WHEN 'Paprika' THEN 1 WHEN 'UrlBatch' THEN 2 WHEN 'MultiImage' THEN 3 WHEN 'Export' THEN 4 WHEN 'Video' THEN 5 ELSE 1 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""ImportJob"" ALTER COLUMN ""Status"" TYPE integer USING CASE ""Status"" WHEN 'Pending' THEN 1 WHEN 'Processing' THEN 2 WHEN 'Complete' THEN 3 WHEN 'Failed' THEN 4 ELSE 1 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""Equipment"" ALTER COLUMN ""Category"" TYPE integer USING CASE ""Category"" WHEN 'Appliance' THEN 1 WHEN 'Cookware' THEN 2 WHEN 'Bakeware' THEN 3 WHEN 'Tool' THEN 4 ELSE 4 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleMember"" ALTER COLUMN ""Status"" TYPE integer USING CASE ""Status"" WHEN 'Pending' THEN 0 WHEN 'Active' THEN 1 WHEN 'Left' THEN 2 ELSE 1 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleMember"" ALTER COLUMN ""Role"" TYPE integer USING CASE ""Role"" WHEN 'Owner' THEN 0 WHEN 'Admin' THEN 1 WHEN 'Member' THEN 2 ELSE 2 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""CircleInvite"" ALTER COLUMN ""Status"" TYPE integer USING CASE ""Status"" WHEN 'Pending' THEN 0 WHEN 'Accepted' THEN 1 WHEN 'Expired' THEN 2 WHEN 'Revoked' THEN 3 ELSE 0 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""BetaInviteCodeRedemption"" ALTER COLUMN ""PreviousTier"" TYPE integer USING CASE ""PreviousTier"" WHEN 'Free' THEN 0 WHEN 'Premium' THEN 1 WHEN 'Beta' THEN 2 ELSE 0 END;");
            migrationBuilder.Sql(@"ALTER TABLE public.""BetaInviteCodeRedemption"" ALTER COLUMN ""NewTier"" TYPE integer USING CASE ""NewTier"" WHEN 'Free' THEN 0 WHEN 'Premium' THEN 1 WHEN 'Beta' THEN 2 ELSE 0 END;");
        }
    }
}
