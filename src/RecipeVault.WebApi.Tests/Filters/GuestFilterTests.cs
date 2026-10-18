using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.Authentication;
using RecipeVault.WebApi.Filters;

namespace RecipeVault.WebApi.Tests.Filters {
    public class GuestFilterTests {
        private static ActionExecutingContext CreateContext(bool isGuest, IMemoryCache cache = null, string clientIp = "203.0.113.5", Dictionary<string, string> config = null) {
            var services = new ServiceCollection();
            services.AddSingleton(cache ?? new MemoryCache(new MemoryCacheOptions()));
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(config ?? new Dictionary<string, string>())
                .Build());

            var claims = new List<Claim> { new Claim("sub", "550e8400-e29b-41d4-a716-446655440000") };
            if (isGuest) {
                claims.Add(new Claim(GuestAccess.IsAnonymousClaim, "true"));
            }

            var httpContext = new DefaultHttpContext {
                RequestServices = services.BuildServiceProvider(),
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            };
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(clientIp);

            return new ActionExecutingContext(
                new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
            );
        }

        private static async Task<bool> RunAsync(IAsyncActionFilter filter, ActionExecutingContext context) {
            var nextCalled = false;
            await filter.OnActionExecutionAsync(context, () => {
                nextCalled = true;
                return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()));
            });
            return nextCalled;
        }

        [Fact]
        public void IsGuest_WithoutAnonymousClaim_ReturnsFalse() {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(GuestAccess.IsAnonymousClaim, "false") }, "Test"));
            user.IsGuest().ShouldBeFalse();
        }

        [Fact]
        public async Task RegisteredUserOnly_WithRegisteredUser_AllowsAccess() {
            var context = CreateContext(isGuest: false);

            var nextCalled = await RunAsync(new RegisteredUserOnlyAttribute(), context);

            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        [Fact]
        public async Task RegisteredUserOnly_WithGuest_ReturnsRegistrationRequired() {
            var context = CreateContext(isGuest: true);

            var nextCalled = await RunAsync(new RegisteredUserOnlyAttribute(), context);

            nextCalled.ShouldBeFalse();
            var result = context.Result.ShouldBeOfType<ObjectResult>();
            result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
            result.Value.ShouldBeOfType<ProblemDetails>().Type.ShouldBe(GuestAccess.RegistrationRequiredType);
        }

        [Fact]
        public async Task GuestQuota_WithRegisteredUser_IsNotLimited() {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var filter = new GuestQuotaAttribute(GuestQuotaAttribute.Ai, 1);

            for (var i = 0; i < 3; i++) {
                (await RunAsync(filter, CreateContext(isGuest: false, cache))).ShouldBeTrue();
            }
        }

        [Fact]
        public async Task GuestQuota_WithGuestOverLimit_Returns429() {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var filter = new GuestQuotaAttribute(GuestQuotaAttribute.Ai, 2);

            (await RunAsync(filter, CreateContext(isGuest: true, cache))).ShouldBeTrue();
            (await RunAsync(filter, CreateContext(isGuest: true, cache))).ShouldBeTrue();

            var context = CreateContext(isGuest: true, cache);
            (await RunAsync(filter, context)).ShouldBeFalse();
            var result = context.Result.ShouldBeOfType<ObjectResult>();
            result.StatusCode.ShouldBe(StatusCodes.Status429TooManyRequests);
            result.Value.ShouldBeOfType<ProblemDetails>().Type.ShouldBe(GuestAccess.GuestQuotaExceededType);
        }

        [Fact]
        public async Task GuestQuota_WithConcurrentGuestRequests_AllowsExactlyTheLimit() {
            var cache = new MemoryCache(new MemoryCacheOptions());
            const int limit = 10;

            // Separate attribute instances share the quota through the cache key, like different actions in one bucket
            var results = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Task.Run(() =>
                RunAsync(new GuestQuotaAttribute(GuestQuotaAttribute.Ai, limit), CreateContext(isGuest: true, cache)))));

            results.Count(allowed => allowed).ShouldBe(limit);
        }

        [Fact]
        public async Task GuestQuota_IsTrackedPerIpAndBucket() {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var aiFilter = new GuestQuotaAttribute(GuestQuotaAttribute.Ai, 1);
            var saveFilter = new GuestQuotaAttribute(GuestQuotaAttribute.Save, 1);

            (await RunAsync(aiFilter, CreateContext(isGuest: true, cache, "203.0.113.5"))).ShouldBeTrue();
            (await RunAsync(aiFilter, CreateContext(isGuest: true, cache, "203.0.113.5"))).ShouldBeFalse();

            (await RunAsync(aiFilter, CreateContext(isGuest: true, cache, "198.51.100.7"))).ShouldBeTrue();
            (await RunAsync(saveFilter, CreateContext(isGuest: true, cache, "203.0.113.5"))).ShouldBeTrue();
        }

        [Fact]
        public async Task GuestQuota_UsesConfiguredLimit() {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = new Dictionary<string, string> { { "GuestQuota:Ai", "1" } };
            var filter = new GuestQuotaAttribute(GuestQuotaAttribute.Ai, 100);

            (await RunAsync(filter, CreateContext(isGuest: true, cache, config: config))).ShouldBeTrue();
            (await RunAsync(filter, CreateContext(isGuest: true, cache, config: config))).ShouldBeFalse();
        }

        [Fact]
        public void GetClientIp_PrefersFlyClientIpHeader() {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            httpContext.Request.Headers["Fly-Client-IP"] = "203.0.113.9";

            GuestAccess.GetClientIp(httpContext).ShouldBe("203.0.113.9");
        }
    }
}
