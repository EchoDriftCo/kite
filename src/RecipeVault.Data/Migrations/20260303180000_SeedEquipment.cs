using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeVault.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedEquipment : Migration
    {
        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime SeedDate = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc);

        // EquipmentCategory enum values
        private const int Appliance = 1;
        private const int Cookware = 2;
        private const int Bakeware = 3;
        private const int Tool = 4;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === APPLIANCES ===
            InsertEquipment(migrationBuilder, "Oven", "oven", Appliance, "Standard kitchen oven", true);
            InsertEquipment(migrationBuilder, "Stove/Cooktop", "stove", Appliance, "Stovetop burners", true);
            InsertEquipment(migrationBuilder, "Microwave", "microwave", Appliance, "Microwave oven", true);
            InsertEquipment(migrationBuilder, "Toaster", "toaster", Appliance, "Bread toaster or toaster oven", true);
            InsertEquipment(migrationBuilder, "Blender", "blender", Appliance, "Countertop blender", false);
            InsertEquipment(migrationBuilder, "Immersion Blender", "immersion-blender", Appliance, "Handheld stick blender", false);
            InsertEquipment(migrationBuilder, "Food Processor", "food-processor", Appliance, "Electric food processor", false);
            InsertEquipment(migrationBuilder, "Stand Mixer", "stand-mixer", Appliance, "KitchenAid-style stand mixer", false);
            InsertEquipment(migrationBuilder, "Hand Mixer", "hand-mixer", Appliance, "Electric hand mixer/beaters", false);
            InsertEquipment(migrationBuilder, "Air Fryer", "air-fryer", Appliance, "Countertop air fryer", false);
            InsertEquipment(migrationBuilder, "Instant Pot", "instant-pot", Appliance, "Electric pressure cooker / multi-cooker", false);
            InsertEquipment(migrationBuilder, "Slow Cooker", "slow-cooker", Appliance, "Crock-Pot style slow cooker", false);
            InsertEquipment(migrationBuilder, "Rice Cooker", "rice-cooker", Appliance, "Electric rice cooker", false);
            InsertEquipment(migrationBuilder, "Sous Vide", "sous-vide", Appliance, "Immersion circulator for sous vide cooking", false);
            InsertEquipment(migrationBuilder, "Deep Fryer", "deep-fryer", Appliance, "Electric deep fryer", false);
            InsertEquipment(migrationBuilder, "Bread Machine", "bread-machine", Appliance, "Automatic bread maker", false);
            InsertEquipment(migrationBuilder, "Waffle Iron", "waffle-iron", Appliance, "Waffle maker", false);
            InsertEquipment(migrationBuilder, "Ice Cream Maker", "ice-cream-maker", Appliance, "Home ice cream machine", false);
            InsertEquipment(migrationBuilder, "Electric Griddle", "electric-griddle", Appliance, "Flat countertop griddle", false);
            InsertEquipment(migrationBuilder, "Electric Grill", "electric-grill", Appliance, "Indoor electric grill (e.g., George Foreman)", false);
            InsertEquipment(migrationBuilder, "Outdoor Grill", "outdoor-grill", Appliance, "Gas or charcoal grill", false);
            InsertEquipment(migrationBuilder, "Smoker", "smoker", Appliance, "Meat smoker (electric, pellet, or charcoal)", false);

            // === COOKWARE ===
            InsertEquipment(migrationBuilder, "Skillet/Frying Pan", "skillet", Cookware, "Standard non-stick or stainless skillet", true);
            InsertEquipment(migrationBuilder, "Cast Iron Skillet", "cast-iron-skillet", Cookware, "Cast iron frying pan", false);
            InsertEquipment(migrationBuilder, "Saucepan", "saucepan", Cookware, "Small to medium saucepan with lid", true);
            InsertEquipment(migrationBuilder, "Stock Pot", "stock-pot", Cookware, "Large pot for soups, stocks, and boiling pasta", true);
            InsertEquipment(migrationBuilder, "Dutch Oven", "dutch-oven", Cookware, "Heavy lidded pot for braising and stews", false);
            InsertEquipment(migrationBuilder, "Saute Pan", "saute-pan", Cookware, "Wide flat-bottomed pan with straight sides", false);
            InsertEquipment(migrationBuilder, "Wok", "wok", Cookware, "Round-bottomed pan for stir-frying", false);
            InsertEquipment(migrationBuilder, "Grill Pan", "grill-pan", Cookware, "Stovetop pan with grill ridges", false);
            InsertEquipment(migrationBuilder, "Roasting Pan", "roasting-pan", Cookware, "Large pan for roasting meats", false);
            InsertEquipment(migrationBuilder, "Steamer Basket", "steamer-basket", Cookware, "Insert for steaming vegetables", false);
            InsertEquipment(migrationBuilder, "Double Boiler", "double-boiler", Cookware, "Two-pot system for gentle heating (chocolate, sauces)", false);

            // === BAKEWARE ===
            InsertEquipment(migrationBuilder, "Baking Sheet/Sheet Pan", "baking-sheet", Bakeware, "Flat rimmed baking sheet", true);
            InsertEquipment(migrationBuilder, "9x13 Baking Dish", "9x13-baking-dish", Bakeware, "Standard casserole/baking dish", true);
            InsertEquipment(migrationBuilder, "Muffin Pan", "muffin-pan", Bakeware, "12-cup muffin/cupcake tin", false);
            InsertEquipment(migrationBuilder, "Cake Pan (Round)", "cake-pan-round", Bakeware, "9-inch round cake pan", false);
            InsertEquipment(migrationBuilder, "Cake Pan (Square)", "cake-pan-square", Bakeware, "8 or 9-inch square cake pan", false);
            InsertEquipment(migrationBuilder, "Loaf Pan", "loaf-pan", Bakeware, "Standard bread loaf pan", false);
            InsertEquipment(migrationBuilder, "Pie Dish", "pie-dish", Bakeware, "9-inch pie plate", false);
            InsertEquipment(migrationBuilder, "Bundt Pan", "bundt-pan", Bakeware, "Decorative ring cake pan", false);
            InsertEquipment(migrationBuilder, "Springform Pan", "springform-pan", Bakeware, "Pan with removable sides for cheesecake", false);
            InsertEquipment(migrationBuilder, "Pizza Stone/Steel", "pizza-stone", Bakeware, "Stone or steel for crispy pizza crust", false);
            InsertEquipment(migrationBuilder, "Cooling Rack", "cooling-rack", Bakeware, "Wire rack for cooling baked goods", true);
            InsertEquipment(migrationBuilder, "Ramekins", "ramekins", Bakeware, "Small individual baking dishes", false);
            InsertEquipment(migrationBuilder, "Tart Pan", "tart-pan", Bakeware, "Shallow pan with fluted edges and removable bottom", false);

            // === TOOLS ===
            InsertEquipment(migrationBuilder, "Chef's Knife", "chefs-knife", Tool, "All-purpose kitchen knife", true);
            InsertEquipment(migrationBuilder, "Cutting Board", "cutting-board", Tool, "Wood or plastic cutting surface", true);
            InsertEquipment(migrationBuilder, "Mixing Bowls", "mixing-bowls", Tool, "Set of assorted mixing bowls", true);
            InsertEquipment(migrationBuilder, "Whisk", "whisk", Tool, "Wire whisk for beating and mixing", true);
            InsertEquipment(migrationBuilder, "Spatula", "spatula", Tool, "Rubber or silicone spatula", true);
            InsertEquipment(migrationBuilder, "Wooden Spoon", "wooden-spoon", Tool, "Classic wooden stirring spoon", true);
            InsertEquipment(migrationBuilder, "Tongs", "tongs", Tool, "Kitchen tongs for gripping and flipping", true);
            InsertEquipment(migrationBuilder, "Measuring Cups", "measuring-cups", Tool, "Dry and liquid measuring cups", true);
            InsertEquipment(migrationBuilder, "Measuring Spoons", "measuring-spoons", Tool, "Set of measuring spoons", true);
            InsertEquipment(migrationBuilder, "Colander", "colander", Tool, "Strainer for draining pasta and washing produce", true);
            InsertEquipment(migrationBuilder, "Peeler", "peeler", Tool, "Vegetable peeler", true);
            InsertEquipment(migrationBuilder, "Grater", "grater", Tool, "Box grater or microplane", true);
            InsertEquipment(migrationBuilder, "Can Opener", "can-opener", Tool, "Manual or electric can opener", true);
            InsertEquipment(migrationBuilder, "Rolling Pin", "rolling-pin", Tool, "For rolling dough", false);
            InsertEquipment(migrationBuilder, "Meat Thermometer", "meat-thermometer", Tool, "Instant-read or probe thermometer", false);
            InsertEquipment(migrationBuilder, "Kitchen Scale", "kitchen-scale", Tool, "Digital scale for precise measurements", false);
            InsertEquipment(migrationBuilder, "Mandoline", "mandoline", Tool, "Adjustable slicer for uniform cuts", false);
            InsertEquipment(migrationBuilder, "Mortar & Pestle", "mortar-pestle", Tool, "For grinding spices and making pastes", false);
            InsertEquipment(migrationBuilder, "Kitchen Torch", "kitchen-torch", Tool, "Culinary torch for brulee and finishing", false);
            InsertEquipment(migrationBuilder, "Piping Bags & Tips", "piping-bags", Tool, "For decorating cakes and pastries", false);
            InsertEquipment(migrationBuilder, "Fine Mesh Strainer", "fine-mesh-strainer", Tool, "For straining sauces, sifting flour", false);
            InsertEquipment(migrationBuilder, "Ladle", "ladle", Tool, "Deep spoon for serving soups and stews", true);
            InsertEquipment(migrationBuilder, "Slotted Spoon", "slotted-spoon", Tool, "Spoon with holes for draining", true);
            InsertEquipment(migrationBuilder, "Paring Knife", "paring-knife", Tool, "Small knife for detailed cutting", true);
            InsertEquipment(migrationBuilder, "Bread Knife", "bread-knife", Tool, "Serrated knife for slicing bread", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""Equipment"" WHERE ""Code"" IN (
                'oven','stove','microwave','toaster','blender','immersion-blender','food-processor',
                'stand-mixer','hand-mixer','air-fryer','instant-pot','slow-cooker','rice-cooker',
                'sous-vide','deep-fryer','bread-machine','waffle-iron','ice-cream-maker',
                'electric-griddle','electric-grill','outdoor-grill','smoker',
                'skillet','cast-iron-skillet','saucepan','stock-pot','dutch-oven','saute-pan',
                'wok','grill-pan','roasting-pan','steamer-basket','double-boiler',
                'baking-sheet','9x13-baking-dish','muffin-pan','cake-pan-round','cake-pan-square',
                'loaf-pan','pie-dish','bundt-pan','springform-pan','pizza-stone','cooling-rack',
                'ramekins','tart-pan',
                'chefs-knife','cutting-board','mixing-bowls','whisk','spatula','wooden-spoon',
                'tongs','measuring-cups','measuring-spoons','colander','peeler','grater',
                'can-opener','rolling-pin','meat-thermometer','kitchen-scale','mandoline',
                'mortar-pestle','kitchen-torch','piping-bags','fine-mesh-strainer','ladle',
                'slotted-spoon','paring-knife','bread-knife'
            );");
        }

        private static void InsertEquipment(MigrationBuilder migrationBuilder, string name, string code,
            int category, string description, bool isCommon)
        {
            migrationBuilder.Sql($@"
                INSERT INTO public.""Equipment"" (""Name"", ""Code"", ""Category"", ""Description"", ""IsCommon"",
                    ""CreatedDate"", ""CreatedSubjectId"", ""LastModifiedDate"", ""LastModifiedSubjectId"")
                VALUES ('{name.Replace("'", "''")}', '{code}', {category}, '{description.Replace("'", "''")}', {isCommon},
                    '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}',
                    '{SeedDate:yyyy-MM-dd HH:mm:ss}', '{SystemSubjectId}')
                ON CONFLICT DO NOTHING;
            ");
        }
    }
}
