using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Model for reordering user's collections
    /// </summary>
    public class ReorderCollectionsModel {
        /// <summary>
        /// List of collections with their new sort orders
        /// </summary>
        [Required]
        public List<CollectionOrderModel> Collections { get; set; }
    }

    /// <summary>
    /// Model representing a collection's sort order
    /// </summary>
    public class CollectionOrderModel {
        /// <summary>
        /// Resource ID of the collection
        /// </summary>
        [Required]
        public Guid CollectionResourceId { get; set; }

        /// <summary>
        /// New sort order
        /// </summary>
        [Required]
        public int SortOrder { get; set; }
    }
}
