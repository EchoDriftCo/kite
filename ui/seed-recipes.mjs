/**
 * Seed script to populate database with sample recipes
 * Run with: node seed-recipes.mjs
 */

import https from 'https';
import http from 'http';

const API_URL = 'http://localhost:5000/api/v1/recipes';

const sampleRecipes = [
  {
    title: "Classic Chocolate Chip Cookies",
    description: "Soft and chewy chocolate chip cookies with a perfect golden-brown color.",
    yield: 24,
    prepTimeMinutes: 15,
    cookTimeMinutes: 12,
    source: "Grandma's Recipe Book",
    originalImageUrl: "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2.25, unit: "cups", item: "all-purpose flour" },
      { sortOrder: 2, quantity: 1, unit: "tsp", item: "baking soda" },
      { sortOrder: 3, quantity: 1, unit: "tsp", item: "salt" },
      { sortOrder: 4, quantity: 1, unit: "cup", item: "butter", preparation: "softened" },
      { sortOrder: 5, quantity: 0.75, unit: "cup", item: "granulated sugar" },
      { sortOrder: 6, quantity: 0.75, unit: "cup", item: "brown sugar", preparation: "packed" },
      { sortOrder: 7, quantity: 2, unit: "", item: "large eggs" },
      { sortOrder: 8, quantity: 2, unit: "tsp", item: "vanilla extract" },
      { sortOrder: 9, quantity: 2, unit: "cups", item: "chocolate chips" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 375°F (190°C)." },
      { stepNumber: 2, instruction: "Combine flour, baking soda, and salt in a small bowl." },
      { stepNumber: 3, instruction: "Beat butter, granulated sugar, and brown sugar until creamy." },
      { stepNumber: 4, instruction: "Add eggs and vanilla extract; beat well." },
      { stepNumber: 5, instruction: "Gradually blend in flour mixture." },
      { stepNumber: 6, instruction: "Stir in chocolate chips." },
      { stepNumber: 7, instruction: "Drop rounded tablespoons onto ungreased baking sheets." },
      { stepNumber: 8, instruction: "Bake for 9-11 minutes or until golden brown." },
      { stepNumber: 9, instruction: "Cool on baking sheets for 2 minutes; remove to wire racks." }
    ]
  },
  {
    title: "Spaghetti Carbonara",
    description: "Traditional Italian pasta dish with eggs, cheese, pancetta, and black pepper.",
    yield: 4,
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    source: "Italian Cooking 101",
    originalImageUrl: "https://images.unsplash.com/photo-1612874742237-6526221588e3?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1, unit: "lb", item: "spaghetti" },
      { sortOrder: 2, quantity: 6, unit: "oz", item: "pancetta", preparation: "diced" },
      { sortOrder: 3, quantity: 4, unit: "", item: "large eggs" },
      { sortOrder: 4, quantity: 1, unit: "cup", item: "Pecorino Romano cheese", preparation: "grated" },
      { sortOrder: 5, quantity: 1, unit: "tsp", item: "black pepper", preparation: "freshly ground" },
      { sortOrder: 6, quantity: 1, unit: "tsp", item: "salt" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Bring a large pot of salted water to boil and cook spaghetti according to package directions." },
      { stepNumber: 2, instruction: "While pasta cooks, fry pancetta in a large skillet over medium heat until crispy, about 8 minutes." },
      { stepNumber: 3, instruction: "In a bowl, whisk together eggs, Pecorino Romano, and black pepper." },
      { stepNumber: 4, instruction: "Reserve 1 cup of pasta water, then drain pasta." },
      { stepNumber: 5, instruction: "Add hot pasta to the skillet with pancetta and toss to combine." },
      { stepNumber: 6, instruction: "Remove from heat and quickly stir in egg mixture, adding pasta water as needed to create a creamy sauce." },
      { stepNumber: 7, instruction: "Serve immediately with extra cheese and black pepper." }
    ]
  },
  {
    title: "Chicken Tikka Masala",
    description: "Tender chicken pieces in a creamy tomato-based curry sauce with aromatic spices.",
    yield: 6,
    prepTimeMinutes: 30,
    cookTimeMinutes: 40,
    source: "Indian Kitchen Favorites",
    originalImageUrl: "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "lbs", item: "chicken breast", preparation: "cut into cubes" },
      { sortOrder: 2, quantity: 1, unit: "cup", item: "yogurt" },
      { sortOrder: 3, quantity: 2, unit: "tbsp", item: "lemon juice" },
      { sortOrder: 4, quantity: 2, unit: "tbsp", item: "garam masala" },
      { sortOrder: 5, quantity: 1, unit: "tbsp", item: "cumin" },
      { sortOrder: 6, quantity: 1, unit: "tbsp", item: "paprika" },
      { sortOrder: 7, quantity: 1, unit: "tbsp", item: "ginger", preparation: "minced" },
      { sortOrder: 8, quantity: 4, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 9, quantity: 1, unit: "can", item: "tomato sauce" },
      { sortOrder: 10, quantity: 1, unit: "cup", item: "heavy cream" },
      { sortOrder: 11, quantity: 2, unit: "tbsp", item: "butter" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Combine chicken, yogurt, lemon juice, and half the spices. Marinate for at least 1 hour." },
      { stepNumber: 2, instruction: "Heat butter in a large skillet. Add remaining spices, ginger, and garlic; cook for 1 minute." },
      { stepNumber: 3, instruction: "Add marinated chicken and cook until browned, about 8 minutes." },
      { stepNumber: 4, instruction: "Stir in tomato sauce and bring to a simmer." },
      { stepNumber: 5, instruction: "Reduce heat and simmer for 20 minutes." },
      { stepNumber: 6, instruction: "Stir in heavy cream and simmer for another 10 minutes." },
      { stepNumber: 7, instruction: "Serve over basmati rice with naan bread." }
    ]
  },
  {
    title: "Caesar Salad",
    description: "Classic romaine lettuce salad with creamy Caesar dressing, croutons, and parmesan.",
    yield: 4,
    prepTimeMinutes: 20,
    cookTimeMinutes: 0,
    source: "Salad Masterclass",
    originalImageUrl: "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "heads", item: "romaine lettuce", preparation: "chopped" },
      { sortOrder: 2, quantity: 0.5, unit: "cup", item: "Parmesan cheese", preparation: "freshly grated" },
      { sortOrder: 3, quantity: 2, unit: "cups", item: "croutons" },
      { sortOrder: 4, quantity: 2, unit: "", item: "egg yolks" },
      { sortOrder: 5, quantity: 2, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 6, quantity: 2, unit: "tbsp", item: "lemon juice" },
      { sortOrder: 7, quantity: 1, unit: "tsp", item: "Dijon mustard" },
      { sortOrder: 8, quantity: 0.75, unit: "cup", item: "olive oil" },
      { sortOrder: 9, quantity: 0.25, unit: "cup", item: "Parmesan cheese", preparation: "for dressing" },
      { sortOrder: 10, quantity: 1, unit: "tsp", item: "Worcestershire sauce" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Whisk together egg yolks, garlic, lemon juice, Dijon mustard, and Worcestershire sauce." },
      { stepNumber: 2, instruction: "Slowly drizzle in olive oil while whisking constantly until emulsified." },
      { stepNumber: 3, instruction: "Stir in grated Parmesan and season with salt and pepper." },
      { stepNumber: 4, instruction: "Toss romaine lettuce with dressing until well coated." },
      { stepNumber: 5, instruction: "Top with croutons and additional Parmesan cheese." },
      { stepNumber: 6, instruction: "Serve immediately." }
    ]
  },
  {
    title: "Blueberry Pancakes",
    description: "Fluffy buttermilk pancakes studded with fresh blueberries.",
    yield: 8,
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    source: "Breakfast Favorites",
    originalImageUrl: "https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "cups", item: "all-purpose flour" },
      { sortOrder: 2, quantity: 2, unit: "tbsp", item: "sugar" },
      { sortOrder: 3, quantity: 2, unit: "tsp", item: "baking powder" },
      { sortOrder: 4, quantity: 0.5, unit: "tsp", item: "baking soda" },
      { sortOrder: 5, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 6, quantity: 2, unit: "cups", item: "buttermilk" },
      { sortOrder: 7, quantity: 2, unit: "", item: "large eggs" },
      { sortOrder: 8, quantity: 0.25, unit: "cup", item: "butter", preparation: "melted" },
      { sortOrder: 9, quantity: 1, unit: "cup", item: "fresh blueberries" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Whisk together flour, sugar, baking powder, baking soda, and salt." },
      { stepNumber: 2, instruction: "In another bowl, whisk buttermilk, eggs, and melted butter." },
      { stepNumber: 3, instruction: "Pour wet ingredients into dry ingredients and mix until just combined." },
      { stepNumber: 4, instruction: "Gently fold in blueberries." },
      { stepNumber: 5, instruction: "Heat a griddle or large skillet over medium heat." },
      { stepNumber: 6, instruction: "Pour 1/4 cup batter for each pancake." },
      { stepNumber: 7, instruction: "Cook until bubbles form on surface, then flip and cook until golden." },
      { stepNumber: 8, instruction: "Serve warm with maple syrup and butter." }
    ]
  }
];

