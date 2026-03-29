/**
 * Seed script to populate database with sample recipes
 * Run with: node seed-recipes.mjs
 *
 * NOTE: This script currently has NO authentication. To use with the real API,
 * you will need to add a Bearer token in the headers:
 *   headers: {
 *     'Authorization': 'Bearer <YOUR_TOKEN>',
 *     ...
 *   }
 */

import https from 'https';
import http from 'http';

const API_URL = 'http://localhost:5000/api/v1/recipes';
const FORCE_FLAG = process.argv.includes('--force');

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
  },
  // Mexican Cuisine
  {
    title: "Tacos al Pastor",
    description: "Tender marinated pork with pineapple, cilantro, and onions in warm tortillas.",
    yield: 8,
    prepTimeMinutes: 30,
    cookTimeMinutes: 15,
    source: "Mexico City Street Food",
    originalImageUrl: "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "lbs", item: "pork shoulder", preparation: "thinly sliced" },
      { sortOrder: 2, quantity: 4, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 3, quantity: 2, unit: "tbsp", item: "achiote paste" },
      { sortOrder: 4, quantity: 0.25, unit: "cup", item: "vinegar" },
      { sortOrder: 5, quantity: 2, unit: "tbsp", item: "olive oil" },
      { sortOrder: 6, quantity: 1, unit: "tbsp", item: "cumin" },
      { sortOrder: 7, quantity: 1, unit: "tbsp", item: "oregano" },
      { sortOrder: 8, quantity: 0.25, unit: "fresh pineapple", item: "pineapple", preparation: "sliced" },
      { sortOrder: 9, quantity: 16, unit: "", item: "corn tortillas" },
      { sortOrder: 10, quantity: 0.5, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 11, quantity: 0.5, unit: "cup", item: "fresh cilantro", preparation: "chopped" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Blend achiote paste, garlic, vinegar, and spices into a marinade." },
      { stepNumber: 2, instruction: "Coat pork slices with marinade and refrigerate for at least 2 hours." },
      { stepNumber: 3, instruction: "Heat oil in a large skillet over medium-high heat." },
      { stepNumber: 4, instruction: "Cook pork until browned and caramelized, about 10-15 minutes." },
      { stepNumber: 5, instruction: "Warm tortillas in a skillet or over an open flame." },
      { stepNumber: 6, instruction: "Assemble tacos with pork, pineapple slice, onion, and cilantro." }
    ]
  },
  {
    title: "Enchiladas Verdes",
    description: "Soft tortillas filled with chicken and covered in tangy green tomatillo sauce and melted cheese.",
    yield: 4,
    prepTimeMinutes: 25,
    cookTimeMinutes: 30,
    source: "Traditional Mexican Cooking",
    originalImageUrl: "https://images.unsplash.com/photo-1599022427431-42c91e8e0908?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "lbs", item: "chicken breast", preparation: "cooked and shredded" },
      { sortOrder: 2, quantity: 12, unit: "", item: "corn tortillas" },
      { sortOrder: 3, quantity: 2, unit: "cups", item: "salsa verde" },
      { sortOrder: 4, quantity: 2, unit: "cups", item: "Mexican cheese blend", preparation: "shredded" },
      { sortOrder: 5, quantity: 1, unit: "cup", item: "sour cream" },
      { sortOrder: 6, quantity: 0.5, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 7, quantity: 2, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 8, quantity: 1, unit: "", item: "jalapeño", preparation: "seeded and diced" },
      { sortOrder: 9, quantity: 0.25, unit: "cup", item: "fresh cilantro", preparation: "chopped" },
      { sortOrder: 10, quantity: 2, unit: "tbsp", item: "olive oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 350°F (175°C)." },
      { stepNumber: 2, instruction: "Mix shredded chicken with sour cream, jalapeño, and half the cilantro." },
      { stepNumber: 3, instruction: "Warm tortillas to make them pliable." },
      { stepNumber: 4, instruction: "Spread a thin layer of salsa verde on the bottom of a baking dish." },
      { stepNumber: 5, instruction: "Fill each tortilla with chicken mixture, roll, and place seam-side down in the dish." },
      { stepNumber: 6, instruction: "Pour remaining salsa verde over the enchiladas and top with cheese." },
      { stepNumber: 7, instruction: "Bake for 25-30 minutes until cheese is melted and bubbly." },
      { stepNumber: 8, instruction: "Garnish with remaining cilantro and serve." }
    ]
  },
  // Japanese Cuisine
  {
    title: "Chicken Teriyaki",
    description: "Glazed chicken with a sweet and savory Japanese sauce, served with steamed rice.",
    yield: 4,
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    source: "Japanese Home Cooking",
    originalImageUrl: "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1.5, unit: "lbs", item: "chicken breast", preparation: "cut into bite-sized pieces" },
      { sortOrder: 2, quantity: 0.5, unit: "cup", item: "soy sauce" },
      { sortOrder: 3, quantity: 0.25, unit: "cup", item: "mirin" },
      { sortOrder: 4, quantity: 2, unit: "tbsp", item: "sugar" },
      { sortOrder: 5, quantity: 1, unit: "tbsp", item: "ginger", preparation: "minced" },
      { sortOrder: 6, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 7, quantity: 2, unit: "tbsp", item: "vegetable oil" },
      { sortOrder: 8, quantity: 1, unit: "tbsp", item: "cornstarch" },
      { sortOrder: 9, quantity: 2, unit: "tbsp", item: "water" },
      { sortOrder: 10, quantity: 2, unit: "", item: "green onions", preparation: "sliced" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Mix soy sauce, mirin, sugar, ginger, and garlic in a bowl to create teriyaki sauce." },
      { stepNumber: 2, instruction: "Heat oil in a large skillet over medium-high heat." },
      { stepNumber: 3, instruction: "Cook chicken until golden and cooked through, about 8-10 minutes." },
      { stepNumber: 4, instruction: "Pour teriyaki sauce over chicken and simmer for 3-5 minutes." },
      { stepNumber: 5, instruction: "Mix cornstarch with water and add to the skillet to thicken sauce." },
      { stepNumber: 6, instruction: "Cook for another minute until sauce coats the chicken." },
      { stepNumber: 7, instruction: "Garnish with green onions and serve over steamed rice." }
    ]
  },
  {
    title: "Vegetable Fried Rice",
    description: "Day-old rice stir-fried with mixed vegetables, eggs, and soy sauce.",
    yield: 4,
    prepTimeMinutes: 15,
    cookTimeMinutes: 15,
    source: "Asian Kitchen Essentials",
    originalImageUrl: "https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 3, unit: "cups", item: "cooked rice", preparation: "day-old, chilled" },
      { sortOrder: 2, quantity: 3, unit: "", item: "large eggs", preparation: "beaten" },
      { sortOrder: 3, quantity: 1, unit: "cup", item: "mixed vegetables", preparation: "diced (peas, carrots, corn)" },
      { sortOrder: 4, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 5, quantity: 2, unit: "tbsp", item: "soy sauce" },
      { sortOrder: 6, quantity: 1, unit: "tbsp", item: "sesame oil" },
      { sortOrder: 7, quantity: 2, unit: "tbsp", item: "vegetable oil" },
      { sortOrder: 8, quantity: 0.5, unit: "tsp", item: "white pepper" },
      { sortOrder: 9, quantity: 3, unit: "", item: "green onions", preparation: "sliced" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Heat 1 tbsp oil in a large wok or skillet over high heat." },
      { stepNumber: 2, instruction: "Scramble the beaten eggs until cooked through, then remove and set aside." },
      { stepNumber: 3, instruction: "Add remaining oil and stir-fry garlic for 30 seconds." },
      { stepNumber: 4, instruction: "Add mixed vegetables and stir-fry for 2-3 minutes." },
      { stepNumber: 5, instruction: "Add rice, breaking up any clumps, and stir-fry for 3-4 minutes." },
      { stepNumber: 6, instruction: "Add soy sauce, sesame oil, white pepper, and cooked eggs." },
      { stepNumber: 7, instruction: "Toss everything together for 1-2 minutes." },
      { stepNumber: 8, instruction: "Top with green onions and serve hot." }
    ]
  },
  // Middle Eastern Cuisine
  {
    title: "Hummus",
    description: "Creamy chickpea and tahini dip with garlic, lemon juice, and olive oil.",
    yield: 2,
    prepTimeMinutes: 10,
    cookTimeMinutes: 0,
    source: "Levantine Kitchen",
    originalImageUrl: "https://images.unsplash.com/photo-1599599810694-b5ac4dd4e1b9?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "cans", item: "chickpeas", preparation: "drained and rinsed" },
      { sortOrder: 2, quantity: 0.33, unit: "cup", item: "tahini" },
      { sortOrder: 3, quantity: 3, unit: "cloves", item: "garlic" },
      { sortOrder: 4, quantity: 0.33, unit: "cup", item: "lemon juice", preparation: "fresh" },
      { sortOrder: 5, quantity: 0.25, unit: "cup", item: "olive oil" },
      { sortOrder: 6, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 7, quantity: 0.25, unit: "tsp", item: "cayenne pepper" },
      { sortOrder: 8, quantity: 2, unit: "tbsp", item: "water" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Add chickpeas, tahini, garlic, lemon juice, and salt to a food processor." },
      { stepNumber: 2, instruction: "Blend until smooth, scraping down the sides as needed." },
      { stepNumber: 3, instruction: "With the processor running, slowly drizzle in olive oil." },
      { stepNumber: 4, instruction: "Add water a little at a time to reach desired consistency." },
      { stepNumber: 5, instruction: "Taste and adjust lemon juice and salt as needed." },
      { stepNumber: 6, instruction: "Transfer to a serving bowl and dust with cayenne pepper." },
      { stepNumber: 7, instruction: "Serve with pita bread, vegetables, or crackers." }
    ]
  },
  {
    title: "Falafel",
    description: "Crispy chickpea fritters seasoned with Middle Eastern spices, perfect with tahini sauce.",
    yield: 12,
    prepTimeMinutes: 20,
    cookTimeMinutes: 20,
    source: "Street Food Favorites",
    originalImageUrl: "https://images.unsplash.com/photo-1601050690597-df0bea8c8546?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "cans", item: "chickpeas", preparation: "drained (not canned)" },
      { sortOrder: 2, quantity: 0.5, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 3, quantity: 0.25, unit: "cup", item: "fresh parsley", preparation: "chopped" },
      { sortOrder: 4, quantity: 0.25, unit: "cup", item: "fresh cilantro", preparation: "chopped" },
      { sortOrder: 5, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 6, quantity: 1, unit: "tbsp", item: "cumin" },
      { sortOrder: 7, quantity: 1, unit: "tsp", item: "coriander" },
      { sortOrder: 8, quantity: 0.5, unit: "tsp", item: "cayenne pepper" },
      { sortOrder: 9, quantity: 0.5, unit: "cup", item: "flour", preparation: "all-purpose" },
      { sortOrder: 10, quantity: 1, unit: "tsp", item: "baking powder" },
      { sortOrder: 11, quantity: "oil for frying", unit: "", item: "vegetable oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Pulse chickpeas with onion, parsley, cilantro, garlic, and spices in a food processor." },
      { stepNumber: 2, instruction: "Mix in flour and baking powder until the mixture holds together." },
      { stepNumber: 3, instruction: "Form mixture into balls or patties." },
      { stepNumber: 4, instruction: "Heat oil in a deep fryer or large pot to 350°F (175°C)." },
      { stepNumber: 5, instruction: "Fry falafels in batches until golden brown, about 3-4 minutes." },
      { stepNumber: 6, instruction: "Drain on paper towels and serve hot with tahini sauce or hummus." }
    ]
  },
  // Southern US Cuisine
  {
    title: "Jambalaya",
    description: "One-pot Louisiana rice dish with andouille sausage, chicken, and vegetables in a spiced tomato sauce.",
    yield: 6,
    prepTimeMinutes: 20,
    cookTimeMinutes: 35,
    source: "New Orleans Soul Food",
    originalImageUrl: "https://images.unsplash.com/photo-1609880290413-74f0a0176e23?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1, unit: "lb", item: "andouille sausage", preparation: "sliced" },
      { sortOrder: 2, quantity: 1, unit: "lb", item: "chicken breast", preparation: "diced" },
      { sortOrder: 3, quantity: 1, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 4, quantity: 1, unit: "cup", item: "celery", preparation: "diced" },
      { sortOrder: 5, quantity: 1, unit: "cup", item: "bell pepper", preparation: "diced" },
      { sortOrder: 6, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 7, quantity: 2, unit: "cups", item: "long-grain rice" },
      { sortOrder: 8, quantity: 1, unit: "can", item: "diced tomatoes" },
      { sortOrder: 9, quantity: 4, unit: "cups", item: "chicken broth" },
      { sortOrder: 10, quantity: 2, unit: "tsp", item: "Cajun seasoning" },
      { sortOrder: 11, quantity: 1, unit: "tsp", item: "thyme" },
      { sortOrder: 12, quantity: 2, unit: "tbsp", item: "olive oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Heat oil in a large pot over medium-high heat." },
      { stepNumber: 2, instruction: "Brown sausage and chicken, then remove and set aside." },
      { stepNumber: 3, instruction: "Sauté onion, celery, bell pepper, and garlic until softened, about 5 minutes." },
      { stepNumber: 4, instruction: "Add rice and toast for 2 minutes, stirring constantly." },
      { stepNumber: 5, instruction: "Return sausage and chicken to the pot." },
      { stepNumber: 6, instruction: "Add tomatoes, broth, Cajun seasoning, and thyme." },
      { stepNumber: 7, instruction: "Bring to a boil, then reduce heat and simmer covered for 20-25 minutes." },
      { stepNumber: 8, instruction: "Fluff with a fork and serve hot." }
    ]
  },
  {
    title: "Biscuits and Gravy",
    description: "Buttery biscuits smothered in creamy sausage gravy for a classic Southern breakfast.",
    yield: 8,
    prepTimeMinutes: 15,
    cookTimeMinutes: 25,
    source: "Southern Breakfast Classics",
    originalImageUrl: "https://images.unsplash.com/photo-1526668752670-639b8e8e0b37?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "cups", item: "all-purpose flour" },
      { sortOrder: 2, quantity: 1, unit: "tbsp", item: "baking powder" },
      { sortOrder: 3, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 4, quantity: 0.25, unit: "cup", item: "cold butter", preparation: "cubed" },
      { sortOrder: 5, quantity: 0.75, unit: "cup", item: "buttermilk" },
      { sortOrder: 6, quantity: 1, unit: "lb", item: "ground sausage" },
      { sortOrder: 7, quantity: 2, unit: "tbsp", item: "butter" },
      { sortOrder: 8, quantity: 2, unit: "tbsp", item: "all-purpose flour" },
      { sortOrder: 9, quantity: 2, unit: "cups", item: "whole milk" },
      { sortOrder: 10, quantity: 0.5, unit: "tsp", item: "black pepper" },
      { sortOrder: 11, quantity: 0.25, unit: "tsp", item: "salt" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 400°F (200°C)." },
      { stepNumber: 2, instruction: "Mix flour, baking powder, and salt. Cut in cold butter until crumbly." },
      { stepNumber: 3, instruction: "Stir in buttermilk just until dough comes together." },
      { stepNumber: 4, instruction: "Turn dough onto a floured surface and pat to 1-inch thickness." },
      { stepNumber: 5, instruction: "Cut into rounds and place on a baking sheet." },
      { stepNumber: 6, instruction: "Bake for 15 minutes until golden brown." },
      { stepNumber: 7, instruction: "Meanwhile, brown sausage in a skillet and drain off excess fat." },
      { stepNumber: 8, instruction: "Make a roux with butter and flour, cook for 1 minute." },
      { stepNumber: 9, instruction: "Gradually whisk in milk, stirring constantly until thickened." },
      { stepNumber: 10, instruction: "Season with black pepper and salt, add cooked sausage." },
      { stepNumber: 11, instruction: "Split biscuits and smother with sausage gravy." }
    ]
  },
  // French Cuisine
  {
    title: "Ratatouille",
    description: "Provençal vegetable stew with eggplant, zucchini, bell peppers, and tomatoes in herb-infused olive oil.",
    yield: 6,
    prepTimeMinutes: 20,
    cookTimeMinutes: 40,
    source: "French Country Cooking",
    originalImageUrl: "https://images.unsplash.com/photo-1585518419759-4e9f5b1b1b39?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 2, unit: "", item: "medium eggplants", preparation: "diced" },
      { sortOrder: 2, quantity: 2, unit: "", item: "medium zucchini", preparation: "diced" },
      { sortOrder: 3, quantity: 2, unit: "", item: "bell peppers", preparation: "diced" },
      { sortOrder: 4, quantity: 4, unit: "", item: "medium tomatoes", preparation: "diced" },
      { sortOrder: 5, quantity: 4, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 6, quantity: 0.5, unit: "cup", item: "olive oil" },
      { sortOrder: 7, quantity: 1, unit: "tbsp", item: "Herbes de Provence" },
      { sortOrder: 8, quantity: 1, unit: "tbsp", item: "balsamic vinegar" },
      { sortOrder: 9, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 10, quantity: 0.25, unit: "tsp", item: "black pepper" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Heat olive oil in a large pot over medium heat." },
      { stepNumber: 2, instruction: "Add garlic and Herbes de Provence, cook for 1 minute." },
      { stepNumber: 3, instruction: "Add eggplant and cook for 5 minutes." },
      { stepNumber: 4, instruction: "Add zucchini, bell peppers, and tomatoes." },
      { stepNumber: 5, instruction: "Stir in balsamic vinegar, salt, and pepper." },
      { stepNumber: 6, instruction: "Simmer uncovered for 30-35 minutes, stirring occasionally." },
      { stepNumber: 7, instruction: "Serve warm or at room temperature as a side dish or appetizer." }
    ]
  },
  {
    title: "Quiche Lorraine",
    description: "Classic French custard pie with bacon, onions, and gruyère cheese in a flaky crust.",
    yield: 6,
    prepTimeMinutes: 20,
    cookTimeMinutes: 40,
    source: "French Bistro Classics",
    originalImageUrl: "https://images.unsplash.com/photo-1565958011504-4b852f45cc82?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1, unit: "", item: "pie crust", preparation: "unbaked" },
      { sortOrder: 2, quantity: 8, unit: "oz", item: "bacon", preparation: "diced" },
      { sortOrder: 3, quantity: 1, unit: "cup", item: "onion", preparation: "sliced" },
      { sortOrder: 4, quantity: 1.5, unit: "cups", item: "gruyère cheese", preparation: "grated" },
      { sortOrder: 5, quantity: 4, unit: "", item: "large eggs" },
      { sortOrder: 6, quantity: 1.5, unit: "cups", item: "heavy cream" },
      { sortOrder: 7, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 8, quantity: 0.25, unit: "tsp", item: "white pepper" },
      { sortOrder: 9, quantity: 0.25, unit: "tsp", item: "nutmeg", preparation: "freshly grated" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 375°F (190°C)." },
      { stepNumber: 2, instruction: "Cook bacon until crispy, then drain and set aside." },
      { stepNumber: 3, instruction: "Sauté onions in bacon fat until caramelized." },
      { stepNumber: 4, instruction: "Scatter bacon, onions, and cheese over the pie crust." },
      { stepNumber: 5, instruction: "Whisk together eggs, heavy cream, salt, white pepper, and nutmeg." },
      { stepNumber: 6, instruction: "Pour egg mixture over bacon and cheese." },
      { stepNumber: 7, instruction: "Bake for 35-40 minutes until custard is set and top is golden." },
      { stepNumber: 8, instruction: "Cool for 5 minutes before slicing and serving." }
    ]
  },
  // Chinese Cuisine
  {
    title: "Kung Pao Chicken",
    description: "Stir-fried chicken with peanuts, bell peppers, and chilies in a tangy sauce.",
    yield: 4,
    prepTimeMinutes: 15,
    cookTimeMinutes: 15,
    source: "Sichuan Kitchen",
    originalImageUrl: "https://images.unsplash.com/photo-1612874742237-6526221588e3?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1.5, unit: "lbs", item: "chicken breast", preparation: "cut into cubes" },
      { sortOrder: 2, quantity: 0.75, unit: "cup", item: "roasted peanuts" },
      { sortOrder: 3, quantity: 2, unit: "", item: "bell peppers", preparation: "diced" },
      { sortOrder: 4, quantity: 3, unit: "", item: "dried red chilies", preparation: "whole" },
      { sortOrder: 5, quantity: 0.33, unit: "cup", item: "soy sauce" },
      { sortOrder: 6, quantity: 2, unit: "tbsp", item: "rice vinegar" },
      { sortOrder: 7, quantity: 1, unit: "tbsp", item: "sugar" },
      { sortOrder: 8, quantity: 1, unit: "tbsp", item: "sesame oil" },
      { sortOrder: 9, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 10, quantity: 1, unit: "tbsp", item: "ginger", preparation: "minced" },
      { sortOrder: 11, quantity: 3, unit: "tbsp", item: "vegetable oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Mix soy sauce, rice vinegar, sugar, and sesame oil for the sauce." },
      { stepNumber: 2, instruction: "Heat oil in a wok or large skillet over high heat." },
      { stepNumber: 3, instruction: "Cook chicken until golden, about 5-7 minutes. Remove and set aside." },
      { stepNumber: 4, instruction: "Add dried chilies and stir-fry for 30 seconds." },
      { stepNumber: 5, instruction: "Add garlic and ginger, stir-fry for 30 seconds." },
      { stepNumber: 6, instruction: "Add bell peppers and stir-fry for 2-3 minutes." },
      { stepNumber: 7, instruction: "Return chicken to the wok." },
      { stepNumber: 8, instruction: "Pour in sauce and add peanuts. Toss everything for 1-2 minutes." },
      { stepNumber: 9, instruction: "Serve immediately over steamed rice." }
    ]
  },
  {
    title: "Mapo Tofu",
    description: "Silky tofu cubes in a spicy, numbing Sichuan sauce with ground pork.",
    yield: 4,
    prepTimeMinutes: 10,
    cookTimeMinutes: 15,
    source: "Sichuan Province",
    originalImageUrl: "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 1, unit: "lb", item: "firm tofu", preparation: "cubed" },
      { sortOrder: 2, quantity: 0.5, unit: "lb", item: "ground pork" },
      { sortOrder: 3, quantity: 3, unit: "tbsp", item: "doubanjiang", preparation: "spicy bean paste" },
      { sortOrder: 4, quantity: 2, unit: "tbsp", item: "soy sauce" },
      { sortOrder: 5, quantity: 1, unit: "tbsp", item: "sesame oil" },
      { sortOrder: 6, quantity: 0.5, unit: "tsp", item: "Sichuan peppercorns", preparation: "ground" },
      { sortOrder: 7, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 8, quantity: 1, unit: "tbsp", item: "ginger", preparation: "minced" },
      { sortOrder: 9, quantity: 2, unit: "", item: "green onions", preparation: "sliced" },
      { sortOrder: 10, quantity: 1, unit: "tbsp", item: "cornstarch" },
      { sortOrder: 11, quantity: 0.5, unit: "cup", item: "chicken broth" },
      { sortOrder: 12, quantity: 2, unit: "tbsp", item: "vegetable oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Heat oil in a large skillet over medium-high heat." },
      { stepNumber: 2, instruction: "Brown ground pork, breaking it up as it cooks." },
      { stepNumber: 3, instruction: "Add doubanjiang paste and stir-fry for 1 minute." },
      { stepNumber: 4, instruction: "Add garlic and ginger, stir-fry for 30 seconds." },
      { stepNumber: 5, instruction: "Mix cornstarch with broth and add to the skillet with soy sauce." },
      { stepNumber: 6, instruction: "Gently add tofu cubes and simmer for 3-5 minutes." },
      { stepNumber: 7, instruction: "Stir in sesame oil and Sichuan peppercorns." },
      { stepNumber: 8, instruction: "Garnish with green onions and serve over rice." }
    ]
  },
  // Greek Cuisine
  {
    title: "Moussaka",
    description: "Layered eggplant and meat sauce topped with creamy béchamel sauce, baked until golden.",
    yield: 8,
    prepTimeMinutes: 30,
    cookTimeMinutes: 60,
    source: "Greek Mediterranean",
    originalImageUrl: "https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 3, unit: "lbs", item: "eggplants", preparation: "sliced lengthwise" },
      { sortOrder: 2, quantity: 1.5, unit: "lbs", item: "ground beef" },
      { sortOrder: 3, quantity: 1, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 4, quantity: 3, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 5, quantity: 1, unit: "can", item: "crushed tomatoes" },
      { sortOrder: 6, quantity: 0.25, unit: "cup", item: "red wine" },
      { sortOrder: 7, quantity: 1, unit: "tsp", item: "oregano" },
      { sortOrder: 8, quantity: 3, unit: "tbsp", item: "butter" },
      { sortOrder: 9, quantity: 3, unit: "tbsp", item: "all-purpose flour" },
      { sortOrder: 10, quantity: 2.5, unit: "cups", item: "whole milk" },
      { sortOrder: 11, quantity: 0.5, unit: "cup", item: "Parmesan cheese", preparation: "grated" },
      { sortOrder: 12, quantity: "olive oil for frying", unit: "", item: "olive oil" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 350°F (175°C)." },
      { stepNumber: 2, instruction: "Brush eggplant slices with oil and bake until tender, about 20 minutes." },
      { stepNumber: 3, instruction: "Brown beef with onion and garlic, then add tomatoes, wine, and oregano." },
      { stepNumber: 4, instruction: "Simmer meat sauce for 15 minutes." },
      { stepNumber: 5, instruction: "Make béchamel: melt butter, whisk in flour, gradually add milk." },
      { stepNumber: 6, instruction: "Stir in Parmesan cheese to béchamel." },
      { stepNumber: 7, instruction: "Layer in a baking dish: eggplant, meat sauce, eggplant, béchamel." },
      { stepNumber: 8, instruction: "Bake for 40-45 minutes until top is golden and filling is set." }
    ]
  },
  {
    title: "Spanakopita",
    description: "Greek spinach and feta pie wrapped in crispy phyllo dough.",
    yield: 6,
    prepTimeMinutes: 20,
    cookTimeMinutes: 30,
    source: "Greek Traditional Recipes",
    originalImageUrl: "https://images.unsplash.com/photo-1628191692879-7ab60b27f83f?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 10, unit: "oz", item: "frozen spinach", preparation: "thawed and drained" },
      { sortOrder: 2, quantity: 1.5, unit: "cups", item: "feta cheese", preparation: "crumbled" },
      { sortOrder: 3, quantity: 4, unit: "", item: "eggs" },
      { sortOrder: 4, quantity: 0.25, unit: "cup", item: "onion", preparation: "diced" },
      { sortOrder: 5, quantity: 1, unit: "tbsp", item: "fresh dill", preparation: "chopped" },
      { sortOrder: 6, quantity: 0.25, unit: "tsp", item: "nutmeg" },
      { sortOrder: 7, quantity: 1, unit: "pkg", item: "phyllo dough", preparation: "thawed" },
      { sortOrder: 8, quantity: 0.5, unit: "cup", item: "butter", preparation: "melted" },
      { sortOrder: 9, quantity: 0.5, unit: "tsp", item: "salt" },
      { sortOrder: 10, quantity: 0.25, unit: "tsp", item: "black pepper" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Preheat oven to 350°F (175°C)." },
      { stepNumber: 2, instruction: "Squeeze spinach dry and mix with feta, eggs, onion, dill, nutmeg, salt, and pepper." },
      { stepNumber: 3, instruction: "Brush a baking dish with melted butter." },
      { stepNumber: 4, instruction: "Layer 5-6 sheets of phyllo, brushing each with butter." },
      { stepNumber: 5, instruction: "Spread spinach mixture evenly over phyllo." },
      { stepNumber: 6, instruction: "Layer remaining phyllo sheets, brushing each with butter." },
      { stepNumber: 7, instruction: "Score the top into squares and bake for 25-30 minutes until golden." },
      { stepNumber: 8, instruction: "Cool for 5 minutes before serving." }
    ]
  },
  // Ethiopian Cuisine
  {
    title: "Doro Wat",
    description: "Spiced Ethiopian chicken stew with hard-boiled eggs, ginger, and aromatic spices.",
    yield: 6,
    prepTimeMinutes: 20,
    cookTimeMinutes: 45,
    source: "Ethiopian Highlands",
    originalImageUrl: "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=800",
    ingredients: [
      { sortOrder: 1, quantity: 3, unit: "lbs", item: "chicken", preparation: "cut into pieces" },
      { sortOrder: 2, quantity: 8, unit: "", item: "eggs", preparation: "hard-boiled and peeled" },
      { sortOrder: 3, quantity: 1, unit: "cup", item: "onion", preparation: "finely chopped" },
      { sortOrder: 4, quantity: 3, unit: "tbsp", item: "ginger", preparation: "minced" },
      { sortOrder: 5, quantity: 4, unit: "cloves", item: "garlic", preparation: "minced" },
      { sortOrder: 6, quantity: 2, unit: "tbsp", item: "berbere spice blend" },
      { sortOrder: 7, quantity: 0.5, unit: "cup", item: "clarified butter" },
      { sortOrder: 8, quantity: 0.5, unit: "cup", item: "tomato paste" },
      { sortOrder: 9, quantity: 1.5, unit: "cups", item: "chicken broth" },
      { sortOrder: 10, quantity: 1, unit: "tbsp", item: "honey" },
      { sortOrder: 11, quantity: 1, unit: "tsp", item: "salt" }
    ],
    instructions: [
      { stepNumber: 1, instruction: "Heat clarified butter in a large pot over medium heat." },
      { stepNumber: 2, instruction: "Cook onions until softened, about 5 minutes." },
      { stepNumber: 3, instruction: "Add ginger, garlic, and berbere spice. Cook for 2 minutes." },
      { stepNumber: 4, instruction: "Add chicken pieces and stir-fry until lightly browned." },
      { stepNumber: 5, instruction: "Stir in tomato paste and cook for 2 minutes." },
      { stepNumber: 6, instruction: "Add chicken broth, honey, and salt. Bring to a simmer." },
      { stepNumber: 7, instruction: "Cover and simmer for 30 minutes until chicken is tender." },
      { stepNumber: 8, instruction: "Add hard-boiled eggs and simmer for another 5 minutes." },
      { stepNumber: 9, instruction: "Serve over injera bread." }
    ]
  }
];

