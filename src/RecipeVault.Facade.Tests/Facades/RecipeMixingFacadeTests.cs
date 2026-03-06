using System;
using System.Threading;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.DomainService;
using RecipeVault.DomainService.Models;
using RecipeVault.Dto.Input;
using RecipeVault.Facade.Mappers;
using RecipeVault.TestUtilities.Builders;
using Shouldly;
using Xunit;

namespace RecipeVault.Facade.Tests.Facades {
    public class RecipeMixingFacadeTests {
        private static RecipeMixingFacade CreateFacade(
            Mock<IUnitOfWork> uow,
            Mock<IRecipeService> recipeService,
            Mock<IRecipeMixingService> mixingService,
            Mock<ISubjectPrincipal> subjectPrincipal,
            Mock<ILogger<RecipeMixingFacade>> logger = null) {
            logger ??= new Mock<ILogger<RecipeMixingFacade>>();
            return new RecipeMixingFacade(
                logger.Object,
                uow.Object,
                recipeService.Object,
                mixingService.Object,
                new RecipeMapper(new SubjectMapper(), new TagMapper(new SubjectMapper())),
                subjectPrincipal.Object);
        }

        [Fact]
        public async Task MixRecipesAsync_WithSameRecipeIds_ThrowsArgumentException() {
            var recipeId = Guid.NewGuid();
            var request = new MixRecipesRequestDto {
                RecipeAId = recipeId,
                RecipeBId = recipeId,
                Mode = "guided",
                Intent = "keep the sauce"
            };

            var facade = CreateFacade(
                new Mock<IUnitOfWork>(),
                new Mock<IRecipeService>(MockBehavior.Strict),
                new Mock<IRecipeMixingService>(MockBehavior.Strict),
                new Mock<ISubjectPrincipal>());

            var ex = await Should.ThrowAsync<ArgumentException>(() => facade.MixRecipesAsync(request));
            ex.Message.ShouldContain("Cannot mix a recipe with itself");
        }

        [Fact]
        public async Task RefineMixedRecipeAsync_PreservesParentRecipeResourceIds() {
            var parentA = Guid.NewGuid();
            var parentB = Guid.NewGuid();

            var request = new RefineMixRequestDto {
                RefinementNotes = "Make it spicier",
                Preview = new Dto.Output.MixedRecipePreviewDto {
                    Title = "Original Mix",
                    Yield = 4,
                    RecipeAResourceId = parentA,
                    RecipeBResourceId = parentB
                }
            };

            var refined = new MixedRecipePreview {
                Title = "Refined Mix",
                Yield = 4,
                MixNotes = "updated"
            };

            var mixingService = new Mock<IRecipeMixingService>();
            mixingService
                .Setup(x => x.RefineMixedRecipeAsync(It.IsAny<MixedRecipePreview>(), request.RefinementNotes, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refined);

            var subjectPrincipal = new Mock<ISubjectPrincipal>();
            subjectPrincipal.Setup(x => x.SubjectId).Returns(Guid.NewGuid().ToString());

            var facade = CreateFacade(
                new Mock<IUnitOfWork>(),
                new Mock<IRecipeService>(),
                mixingService,
                subjectPrincipal);

            var result = await facade.RefineMixedRecipeAsync(request);

            result.ShouldNotBeNull();
            result.Title.ShouldBe("Refined Mix");
            result.RecipeAResourceId.ShouldBe(parentA);
            result.RecipeBResourceId.ShouldBe(parentB);
        }
    }
}
