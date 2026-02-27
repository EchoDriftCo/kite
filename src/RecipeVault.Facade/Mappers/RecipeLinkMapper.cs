using System;
using System.Linq;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class RecipeLinkMapper {
        private readonly TagMapper tagMapper;

        public RecipeLinkMapper(TagMapper tagMapper) {
            this.tagMapper = tagMapper;
        }

        public RecipeLinkDto MapToDto(RecipeLink entity) {
            if (entity == null) {
                return null;
            }

            return new RecipeLinkDto {
                RecipeLinkResourceId = entity.RecipeLinkResourceId,
                ParentRecipeId = entity.ParentRecipeId,
                LinkedRecipeId = entity.LinkedRecipeId,
                IngredientIndex = entity.IngredientIndex,
                DisplayText = entity.DisplayText,
                IncludeInTotalTime = entity.IncludeInTotalTime,
                PortionUsed = entity.PortionUsed
            };
        }

        public LinkedRecipeDto MapToLinkedRecipeDto(RecipeLink entity) {
            if (entity == null || entity.LinkedRecipe == null) {
                return null;
            }

            var recipe = entity.LinkedRecipe;

            return new LinkedRecipeDto {
                RecipeLinkResourceId = entity.RecipeLinkResourceId,
                RecipeResourceId = recipe.RecipeResourceId,
                Title = recipe.Title,
                Yield = recipe.Yield,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                TotalTimeMinutes = recipe.TotalTimeMinutes,
                OriginalImageUrl = recipe.OriginalImageUrl,
                IngredientIndex = entity.IngredientIndex,
                DisplayText = entity.DisplayText,
                IncludeInTotalTime = entity.IncludeInTotalTime,
                PortionUsed = entity.PortionUsed,
                Ingredients = recipe.Ingredients?.Select(i => new RecipeIngredientDto {
                    RecipeIngredientId = i.RecipeIngredientId,
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = recipe.Instructions?.Select(i => new RecipeInstructionDto {
                    RecipeInstructionId = i.RecipeInstructionId,
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList(),
                Tags = recipe.RecipeTags?
                    .Where(rt => !rt.IsOverridden)
                    .Select(rt => tagMapper.MapToRecipeTagDto(rt))
                    .ToList()
            };
        }

        public UsedInRecipeDto MapToUsedInRecipeDto(RecipeLink entity) {
            if (entity == null || entity.ParentRecipe == null) {
                return null;
            }

            var recipe = entity.ParentRecipe;

            return new UsedInRecipeDto {
                RecipeLinkResourceId = entity.RecipeLinkResourceId,
                RecipeResourceId = recipe.RecipeResourceId,
                Title = recipe.Title,
                OriginalImageUrl = recipe.OriginalImageUrl,
                OwnerName = recipe.CreatedSubject != null
                    ? $"{recipe.CreatedSubject.GivenName} {recipe.CreatedSubject.FamilyName}".Trim()
                    : "Unknown"
            };
        }
    }
}
