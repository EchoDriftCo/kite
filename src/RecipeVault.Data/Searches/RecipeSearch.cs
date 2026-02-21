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
                
                // Include matching on tag names and aliases
                // For owner searching own recipes: match on Tag.Name OR their UserTagAlias.Alias
                // For public search: match on Tag.Name OR (Alias where ShowAliasPublicly=true) OR NormalizedEntityId
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
                                // Owner's own aliases
                                (x.CreatedSubject.SubjectId == SearchingUserId && 
                                 rt.Tag.UserTagAliases.Any(uta => 
                                    uta.UserId == SearchingUserId && 
                                    EF.Functions.ILike(uta.Alias, pattern))) ||
                                // Public aliases or normalized entities
                                (x.IsPublic && rt.Tag.UserTagAliases.Any(uta =>
                                    (uta.ShowAliasPublicly && EF.Functions.ILike(uta.Alias, pattern)) ||
                                    (uta.NormalizedEntityId != null && EF.Functions.ILike(uta.NormalizedEntityId, pattern))))
                            )));
                } else {
                    // No user context, search public recipes only by tag names and public aliases
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
                                rt.Tag.UserTagAliases.Any(uta =>
                                    (uta.ShowAliasPublicly && EF.Functions.ILike(uta.Alias, pattern)) ||
                                    (uta.NormalizedEntityId != null && EF.Functions.ILike(uta.NormalizedEntityId, pattern)))
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

            return entities;
        }
    }
}
