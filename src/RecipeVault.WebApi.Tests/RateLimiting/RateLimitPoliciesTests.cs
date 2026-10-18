using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.Authentication;
using RecipeVault.WebApi.RateLimiting;

namespace RecipeVault.WebApi.Tests.RateLimiting {
    public class RateLimitPoliciesTests {
        private const string UserId = "550e8400-e29b-41d4-a716-446655440000";
        private const string ClientIp = "203.0.113.5";

        private static DefaultHttpContext CreateContext(bool authenticated, bool guest = false) {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(ClientIp);
            if (authenticated) {
                var claims = new List<Claim> { new Claim("sub", UserId) };
                if (guest) {
                    claims.Add(new Claim(GuestAccess.IsAnonymousClaim, "true"));
                }
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            }
            return context;
        }

        [Fact]
        public void PartitionKey_RegisteredUser_IsPerUser() {
            RateLimitPolicies.PartitionKey(CreateContext(authenticated: true)).ShouldBe("user:" + UserId);
        }

        [Fact]
        public void PartitionKey_Guest_IsPerIp_BecauseGuestsCanMintNewIds() {
            RateLimitPolicies.PartitionKey(CreateContext(authenticated: true, guest: true)).ShouldBe("ip:" + ClientIp);
        }

        [Fact]
        public void PartitionKey_Anonymous_IsPerIp() {
            RateLimitPolicies.PartitionKey(CreateContext(authenticated: false)).ShouldBe("ip:" + ClientIp);
        }

        [Fact]
        public void PartitionKey_PrefersFlyClientIp() {
            var context = CreateContext(authenticated: false);
            context.Request.Headers["Fly-Client-IP"] = "198.51.100.7";
            RateLimitPolicies.PartitionKey(context).ShouldBe("ip:198.51.100.7");
        }
    }
}
