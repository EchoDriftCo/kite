using System;
using System.Collections.Generic;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data {
    /// <summary>
    /// Sample recipe seed data for the onboarding flow.
    /// Each recipe showcases a specific RecipeVault feature.
    /// Owned by the system account (d290f1ee-6c54-5f96-8b2f-9f14e72c8c39).
    /// </summary>
    public static class SampleRecipeSeedData {
        public static readonly Guid SystemSubjectId = Guid.Parse("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39");

        /// <summary>
        /// Creates the 6 sample recipes with full ingredients and instructions.
        /// These are inserted as system-owned recipes; users fork them during onboarding.
        /// </summary>
        public static List<SampleRecipeDefinition> GetSampleRecipes() {
            return new List<SampleRecipeDefinition> {
                CreateThaiBasilChicken(),
                CreateShakshuka(),
                CreateBibimbap(),
                CreateSpicyBibimbapFork(),
                CreateLemonSalmon(),
                CreateChickpeaStew()
            };
        }

        private static SampleRecipeDefinition CreateThaiBasilChicken() {
            return new SampleRecipeDefinition {
                Title = "Thai Basil Chicken (Pad Kra Pao)",
                Description = "Spicy, savory Thai stir-fry showcasing RecipeVault's ingredient substitution feature. Pre-loaded with 5 substitution options for hard-to-find ingredients.",
                Yield = 4,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 15,
                Showcases = "ingredient-substitutions",
                Tags = new[] { "Thai", "Stir-Fry", "Spicy", "Quick", "Poultry" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 1m, "lb", "ground chicken", null, "1 lb ground chicken"),
                    new(2, 3m, "tbsp", "fish sauce", null, "3 tbsp fish sauce"),
                    new(3, 1m, "tbsp", "oyster sauce", null, "1 tbsp oyster sauce"),
                    new(4, 1m, "tbsp", "soy sauce", null, "1 tbsp soy sauce"),
                    new(5, 1m, "tsp", "sugar", null, "1 tsp sugar"),
                    new(6, 1m, "cup", "Thai basil leaves", "packed", "1 cup Thai basil leaves, packed"),
                    new(7, 4m, null, "cloves garlic", "minced", "4 cloves garlic, minced"),
                    new(8, 3m, null, "Thai bird's eye chilies", "minced", "3 Thai bird's eye chilies, minced"),
                    new(9, 2m, "tbsp", "vegetable oil", null, "2 tbsp vegetable oil"),
                    new(10, 4m, null, "eggs", "fried, for serving", "4 eggs, fried, for serving"),
                    new(11, null, null, "jasmine rice", "for serving", "jasmine rice, for serving"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Mix fish sauce, oyster sauce, soy sauce, and sugar in a small bowl. Set aside."),
                    new(2, "Heat oil in a wok or large skillet over high heat until smoking."),
                    new(3, "Add garlic and chilies, stir-fry for 30 seconds until fragrant."),
                    new(4, "Add ground chicken, breaking it apart. Cook for 3-4 minutes until no longer pink."),
                    new(5, "Pour in the sauce mixture and stir to coat evenly. Cook for 2 minutes."),
                    new(6, "Remove from heat and fold in Thai basil leaves until just wilted."),
                    new(7, "Serve over jasmine rice topped with a fried egg."),
                }
            };
        }

        private static SampleRecipeDefinition CreateShakshuka() {
            return new SampleRecipeDefinition {
                Title = "Shakshuka with Feta",
                Description = "North African poached eggs in spiced tomato sauce. Perfect for showcasing Cooking Mode with 3 built-in timers for each cooking phase.",
                Yield = 4,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 25,
                Showcases = "cooking-mode",
                Tags = new[] { "Mediterranean", "Breakfast", "Vegetarian", "One-Pan", "Eggs" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 2m, "tbsp", "olive oil", null, "2 tbsp olive oil"),
                    new(2, 1m, null, "large onion", "diced", "1 large onion, diced"),
                    new(3, 1m, null, "red bell pepper", "diced", "1 red bell pepper, diced"),
                    new(4, 4m, null, "cloves garlic", "minced", "4 cloves garlic, minced"),
                    new(5, 1m, "tsp", "cumin", null, "1 tsp cumin"),
                    new(6, 1m, "tsp", "paprika", null, "1 tsp paprika"),
                    new(7, 0.5m, "tsp", "chili flakes", null, "1/2 tsp chili flakes"),
                    new(8, 1m, null, "28 oz can crushed tomatoes", null, "1 (28 oz) can crushed tomatoes"),
                    new(9, 6m, null, "eggs", null, "6 eggs"),
                    new(10, 0.5m, "cup", "feta cheese", "crumbled", "1/2 cup feta cheese, crumbled"),
                    new(11, null, null, "fresh parsley", "chopped, for garnish", "fresh parsley, chopped, for garnish"),
                    new(12, null, null, "crusty bread", "for serving", "crusty bread, for serving"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Heat olive oil in a large oven-safe skillet over medium heat."),
                    new(2, "Saute onion and bell pepper for 5 minutes until softened."),
                    new(3, "Add garlic, cumin, paprika, and chili flakes. Cook for 1 minute until fragrant."),
                    new(4, "Pour in crushed tomatoes, season with salt and pepper. Simmer for 5 minutes until slightly thickened. [TIMER: 5 min]"),
                    new(5, "Make 6 small wells in the sauce. Crack an egg into each well."),
                    new(6, "Cover and cook for 6 minutes until egg whites are set but yolks are still runny. [TIMER: 6 min]"),
                    new(7, "Remove from heat. Sprinkle with feta and parsley."),
                    new(8, "Let rest for 2 minutes before serving with crusty bread. [TIMER: 2 min]"),
                }
            };
        }

        private static SampleRecipeDefinition CreateBibimbap() {
            return new SampleRecipeDefinition {
                Title = "Korean Bibimbap Bowl",
                Description = "Classic Korean mixed rice bowl with complete nutritional data. Showcases RecipeVault's nutrition tracking with full macro breakdown: 520 cal, 28g protein, 62g carbs, 18g fat per serving.",
                Yield = 4,
                PrepTimeMinutes = 20,
                CookTimeMinutes = 30,
                Showcases = "nutrition",
                Tags = new[] { "Korean", "Rice Bowl", "Healthy", "High-Protein", "Colorful" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 2m, "cups", "short-grain rice", "cooked", "2 cups short-grain rice, cooked"),
                    new(2, 0.5m, "lb", "ground beef", null, "1/2 lb ground beef"),
                    new(3, 2m, "tbsp", "soy sauce", null, "2 tbsp soy sauce"),
                    new(4, 1m, "tbsp", "sesame oil", null, "1 tbsp sesame oil"),
                    new(5, 1m, null, "carrot", "julienned", "1 carrot, julienned"),
                    new(6, 1m, null, "zucchini", "julienned", "1 zucchini, julienned"),
                    new(7, 2m, "cups", "spinach", null, "2 cups spinach"),
                    new(8, 1m, "cup", "bean sprouts", null, "1 cup bean sprouts"),
                    new(9, 4m, null, "eggs", null, "4 eggs"),
                    new(10, 2m, "tbsp", "gochujang", null, "2 tbsp gochujang"),
                    new(11, 1m, "tbsp", "rice vinegar", null, "1 tbsp rice vinegar"),
                    new(12, 1m, "tbsp", "sesame seeds", "for garnish", "1 tbsp sesame seeds, for garnish"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Cook rice according to package directions. Keep warm."),
                    new(2, "Season ground beef with soy sauce and sesame oil. Cook in a skillet over medium-high heat until browned, about 5 minutes."),
                    new(3, "Blanch spinach in boiling water for 30 seconds, drain and squeeze dry. Season with a dash of sesame oil."),
                    new(4, "Saute carrots in a hot pan with a little oil for 2 minutes. Set aside."),
                    new(5, "Saute zucchini in the same pan for 2 minutes. Set aside."),
                    new(6, "Blanch bean sprouts for 1 minute, drain well."),
                    new(7, "Fry eggs sunny-side up in a non-stick pan."),
                    new(8, "Mix gochujang with rice vinegar to make the sauce."),
                    new(9, "Assemble bowls: rice on the bottom, arrange vegetables and beef in sections on top. Place a fried egg in the center."),
                    new(10, "Serve with gochujang sauce and sesame seeds. Mix everything together before eating."),
                }
            };
        }

        private static SampleRecipeDefinition CreateSpicyBibimbapFork() {
            return new SampleRecipeDefinition {
                Title = "Spicy Bibimbap",
                Description = "A fork of Korean Bibimbap with upgraded gochujang sauce for extra heat. Demonstrates RecipeVault's recipe forking feature — your changes don't affect the original.",
                Yield = 4,
                PrepTimeMinutes = 20,
                CookTimeMinutes = 30,
                Showcases = "recipe-forking",
                IsForkedFromBibimbap = true,
                Tags = new[] { "Korean", "Rice Bowl", "Spicy", "High-Protein", "Fork" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 2m, "cups", "short-grain rice", "cooked", "2 cups short-grain rice, cooked"),
                    new(2, 0.5m, "lb", "ground beef", null, "1/2 lb ground beef"),
                    new(3, 2m, "tbsp", "soy sauce", null, "2 tbsp soy sauce"),
                    new(4, 1m, "tbsp", "sesame oil", null, "1 tbsp sesame oil"),
                    new(5, 1m, null, "carrot", "julienned", "1 carrot, julienned"),
                    new(6, 1m, null, "zucchini", "julienned", "1 zucchini, julienned"),
                    new(7, 2m, "cups", "spinach", null, "2 cups spinach"),
                    new(8, 1m, "cup", "bean sprouts", null, "1 cup bean sprouts"),
                    new(9, 4m, null, "eggs", null, "4 eggs"),
                    new(10, 3m, "tbsp", "gochujang", null, "3 tbsp gochujang"),
                    new(11, 1m, "tbsp", "gochugaru (Korean chili flakes)", null, "1 tbsp gochugaru (Korean chili flakes)"),
                    new(12, 1m, "tbsp", "rice vinegar", null, "1 tbsp rice vinegar"),
                    new(13, 1m, "tsp", "honey", null, "1 tsp honey"),
                    new(14, 1m, "tbsp", "sesame seeds", "for garnish", "1 tbsp sesame seeds, for garnish"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Cook rice according to package directions. Keep warm."),
                    new(2, "Season ground beef with soy sauce and sesame oil. Cook in a skillet over medium-high heat until browned, about 5 minutes."),
                    new(3, "Blanch spinach in boiling water for 30 seconds, drain and squeeze dry. Season with a dash of sesame oil."),
                    new(4, "Saute carrots in a hot pan with a little oil for 2 minutes. Set aside."),
                    new(5, "Saute zucchini in the same pan for 2 minutes. Set aside."),
                    new(6, "Blanch bean sprouts for 1 minute, drain well."),
                    new(7, "Fry eggs sunny-side up in a non-stick pan."),
                    new(8, "Make the upgraded spicy sauce: mix gochujang, gochugaru, rice vinegar, and honey until smooth."),
                    new(9, "Assemble bowls: rice on the bottom, arrange vegetables and beef in sections on top. Place a fried egg in the center."),
                    new(10, "Drizzle generously with the spicy gochujang sauce and sesame seeds. Mix everything together before eating."),
                }
            };
        }

        private static SampleRecipeDefinition CreateLemonSalmon() {
            return new SampleRecipeDefinition {
                Title = "One-Pan Lemon Herb Salmon",
                Description = "Simple, elegant salmon dinner that scales beautifully. Default 4 servings — try scaling to 2 or 6 to see ingredient quantities update automatically.",
                Yield = 4,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 20,
                Showcases = "recipe-scaling",
                Tags = new[] { "Seafood", "Healthy", "Quick", "Low-Carb", "One-Pan", "Gluten-Free" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 4m, null, "salmon fillets", "6 oz each", "4 salmon fillets, 6 oz each"),
                    new(2, 2m, "tbsp", "olive oil", null, "2 tbsp olive oil"),
                    new(3, 2m, null, "lemons", null, "2 lemons"),
                    new(4, 4m, null, "cloves garlic", "minced", "4 cloves garlic, minced"),
                    new(5, 2m, "tbsp", "fresh dill", "chopped", "2 tbsp fresh dill, chopped"),
                    new(6, 1m, "tbsp", "fresh thyme leaves", null, "1 tbsp fresh thyme leaves"),
                    new(7, 1m, "lb", "asparagus", "trimmed", "1 lb asparagus, trimmed"),
                    new(8, 1m, "pint", "cherry tomatoes", null, "1 pint cherry tomatoes"),
                    new(9, null, null, "salt and pepper", "to taste", "salt and pepper, to taste"),
                    new(10, 2m, "tbsp", "butter", null, "2 tbsp butter"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Preheat oven to 400F (200C)."),
                    new(2, "Arrange asparagus and cherry tomatoes on a large sheet pan. Drizzle with 1 tbsp olive oil, season with salt and pepper."),
                    new(3, "Nestle salmon fillets among the vegetables. Drizzle with remaining olive oil."),
                    new(4, "Mix minced garlic, dill, and thyme. Spread evenly over salmon fillets."),
                    new(5, "Slice one lemon into thin rounds and place on top of salmon. Juice the second lemon over everything."),
                    new(6, "Dot butter pieces around the pan."),
                    new(7, "Bake for 18-20 minutes until salmon flakes easily and asparagus is tender-crisp."),
                    new(8, "Serve immediately with lemon wedges on the side."),
                }
            };
        }

        private static SampleRecipeDefinition CreateChickpeaStew() {
            return new SampleRecipeDefinition {
                Title = "Moroccan Chickpea Stew",
                Description = "Rich, warming North African stew loaded with spices. Tagged with 7+ tags across multiple categories, showcasing RecipeVault's powerful tag system and collection organization.",
                Yield = 6,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 35,
                Showcases = "tags-collections",
                Tags = new[] { "Vegan", "Moroccan", "Stew", "High-Fiber", "One-Pot", "Gluten-Free", "Meal-Prep" },
                Ingredients = new List<SampleIngredient> {
                    new(1, 2m, "tbsp", "olive oil", null, "2 tbsp olive oil"),
                    new(2, 1m, null, "large onion", "diced", "1 large onion, diced"),
                    new(3, 3m, null, "cloves garlic", "minced", "3 cloves garlic, minced"),
                    new(4, 1m, "tbsp", "fresh ginger", "grated", "1 tbsp fresh ginger, grated"),
                    new(5, 2m, "tsp", "cumin", null, "2 tsp cumin"),
                    new(6, 1m, "tsp", "turmeric", null, "1 tsp turmeric"),
                    new(7, 1m, "tsp", "cinnamon", null, "1 tsp cinnamon"),
                    new(8, 0.5m, "tsp", "cayenne pepper", null, "1/2 tsp cayenne pepper"),
                    new(9, 2m, null, "15 oz cans chickpeas", "drained and rinsed", "2 (15 oz) cans chickpeas, drained and rinsed"),
                    new(10, 1m, null, "28 oz can diced tomatoes", null, "1 (28 oz) can diced tomatoes"),
                    new(11, 1m, "cup", "vegetable broth", null, "1 cup vegetable broth"),
                    new(12, 2m, null, "sweet potatoes", "peeled and cubed", "2 sweet potatoes, peeled and cubed"),
                    new(13, 2m, "cups", "baby spinach", null, "2 cups baby spinach"),
                    new(14, 0.25m, "cup", "dried apricots", "chopped", "1/4 cup dried apricots, chopped"),
                    new(15, null, null, "fresh cilantro", "for garnish", "fresh cilantro, for garnish"),
                    new(16, null, null, "couscous or flatbread", "for serving", "couscous or flatbread, for serving"),
                },
                Instructions = new List<SampleInstruction> {
                    new(1, "Heat olive oil in a large Dutch oven or heavy pot over medium heat."),
                    new(2, "Saute onion for 5 minutes until translucent."),
                    new(3, "Add garlic and ginger, cook for 1 minute until fragrant."),
                    new(4, "Add cumin, turmeric, cinnamon, and cayenne. Stir for 30 seconds to bloom the spices."),
                    new(5, "Add chickpeas, diced tomatoes, vegetable broth, sweet potatoes, and dried apricots. Stir to combine."),
                    new(6, "Bring to a boil, then reduce heat and simmer for 25-30 minutes until sweet potatoes are tender."),
                    new(7, "Stir in baby spinach and cook for 2 minutes until wilted."),
                    new(8, "Season with salt and pepper to taste."),
                    new(9, "Serve over couscous or with flatbread, garnished with fresh cilantro."),
                }
            };
        }
    }

    public class SampleRecipeDefinition {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Yield { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public string Showcases { get; set; }
        public bool IsForkedFromBibimbap { get; set; }
        public string[] Tags { get; set; }
        public List<SampleIngredient> Ingredients { get; set; }
        public List<SampleInstruction> Instructions { get; set; }
    }

    public class SampleIngredient {
        public SampleIngredient(int sortOrder, decimal? quantity, string unit, string item, string preparation, string rawText) {
            SortOrder = sortOrder;
            Quantity = quantity;
            Unit = unit;
            Item = item;
            Preparation = preparation;
            RawText = rawText;
        }

        public int SortOrder { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }

    public class SampleInstruction {
        public SampleInstruction(int stepNumber, string instruction) {
            StepNumber = stepNumber;
            Instruction = instruction;
        }

        public int StepNumber { get; set; }
        public string Instruction { get; set; }
    }
}
