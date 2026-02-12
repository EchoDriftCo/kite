using System;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public interface ITagSearch : ISearch, ISearchBuilder<Tag> {
        string Name { get; set; }
        int? Category { get; set; }
        bool? IsGlobal { get; set; }
        Guid? CreatedSubjectId { get; set; }
    }
}
