using System;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade.Tests.Facades {
    public class CircleMapperTests {
        private readonly CircleMapper mapper;
        private static readonly Guid OwnerSubjectId = Guid.NewGuid();
        private static readonly Guid OtherSubjectId = Guid.NewGuid();

        public CircleMapperTests() {
            var subjectMapper = new SubjectMapper();
            var tagMapper = new TagMapper(subjectMapper);
            var recipeMapper = new RecipeMapper(subjectMapper, tagMapper);
            mapper = new CircleMapper(subjectMapper, recipeMapper);
        }

        private static Circle CreateTestCircle() {
            return new Circle("Test Circle", "A test circle", OwnerSubjectId);
        }

        [Fact]
        public void MapToDto_WhenCurrentUserIsOwner_ShouldSetIsOwnerTrue() {
            var circle = CreateTestCircle();
            var dto = mapper.MapToDto(circle, OwnerSubjectId);
            dto.IsOwner.ShouldBeTrue();
            dto.Name.ShouldBe("Test Circle");
        }

        [Fact]
        public void MapToDto_WhenCurrentUserIsNotOwner_ShouldSetIsOwnerFalse() {
            var circle = CreateTestCircle();
            var dto = mapper.MapToDto(circle, OtherSubjectId);
            dto.IsOwner.ShouldBeFalse();
        }

        [Fact]
        public void MapToDto_WhenNoCurrentSubjectId_ShouldSetIsOwnerFalse() {
            var circle = CreateTestCircle();
            var dto = mapper.MapToDto(circle);
            dto.IsOwner.ShouldBeFalse();
        }

        [Fact]
        public void MapToDto_WhenNullEntity_ShouldReturnNull() {
            var dto = mapper.MapToDto(null, OwnerSubjectId);
            dto.ShouldBeNull();
        }
    }
}

