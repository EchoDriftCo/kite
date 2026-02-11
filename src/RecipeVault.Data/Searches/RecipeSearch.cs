using System;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class RecipeSearch : Search, IRecipeSearch {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public Guid? CreatedSubjectId { get; set; }

        public IQueryable<Recipe> Build(IQueryable<Recipe> entities) {
            if (CreatedSubjectId.HasValue) {
                entities = entities.Where(x => x.CreatedSubject.SubjectId == CreatedSubjectId);
            }

            if (RecipeResourceId.HasValue) {
                entities = entities.Where(x => x.RecipeResourceId == RecipeResourceId);
            }

            if (!string.IsNullOrEmpty(Title)) {
                entities = entities.Where(x => x.Title.Contains(Title));
            }

            return entities;
        }
    }
}
