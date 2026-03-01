using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.WebApi.Filters;

namespace RecipeVault.WebApi.Tests.Filters {
    public class RequiresTierAttributeTests {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private static (ActionExecutingContext context, Mock<ISubjectPrincipal> subjectMock, Mock<IUserAccountService> serviceMock, Mock<IUnitOfWork> uowMock) CreateContext() {
            var subjectMock = new Mock<ISubjectPrincipal>();
            var serviceMock = new Mock<IUserAccountService>();
            var uowMock = new Mock<IUnitOfWork>();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(subjectMock.Object)
                .AddSingleton(serviceMock.Object)
                .AddSingleton(uowMock.Object)
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object());

            return (context, subjectMock, serviceMock, uowMock);
        }

        [Fact]
        public async Task OnActionExecutionAsync_FreeUserAccessingPremium_Returns403() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());

            var account = new UserAccount(TestSubjectId);
            serviceMock
                .Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(account);

            uowMock
                .Setup(x => x.SaveChangesAsync(default))
                .ReturnsAsync(0);

            var filter = new RequiresTierAttribute(AccountTier.Premium);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeFalse();
            context.Result.ShouldBeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
            var problem = (ProblemDetails)result.Value;
            problem.Detail.ShouldBe("This feature requires a premium account");
        }

        [Fact]
        public async Task OnActionExecutionAsync_PremiumUserAccessingPremium_Allows() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());

            var account = new UserAccount(TestSubjectId);
            account.SetTier(AccountTier.Premium);
            serviceMock
                .Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(account);

            uowMock
                .Setup(x => x.SaveChangesAsync(default))
                .ReturnsAsync(0);

            var filter = new RequiresTierAttribute(AccountTier.Premium);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        [Fact]
        public async Task OnActionExecutionAsync_BetaUserAccessingPremium_Allows() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());

            var account = new UserAccount(TestSubjectId);
            account.SetTier(AccountTier.Beta);
            serviceMock
                .Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(account);

            uowMock
                .Setup(x => x.SaveChangesAsync(default))
                .ReturnsAsync(0);

            var filter = new RequiresTierAttribute(AccountTier.Premium);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        [Fact]
        public async Task OnActionExecutionAsync_NoSubjectId_Returns401() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(string.Empty);

            var filter = new RequiresTierAttribute(AccountTier.Premium);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeFalse();
            context.Result.ShouldBeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task OnActionExecutionAsync_FreeFeature_AllowsFreeUser() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());

            var account = new UserAccount(TestSubjectId);
            serviceMock
                .Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(account);

            uowMock
                .Setup(x => x.SaveChangesAsync(default))
                .ReturnsAsync(0);

            var filter = new RequiresTierAttribute(AccountTier.Free);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        [Fact]
        public async Task OnActionExecutionAsync_AutoCreatesAccountForNewUser() {
            // Arrange
            var (context, subjectMock, serviceMock, uowMock) = CreateContext();

            subjectMock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());

            var newAccount = new UserAccount(TestSubjectId);
            serviceMock
                .Setup(x => x.GetOrCreateAccountAsync(TestSubjectId))
                .ReturnsAsync(newAccount);

            uowMock
                .Setup(x => x.SaveChangesAsync(default))
                .ReturnsAsync(1);

            var filter = new RequiresTierAttribute(AccountTier.Free);
            var nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null);
            });

            // Assert
            nextCalled.ShouldBeTrue();
            serviceMock.Verify(x => x.GetOrCreateAccountAsync(TestSubjectId), Times.Once);
            uowMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
