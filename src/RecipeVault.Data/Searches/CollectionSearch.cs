using System;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class CollectionSearch : Search, ICollectionSearch {
        public Guid? SubjectId { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsFeatured { get; set; }
        public string SearchTerm { get; set; }

        public IQueryable<Collection> Build(IQueryable<Collection> entities) {
            if (SubjectId.HasValue) {
                entities = entities.Where(x => x.SubjectId == SubjectId.Value);
            }

            if (IsPublic.HasValue) {
                entities = entities.Where(x => x.IsPublic == IsPublic.Value);
            }

            if (IsFeatured.HasValue) {
                entities = entities.Where(x => x.IsFeatured == IsFeatured.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                entities = entities.Where(x => x.Name.Contains(SearchTerm) || x.Description.Contains(SearchTerm));
            }

            entities = entities.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);

            return entities;
        }
    }

    public interface ICollectionSearch {
        IQueryable<Collection> Build(IQueryable<Collection> entities);
    }
}
