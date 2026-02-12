using System;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class TagSearch : Search, ITagSearch {
        public string Name { get; set; }
        public int? Category { get; set; }
        public bool? IsGlobal { get; set; }
        public Guid? CreatedSubjectId { get; set; }

        public IQueryable<Tag> Build(IQueryable<Tag> entities) {
            // When user is specified: show global tags + user's own tags
            if (CreatedSubjectId.HasValue) {
                entities = entities.Where(x => x.IsGlobal || x.CreatedSubject.SubjectId == CreatedSubjectId);
            }

            if (!string.IsNullOrEmpty(Name)) {
                entities = entities.Where(x => x.Name.Contains(Name));
            }

            if (Category.HasValue) {
                entities = entities.Where(x => (int)x.Category == Category.Value);
            }

            if (IsGlobal.HasValue) {
                entities = entities.Where(x => x.IsGlobal == IsGlobal.Value);
            }

            return entities;
        }
    }
}
