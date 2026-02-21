#pragma warning disable CS1591 // Missing XML comments

using System;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Responses {
    public class TagModel : AuditableEntityModel {
        public int TagId { get; set; }
        public Guid TagResourceId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public bool IsGlobal { get; set; }
        public int? SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public bool IsSystemTag { get; set; }
    }
}
