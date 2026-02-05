using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class UnitModel {
        public Guid UnitResourceId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string PluralName { get; set; }
        public string Type { get; set; }
        public decimal? MetricEquivalentMl { get; set; }
        public decimal? MetricEquivalentG { get; set; }
        public List<UnitAliasModel> Aliases { get; set; }
    }

    public class UnitAliasModel {
        public string Alias { get; set; }
    }
}
