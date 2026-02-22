using System;
using System.Collections.Generic;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class RecipeSearch : Search, IRecipeSearch {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public Guid? CreatedSubjectId { get; set; }
        public bool? IsPublic { get; set; }
        public bool IncludePublic { get; set; }
        public List<Guid> TagResourceIds { get; set; }
        public int? TagCategory { get; set; }
        public bool? IsFavorite { get; set; }
        public int? MinRating { get; set; }
        public Guid? SearchingUserId { get; set; }  // Used to match user's own aliases
        public int? ForkedFromRecipeId { get; set; }

        public IQueryable<Recipe> Build(IQueryable<Recipe> entities) {
            if (IncludePublic && CreatedSubjectId.HasValue) {
                entities = entities.Where(x => x.CreatedSubject.SubjectId == CreatedSubjectId || x.IsPublic);
            } else if (CreatedSubjectId.HasValue) {
                entities = entities.Where(x => x.CreatedSubject.SubjectId == CreatedSubjectId);
            }

            if (IsPublic.HasValue) {
                entities = entities.Where(x => x.IsPublic == IsPublic.Value);
            }

            if (RecipeResourceId.HasValue) {
                entities = entities.Where(x => x.RecipeResourceId == RecipeResourceId);
            }

            if (!string.IsNullOrEmpty(Title)) {
                var pattern = $"%{Title}%";
                
                // Include matching on tag names, RecipeTag.Detail, and RecipeTag.NormalizedEntityId
                // For owner searching own recipes: match on Tag.Name OR RecipeTag.Detail (on own recipes)
                // For public search: match on Tag.Name OR RecipeTag.Detail OR RecipeTag.NormalizedEntityId
                if (SearchingUserId.HasValue) {
                    // Owner searching their own recipes or public recipes
                    entities = entities.Where(x =>
                        EF.Functions.ILike(x.Title, pattern) ||
                        (x.Description != null && EF.Functions.ILike(x.Description, pattern)) ||
                        (x.Source != null && EF.Functions.ILike(x.Source, pattern)) ||
                        x.Ingredients.Any(i =>
                            (i.Item != null && EF.Functions.ILike(i.Item, pattern)) ||
                            (i.RawText != null && EF.Functions.ILike(i.RawText, pattern))) ||
                        x.RecipeTags.Any(rt => 
                            !rt.IsOverridden && (
                                EF.Functions.ILike(rt.Tag.Name, pattern) ||
                                // Owner's own recipe details
                                (x.CreatedSubject.SubjectId == SearchingUserId && 
                                 rt.Detail != null && 
                                 EF.Functions.ILike(rt.Detail, pattern)) ||
                                // Public recipe details or normalized entities
                                (x.IsPublic && (
                                    (rt.Detail != null && EF.Functions.ILike(rt.Detail, pattern)) ||
                                    (rt.NormalizedEntityId != null && EF.Functions.ILike(rt.NormalizedEntityId, pattern))))
                            )));
                } else {
                    // No user context, search public recipes only by tag names, details, and normalized entities
                    entities = entities.Where(x =>
                        EF.Functions.ILike(x.Title, pattern) ||
                        (x.Description != null && EF.Functions.ILike(x.Description, pattern)) ||
                        (x.Source != null && EF.Functions.ILike(x.Source, pattern)) ||
                        x.Ingredients.Any(i =>
                            (i.Item != null && EF.Functions.ILike(i.Item, pattern)) ||
                            (i.RawText != null && EF.Functions.ILike(i.RawText, pattern))) ||
                        x.RecipeTags.Any(rt => 
                            !rt.IsOverridden && (
                                EF.Functions.ILike(rt.Tag.Name, pattern) ||
                                (rt.Detail != null && EF.Functions.ILike(rt.Detail, pattern)) ||
                                (rt.NormalizedEntityId != null && EF.Functions.ILike(rt.NormalizedEntityId, pattern))
                            )));
                }
            }

            if (TagResourceIds != null && TagResourceIds.Count > 0) {
                entities = entities.Where(x => x.RecipeTags.Any(rt =>
                    !rt.IsOverridden && TagResourceIds.Contains(rt.Tag.TagResourceId)));
            }

            if (TagCategory.HasValue) {
                entities = entities.Where(x => x.RecipeTags.Any(rt =>
                    !rt.IsOverridden && (int)rt.Tag.Category == TagCategory.Value));
            }

            if (IsFavorite.HasValue) {
                entities = entities.Where(x => x.IsFavorite == IsFavorite.Value);
            }

            if (MinRating.HasValue) {
                entities = entities.Where(x => x.Rating.HasValue && x.Rating.Value >= MinRating.Value);
            }

            if (ForkedFromRecipeId.HasValue) {
                entities = entities.Where(x => x.ForkedFromRecipeId == ForkedFromRecipeId.Value);
            }

            return entities;
        }
    }
}
