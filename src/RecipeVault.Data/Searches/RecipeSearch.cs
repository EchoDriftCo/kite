using System;
using System.Collections.Generic;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
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
                entities = entities.Where(x => x.Title.Contains(Title));
            }

            if (TagResourceIds != null && TagResourceIds.Count > 0) {
                entities = entities.Where(x => x.RecipeTags.Any(rt =>
                    !rt.IsOverridden && TagResourceIds.Contains(rt.Tag.TagResourceId)));
            }

            if (TagCategory.HasValue) {
                entities = entities.Where(x => x.RecipeTags.Any(rt =>
                    !rt.IsOverridden && (int)rt.Tag.Category == TagCategory.Value));
            }

            return entities;
        }
    }
}
