using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedEquipmentData : Migration
    {
        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime SeedDate = new DateTime(2026, 2, 26, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure system subject exists
            migrationBuilder.Sql($@"
                INSERT INTO public.""Subject"" (""SubjectId"", ""Name"", ""CreatedDate"")
                VALUES ('{SystemSubjectId}', 'System', '{SeedDate:yyyy-MM-dd HH:mm:ss}')
                ON CONFLICT (""SubjectId"") DO NOTHING;
            ");

            // Seed Appliances (Category = 1)
            InsertEquipment(migrationBuilder, 1, "Instant Pot / Pressure Cooker", "instant-pot", 1, "Electric pressure cooker for fast cooking", false);
            InsertEquipment(migrationBuilder, 2, "Slow Cooker / Crock-Pot", "slow-cooker", 1, "Slow cooker for low and slow cooking", false);
            InsertEquipment(migrationBuilder, 3, "Air Fryer", "air-fryer", 1, "Convection oven that crisps food with hot air", false);
            InsertEquipment(migrationBuilder, 4, "Stand Mixer", "stand-mixer", 1, "Heavy-duty mixer for baking", false);
            InsertEquipment(migrationBuilder, 5, "Food Processor", "food-processor", 1, "Multi-purpose food prep appliance", false);
            InsertEquipment(migrationBuilder, 6, "Blender", "blender", 1, "Blender for smoothies and pureeing", false);
            InsertEquipment(migrationBuilder, 7, "Immersion Blender", "immersion-blender", 1, "Hand-held blender for soups and sauces", false);
            InsertEquipment(migrationBuilder, 8, "Bread Machine", "bread-machine", 1, "Automated bread maker", false);
            InsertEquipment(migrationBuilder, 9, "Sous Vide", "sous-vide", 1, "Precision temperature cooking device", false);
            InsertEquipment(migrationBuilder, 10, "Deep Fryer", "deep-fryer", 1, "Electric deep fryer", false);
            InsertEquipment(migrationBuilder, 11, "Rice Cooker", "rice-cooker", 1, "Automated rice cooker", false);
            InsertEquipment(migrationBuilder, 12, "Waffle Iron", "waffle-iron", 1, "Waffle maker", false);
            InsertEquipment(migrationBuilder, 13, "Ice Cream Maker", "ice-cream-maker", 1, "Ice cream and frozen dessert maker", false);

            // Seed Cookware (Category = 2)
            InsertEquipment(migrationBuilder, 14, "Dutch Oven", "dutch-oven", 2, "Heavy pot for braising and stews", false);
            InsertEquipment(migrationBuilder, 15, "Cast Iron Skillet", "cast-iron", 2, "Cast iron pan for searing and baking", false);
            InsertEquipment(migrationBuilder, 16, "Wok", "wok", 2, "Asian-style stir-fry pan", false);
            InsertEquipment(migrationBuilder, 17, "Grill / Grill Pan", "grill", 2, "Outdoor grill or stovetop grill pan", false);
            InsertEquipment(migrationBuilder, 18, "Pizza Stone", "pizza-stone", 2, "Stone for baking crispy pizza crusts", false);
            InsertEquipment(migrationBuilder, 19, "Roasting Pan", "roasting-pan", 2, "Large pan for roasting meats", false);
            InsertEquipment(migrationBuilder, 20, "Steamer Basket", "steamer-basket", 2, "Basket for steaming vegetables", false);
            InsertEquipment(migrationBuilder, 21, "Double Boiler", "double-boiler", 2, "Two-piece pot for gentle heating", false);

            // Seed Bakeware (Category = 3)
            InsertEquipment(migrationBuilder, 22, "Bundt Pan", "bundt-pan", 3, "Ring-shaped cake pan", false);
            InsertEquipment(migrationBuilder, 23, "Springform Pan", "springform-pan", 3, "Removable-bottom pan for cheesecakes", false);
            InsertEquipment(migrationBuilder, 24, "Tart Pan", "tart-pan", 3, "Shallow pan with fluted sides", false);
            InsertEquipment(migrationBuilder, 25, "Pie Dish", "pie-dish", 3, "Deep dish for pies", false);
            InsertEquipment(migrationBuilder, 26, "Ramekins", "ramekins", 3, "Small individual baking dishes", false);
            InsertEquipment(migrationBuilder, 27, "Muffin Tin", "muffin-tin", 3, "Pan with multiple muffin cups", false);
            InsertEquipment(migrationBuilder, 28, "Loaf Pan", "loaf-pan", 3, "Rectangular pan for breads and loaves", false);

            // Seed Tools (Category = 4)
            InsertEquipment(migrationBuilder, 29, "Mandoline", "mandoline", 4, "Slicing tool for uniform cuts", false);
            InsertEquipment(migrationBuilder, 30, "Kitchen Scale", "kitchen-scale", 4, "Digital or analog scale for precise measurements", false);
            InsertEquipment(migrationBuilder, 31, "Meat Thermometer", "meat-thermometer", 4, "Thermometer for checking internal temps", false);
            InsertEquipment(migrationBuilder, 32, "Piping Bags", "piping-bags", 4, "Bags for decorating cakes and pastries", false);
            InsertEquipment(migrationBuilder, 33, "Culinary Torch", "torch", 4, "Torch for creme brulee and searing", false);
            InsertEquipment(migrationBuilder, 34, "Mortar & Pestle", "mortar-pestle", 4, "Tool for grinding spices and herbs", false);
        }

        private static void InsertEquipment(MigrationBuilder migrationBuilder, int equipmentId, string name, string code, int category, string description, bool isCommon)
        {
            var descValue = description != null ? $"'{description.Replace("'", "''")}'" : "NULL";
            var isCommonValue = isCommon ? "true" : "false";

            migrationBuilder.Sql($@"
                INSERT INTO public.""Equipment"" (""EquipmentId"", ""Name"", ""Code"", ""Category"", ""Description"", ""IsCommon"", ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                VALUES ({equipmentId}, '{name.Replace("'", "''")}', '{code}', {category}, {descValue}, {isCommonValue}, '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}', '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""UserEquipment"";");
            migrationBuilder.Sql(@"DELETE FROM public.""RecipeEquipment"";");
            migrationBuilder.Sql(@"DELETE FROM public.""Equipment"";");
        }
    }
}
