using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.Filters;

namespace RecipeVault.WebApi.Tests.Filters {
    public class AdminOnlyAttributeTests {
        private static readonly Guid AdminSubjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid NonAdminSubjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private static ActionExecutingContext CreateContext(string subjectId, string[] adminSubjectIds) {
            var mockSubjectPrincipal = new Mock<ISubjectPrincipal>();
            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns(subjectId);

            var configData = new Dictionary<string, string> {
                { "AdminSubjectIds:0", adminSubjectIds.Length > 0 ? adminSubjectIds[0] : "" }
            };
            for (int i = 1; i < adminSubjectIds.Length; i++) {
                configData[$"AdminSubjectIds:{i}"] = adminSubjectIds[i];
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<ISubjectPrincipal>(mockSubjectPrincipal.Object);
            services.AddSingleton<IConfiguration>(configuration);

            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()
            );

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
            );
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithAdminUser_AllowsAccess() {
            // Arrange
            var attribute = new AdminOnlyAttribute();
            var context = CreateContext(AdminSubjectId.ToString(), new[] { AdminSubjectId.ToString() });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());
            var nextCalled = false;
            ActionExecutionDelegate next = () => {
                nextCalled = true;
                return Task.FromResult(executedContext);
            };

            // Act
            await attribute.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithNonAdminUser_ReturnsForbidden() {
            // Arrange
            var attribute = new AdminOnlyAttribute();
            var context = CreateContext(NonAdminSubjectId.ToString(), new[] { AdminSubjectId.ToString() });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());
            var nextCalled = false;
            ActionExecutionDelegate next = () => {
                nextCalled = true;
                return Task.FromResult(executedContext);
            };

            // Act
            await attribute.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.ShouldBeFalse();
            context.Result.ShouldBeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithoutSubjectId_ReturnsUnauthorized() {
            // Arrange
            var attribute = new AdminOnlyAttribute();
            var context = CreateContext(null, new[] { AdminSubjectId.ToString() });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());
            var nextCalled = false;
            ActionExecutionDelegate next = () => {
                nextCalled = true;
                return Task.FromResult(executedContext);
            };

            // Act
            await attribute.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.ShouldBeFalse();
            context.Result.ShouldBeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithMultipleAdmins_AllowsAllAdmins() {
            // Arrange
            var attribute = new AdminOnlyAttribute();
            var secondAdminId = Guid.NewGuid().ToString();
            var context = CreateContext(secondAdminId, new[] { AdminSubjectId.ToString(), secondAdminId });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());
            var nextCalled = false;
            ActionExecutionDelegate next = () => {
                nextCalled = true;
                return Task.FromResult(executedContext);
            };

            // Act
            await attribute.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }
    }
}
