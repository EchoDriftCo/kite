using System;
using System.Collections.Generic;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public interface IRecipeSearch : ISearch, ISearchBuilder<Recipe> {
        Guid? RecipeResourceId { get; set; }
        string Title { get; set; }
        Guid? CreatedSubjectId { get; set; }
        bool? IsPublic { get; set; }
        bool IncludePublic { get; set; }
        List<Guid> TagResourceIds { get; set; }
        int? TagCategory { get; set; }
        bool? IsFavorite { get; set; }
        bool? HasRequiredEquipment { get; set; }
        int? MinRating { get; set; }
    }
}