async function createRecipe(recipe) {
  return new Promise((resolve, reject) => {
    const data = JSON.stringify(recipe);
    
    const options = {
      hostname: 'localhost',
      port: 5000,
      path: '/api/v1/recipes',
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(data)
      }
    };

    const req = http.request(options, (res) => {
      let responseData = '';

      res.on('data', (chunk) => {
        responseData += chunk;
      });

      res.on('end', () => {
        if (res.statusCode >= 200 && res.statusCode < 300) {
          resolve(JSON.parse(responseData));
        } else {
          reject(new Error(`HTTP ${res.statusCode}: ${responseData}`));
        }
      });
    });

    req.on('error', (error) => {
      reject(error);
    });

    req.write(data);
    req.end();
  });
}

async function seedDatabase() {
  console.log('🌱 Seeding database with sample recipes...\n');

  let successCount = 0;
  let failCount = 0;

  for (let i = 0; i < sampleRecipes.length; i++) {
    const recipe = sampleRecipes[i];
    try {
      const created = await createRecipe(recipe);
      console.log(`✅ Created: ${recipe.title}`);
      successCount++;
    } catch (error) {
      console.error(`❌ Failed to create ${recipe.title}:`, error.message);
      failCount++;
    }

    // Small delay between requests
    await new Promise(resolve => setTimeout(resolve, 500));
  }

  console.log(`\n🎉 Seeding complete! Success: ${successCount}, Failed: ${failCount}`);
  process.exit(0);
}

seedDatabase().catch(error => {
  console.error('\n💥 Seeding failed:', error);
  process.exit(1);
});
