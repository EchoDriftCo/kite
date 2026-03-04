-- Equipment Seed Data Migration
-- 2026-03-03: 68 items across 4 categories
-- Run in Supabase SQL Editor

-- Ensure system subject exists
INSERT INTO public."Subject" ("SubjectId", "Name", "CreatedDate")
VALUES ('00000000-0000-0000-0000-000000000001', 'System', '2026-03-03 00:00:00')
ON CONFLICT ("SubjectId") DO NOTHING;

-- === APPLIANCES (Category: 1) ===
INSERT INTO public."Equipment" ("Name", "Code", "Category", "Description", "IsCommon", "CreatedDate", "CreatedSubjectId", "LastModifiedDate", "LastModifiedSubjectId") VALUES
('Oven', 'oven', 1, 'Standard kitchen oven', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Stove/Cooktop', 'stove', 1, 'Stovetop burners', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Microwave', 'microwave', 1, 'Microwave oven', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Toaster', 'toaster', 1, 'Bread toaster or toaster oven', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Blender', 'blender', 1, 'Countertop blender', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Immersion Blender', 'immersion-blender', 1, 'Handheld stick blender', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Food Processor', 'food-processor', 1, 'Electric food processor', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Stand Mixer', 'stand-mixer', 1, 'KitchenAid-style stand mixer', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Hand Mixer', 'hand-mixer', 1, 'Electric hand mixer/beaters', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Air Fryer', 'air-fryer', 1, 'Countertop air fryer', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Instant Pot', 'instant-pot', 1, 'Electric pressure cooker / multi-cooker', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Slow Cooker', 'slow-cooker', 1, 'Crock-Pot style slow cooker', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Rice Cooker', 'rice-cooker', 1, 'Electric rice cooker', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Sous Vide', 'sous-vide', 1, 'Immersion circulator for sous vide cooking', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Deep Fryer', 'deep-fryer', 1, 'Electric deep fryer', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Bread Machine', 'bread-machine', 1, 'Automatic bread maker', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Waffle Iron', 'waffle-iron', 1, 'Waffle maker', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Ice Cream Maker', 'ice-cream-maker', 1, 'Home ice cream machine', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Electric Griddle', 'electric-griddle', 1, 'Flat countertop griddle', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Electric Grill', 'electric-grill', 1, 'Indoor electric grill', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Outdoor Grill', 'outdoor-grill', 1, 'Gas or charcoal grill', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Smoker', 'smoker', 1, 'Meat smoker (electric, pellet, or charcoal)', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- === COOKWARE (Category: 2) ===
INSERT INTO public."Equipment" ("Name", "Code", "Category", "Description", "IsCommon", "CreatedDate", "CreatedSubjectId", "LastModifiedDate", "LastModifiedSubjectId") VALUES
('Skillet/Frying Pan', 'skillet', 2, 'Standard non-stick or stainless skillet', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Cast Iron Skillet', 'cast-iron-skillet', 2, 'Cast iron frying pan', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Saucepan', 'saucepan', 2, 'Small to medium saucepan with lid', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Stock Pot', 'stock-pot', 2, 'Large pot for soups, stocks, and boiling pasta', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Dutch Oven', 'dutch-oven', 2, 'Heavy lidded pot for braising and stews', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Saute Pan', 'saute-pan', 2, 'Wide flat-bottomed pan with straight sides', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Wok', 'wok', 2, 'Round-bottomed pan for stir-frying', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Grill Pan', 'grill-pan', 2, 'Stovetop pan with grill ridges', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Roasting Pan', 'roasting-pan', 2, 'Large pan for roasting meats', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Steamer Basket', 'steamer-basket', 2, 'Insert for steaming vegetables', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Double Boiler', 'double-boiler', 2, 'Two-pot system for gentle heating', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- === BAKEWARE (Category: 3) ===
INSERT INTO public."Equipment" ("Name", "Code", "Category", "Description", "IsCommon", "CreatedDate", "CreatedSubjectId", "LastModifiedDate", "LastModifiedSubjectId") VALUES
('Baking Sheet/Sheet Pan', 'baking-sheet', 3, 'Flat rimmed baking sheet', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('9x13 Baking Dish', '9x13-baking-dish', 3, 'Standard casserole/baking dish', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Muffin Pan', 'muffin-pan', 3, '12-cup muffin/cupcake tin', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Cake Pan (Round)', 'cake-pan-round', 3, '9-inch round cake pan', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Cake Pan (Square)', 'cake-pan-square', 3, '8 or 9-inch square cake pan', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Loaf Pan', 'loaf-pan', 3, 'Standard bread loaf pan', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Pie Dish', 'pie-dish', 3, '9-inch pie plate', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Bundt Pan', 'bundt-pan', 3, 'Decorative ring cake pan', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Springform Pan', 'springform-pan', 3, 'Pan with removable sides for cheesecake', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Pizza Stone/Steel', 'pizza-stone', 3, 'Stone or steel for crispy pizza crust', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Cooling Rack', 'cooling-rack', 3, 'Wire rack for cooling baked goods', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Ramekins', 'ramekins', 3, 'Small individual baking dishes', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Tart Pan', 'tart-pan', 3, 'Shallow pan with fluted edges and removable bottom', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- === TOOLS (Category: 4) ===
INSERT INTO public."Equipment" ("Name", "Code", "Category", "Description", "IsCommon", "CreatedDate", "CreatedSubjectId", "LastModifiedDate", "LastModifiedSubjectId") VALUES
('Chef''s Knife', 'chefs-knife', 4, 'All-purpose kitchen knife', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Cutting Board', 'cutting-board', 4, 'Wood or plastic cutting surface', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Mixing Bowls', 'mixing-bowls', 4, 'Set of assorted mixing bowls', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Whisk', 'whisk', 4, 'Wire whisk for beating and mixing', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Spatula', 'spatula', 4, 'Rubber or silicone spatula', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Wooden Spoon', 'wooden-spoon', 4, 'Classic wooden stirring spoon', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Tongs', 'tongs', 4, 'Kitchen tongs for gripping and flipping', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Measuring Cups', 'measuring-cups', 4, 'Dry and liquid measuring cups', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Measuring Spoons', 'measuring-spoons', 4, 'Set of measuring spoons', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Colander', 'colander', 4, 'Strainer for draining pasta and washing produce', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Peeler', 'peeler', 4, 'Vegetable peeler', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Grater', 'grater', 4, 'Box grater or microplane', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Can Opener', 'can-opener', 4, 'Manual or electric can opener', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Rolling Pin', 'rolling-pin', 4, 'For rolling dough', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Meat Thermometer', 'meat-thermometer', 4, 'Instant-read or probe thermometer', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Kitchen Scale', 'kitchen-scale', 4, 'Digital scale for precise measurements', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Mandoline', 'mandoline', 4, 'Adjustable slicer for uniform cuts', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Mortar & Pestle', 'mortar-pestle', 4, 'For grinding spices and making pastes', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Kitchen Torch', 'kitchen-torch', 4, 'Culinary torch for brulee and finishing', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Piping Bags & Tips', 'piping-bags', 4, 'For decorating cakes and pastries', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Fine Mesh Strainer', 'fine-mesh-strainer', 4, 'For straining sauces and sifting flour', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Ladle', 'ladle', 4, 'Deep spoon for serving soups and stews', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Slotted Spoon', 'slotted-spoon', 4, 'Spoon with holes for draining', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Paring Knife', 'paring-knife', 4, 'Small knife for detailed cutting', true, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001'),
('Bread Knife', 'bread-knife', 4, 'Serrated knife for slicing bread', false, '2026-03-03', '00000000-0000-0000-0000-000000000001', '2026-03-03', '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- Record migration
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303180000_SeedEquipment', '8.0.2')
ON CONFLICT DO NOTHING;
