#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class DiscoverSearchModel : SearchModel {
        public string Title { get; set; }

        /// <summary>popular, newest, rating</summary>
        public string SortBy { get; set; }

        public List<Guid> TagResourceIds { get; set; }
        public int? MinRating { get; set; }
    }
}