async function recipeExists(title) {
  return new Promise((resolve, reject) => {
    const encodedTitle = encodeURIComponent(title);
    const options = {
      hostname: 'localhost',
      port: 5000,
      path: `/api/v1/recipes?title=${encodedTitle}`,
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    };

    const req = http.request(options, (res) => {
      let responseData = '';

      res.on('data', (chunk) => {
        responseData += chunk;
      });

      res.on('end', () => {
        try {
          if (res.statusCode >= 200 && res.statusCode < 300) {
            const data = JSON.parse(responseData);
            // Check if any recipe with matching title exists
            const recipes = Array.isArray(data) ? data : data.items || [];
            resolve(recipes.length > 0);
          } else if (res.statusCode === 404) {
            resolve(false);
          } else {
            reject(new Error(`HTTP ${res.statusCode}: ${responseData}`));
          }
        } catch (e) {
          reject(e);
        }
      });
    });

    req.on('error', (error) => {
      reject(error);
    });

    req.end();
  });
}

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
  if (FORCE_FLAG) {
    console.log('⚠️  --force flag detected: skipping duplicate checks\n');
  }

  let successCount = 0;
  let skippedCount = 0;
  let failCount = 0;

  for (let i = 0; i < sampleRecipes.length; i++) {
    const recipe = sampleRecipes[i];
    try {
      // Check if recipe already exists (unless --force flag is set)
      if (!FORCE_FLAG) {
        const exists = await recipeExists(recipe.title);
        if (exists) {
          console.log(`⏭️  Skipped: ${recipe.title} (already exists)`);
          skippedCount++;
          // Small delay between requests
          await new Promise(resolve => setTimeout(resolve, 500));
          continue;
        }
      }

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

  console.log(`\n🎉 Seeding complete! Created: ${successCount}, Skipped: ${skippedCount}, Failed: ${failCount}`);
  process.exit(0);
}

seedDatabase().catch(error => {
  console.error('\n💥 Seeding failed:', error);
  process.exit(1);
});
