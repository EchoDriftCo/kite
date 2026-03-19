#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class RecipeModelMapper {
        private readonly SubjectModelMapper subjectModelMapper;

        public RecipeModelMapper(SubjectModelMapper subjectModelMapper) {
            this.subjectModelMapper = subjectModelMapper;
        }

        public RecipeModel Map(RecipeDto dto) {
            if (dto == null) {
                return null;
            }

            return new RecipeModel {
                RecipeResourceId = dto.RecipeResourceId,
                Title = dto.Title,
                Description = dto.Description,
                Yield = dto.Yield,
                PrepTimeMinutes = dto.PrepTimeMinutes,
                CookTimeMinutes = dto.CookTimeMinutes,
                TotalTimeMinutes = dto.TotalTimeMinutes,
                Source = dto.Source,
                OriginalImageUrl = dto.OriginalImageUrl,
                SourceImageUrl = dto.SourceImageUrl,
                IsPublic = dto.IsPublic,
                Rating = dto.Rating,
                IsFavorite = dto.IsFavorite,
                IsOwner = dto.IsOwner,
                ShareToken = dto.ShareToken,
                Ingredients = dto.Ingredients?.Select(i => new RecipeIngredientModel {
                    RecipeIngredientId = i.RecipeIngredientId,
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = dto.Instructions?.Select(i => new RecipeInstructionModel {
                    RecipeInstructionId = i.RecipeInstructionId,
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList(),
                Tags = dto.Tags?.Select(t => new RecipeTagModel {
                    TagResourceId = t.TagResourceId,
                    GlobalName = t.GlobalName,
                    DisplayName = t.DisplayName,
                    Category = t.Category,
                    CategoryName = t.CategoryName,
                    SourceType = t.SourceType,
                    SourceTypeName = t.SourceTypeName,
                    IsAiAssigned = t.IsAiAssigned,
                    Confidence = t.Confidence,
                    IsOverridden = t.IsOverridden,
                    Detail = t.Detail,
                    NormalizedEntityId = t.NormalizedEntityId,
                    NormalizedEntityType = t.NormalizedEntityType
                }).ToList(),
                ForkedFrom = dto.ForkedFrom != null ? new ForkedFromModel {
                    RecipeResourceId = dto.ForkedFrom.RecipeResourceId,
                    Title = dto.ForkedFrom.Title,
                    OwnerName = dto.ForkedFrom.OwnerName,
                    IsAvailable = dto.ForkedFrom.IsAvailable
                } : null,
                ForkCount = dto.ForkCount,
                CreatedDate = dto.CreatedDate,
                CreatedSubject = subjectModelMapper.Map(dto.CreatedSubject),
                LastModifiedDate = dto.LastModifiedDate,
                LastModifiedSubject = subjectModelMapper.Map(dto.LastModifiedSubject)
            };
        }

        public RecipeSearchDto MapToDto(RecipeSearchModel model) {
            if (model == null) {
                return null;
            }

            return new RecipeSearchDto {
                RecipeResourceId = model.RecipeResourceId,
                Title = model.Title,
                IsPublic = model.IsPublic,
                IncludePublic = model.IncludePublic,
                TagResourceIds = model.TagResourceIds,
                TagCategory = model.TagCategory,
                IsFavorite = model.IsFavorite,
                HasRequiredEquipment = model.HasRequiredEquipment,
                MinRating = model.MinRating,
                CollectionResourceId = model.CollectionResourceId,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Sort = model.Sort
            };
        }

        public RecipeSearchDto MapToDto(DiscoverSearchModel model) {
            if (model == null) {
                return null;
            }

            var (sortBy, sortDirection) = model.SortBy switch {
                "popular" => ("ForkCount", "desc"),
                "rating" => ("Rating", "desc"),
                _ => ("CreatedDate", "desc")  // "newest" is the default
            };

            return new RecipeSearchDto {
                Title = model.Title,
                TagResourceIds = model.TagResourceIds,
                MinRating = model.MinRating,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }

        public UpdateRecipeDto MapToDto(UpdateRecipeModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateRecipeDto {
                Title = model.Title,
                Description = model.Description,
                Yield = model.Yield,
                PrepTimeMinutes = model.PrepTimeMinutes,
                CookTimeMinutes = model.CookTimeMinutes,
                Source = model.Source,
                OriginalImageUrl = model.OriginalImageUrl,
                SourceImageUrl = model.SourceImageUrl,
                Ingredients = model.Ingredients?.Select(i => new UpdateRecipeIngredientDto {
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = model.Instructions?.Select(i => new UpdateRecipeInstructionDto {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList(),
                IsPublic = model.IsPublic
            };
        }

        public ParseRecipeRequestDto MapToDto(ParseRecipeRequestModel model) {
            if (model == null) {
                return null;
            }

            return new ParseRecipeRequestDto {
                Image = model.Image,
                MimeType = model.MimeType,
                Url = model.Url,
                Html = model.Html
            };
        }

        public ParseRecipeResponseModel Map(ParseRecipeResponseDto dto) {
            if (dto == null) {
                return null;
            }

            return new ParseRecipeResponseModel {
                Confidence = dto.Confidence,
                Recipe = dto.Parsed == null ? null : new ParsedRecipeModel {
                    Title = dto.Parsed.Title,
                    Yield = dto.Parsed.Yield,
                    PrepTimeMinutes = dto.Parsed.PrepTimeMinutes,
                    CookTimeMinutes = dto.Parsed.CookTimeMinutes,
                    Ingredients = dto.Parsed.Ingredients?.Select(i => new ParsedIngredientModel {
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Item = i.Item,
                        Preparation = i.Preparation,
                        RawText = i.RawText
                    }).ToList(),
                    Instructions = dto.Parsed.Instructions?.Select(i => new ParsedInstructionModel {
                        StepNumber = i.StepNumber,
                        Instruction = i.Instruction,
                        RawText = i.RawText
                    }).ToList(),
                    ImageUrl = dto.Parsed.ImageUrl
                },
                Warnings = dto.Warnings
            };
        }

        public SubstitutionRequestDto MapToDto(SubstitutionRequestModel model) {
            if (model == null) {
                return null;
            }

            return new SubstitutionRequestDto {
                IngredientIndices = model.IngredientIndices,
                DietaryConstraints = model.DietaryConstraints
            };
        }

        public SubstitutionResponseModel Map(SubstitutionResponseDto dto) {
            if (dto == null) {
                return null;
            }

            return new SubstitutionResponseModel {
                Analysis = dto.Analysis,
                Cached = dto.Cached,
                Substitutions = dto.Substitutions?.Select(s => new IngredientSubstitutionModel {
                    OriginalIndex = s.OriginalIndex,
                    OriginalText = s.OriginalText,
                    Reason = s.Reason,
                    Options = s.Options?.Select(o => new SubstitutionOptionModel {
                        Name = o.Name,
                        Notes = o.Notes,
                        TechniqueAdjustments = o.TechniqueAdjustments,
                        Ingredients = o.Ingredients?.Select(i => new SubstitutionIngredientModel {
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Item = i.Item
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public ApplySubstitutionsDto MapToDto(ApplySubstitutionsModel model) {
            if (model == null) {
                return null;
            }

            return new ApplySubstitutionsDto {
                ForkTitle = model.ForkTitle,
                Selections = model.Selections?.Select(s => new SubstitutionSelectionDto {
                    IngredientIndex = s.IngredientIndex,
                    OptionIndex = s.OptionIndex,
                    SelectedOption = s.SelectedOption == null ? null : new SubstitutionOptionDto {
                        Name = s.SelectedOption.Name,
                        Notes = s.SelectedOption.Notes,
                        TechniqueAdjustments = s.SelectedOption.TechniqueAdjustments,
                        Ingredients = s.SelectedOption.Ingredients?.Select(i => new SubstitutionIngredientDto {
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Item = i.Item
                        }).ToList()
                    }
                }).ToList()
            };
        }

        public CookingDataModel Map(CookingDataDto dto) {
            if (dto == null) {
                return null;
            }

            return new CookingDataModel {
                RecipeResourceId = dto.RecipeResourceId,
                Steps = dto.Steps?.Select(s => new CookingStepModel {
                    StepNumber = s.StepNumber,
                    Instruction = s.Instruction,
                    TimerIndexes = s.TimerIndexes
                }).ToList(),
                Timers = dto.Timers?.Select(t => new TimerModel {
                    Index = t.Index,
                    Label = t.Label,
                    Seconds = t.Seconds,
                    StepNumber = t.StepNumber
                }).ToList(),
                Temperatures = dto.Temperatures?.Select(t => new TemperatureModel {
                    StepNumber = t.StepNumber,
                    Value = t.Value,
                    Unit = t.Unit,
                    Context = t.Context
                }).ToList()
            };
        }
    }
}
