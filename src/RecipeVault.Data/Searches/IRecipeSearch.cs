using System;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public interface IRecipeSearch : ISearch, ISearchBuilder<Recipe> {
        Guid? RecipeResourceId { get; set; }
        string Title { get; set; }
    }
}
