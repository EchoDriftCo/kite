using System.Collections.Generic;
using System.Linq;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class UnitModelMapper {
        public UnitModel Map(UnitDto dto) {
            if (dto == null) {
                return null;
            }

            return new UnitModel {
                UnitResourceId = dto.UnitResourceId,
                Name = dto.Name,
                Abbreviation = dto.Abbreviation,
                PluralName = dto.PluralName,
                Type = dto.Type,
                MetricEquivalentMl = dto.MetricEquivalentMl,
                MetricEquivalentG = dto.MetricEquivalentG,
                Aliases = dto.Aliases?.Select(a => new UnitAliasModel {
                    Alias = a.Alias
                }).ToList()
            };
        }

        public List<UnitModel> Map(IReadOnlyList<UnitDto> dtos) {
            return dtos?.Select(Map).ToList();
        }

        public UnitMatchModel Map(UnitMatchResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new UnitMatchModel {
                IsMatch = dto.IsMatch,
                Unit = dto.Unit != null ? Map(dto.Unit) : null,
                Confidence = dto.Confidence,
                OriginalInput = dto.OriginalInput
            };
        }
    }
}
