using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RecipeVault.WebApi.IntegrationTests.Helpers {
    public static class AuthenticationHelper {
        private const string TestSecret = "bUWJ9VWAx24G11fJOornAd1YeKRTokg8SV3IftX/WH4MFQlknXzaKGY7qO2fNhU4S7shnPS9TLf8t9Z/My0X/g==";
        public const string TestSubjectId = "550e8400-e29b-41d4-a716-446655440000"; // Test user GUID

        public static string GenerateTestJwt(string subject = TestSubjectId, Dictionary<string, object> claims = null) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claimsList = new List<Claim>
            {
                new Claim("sub", subject),
                new Claim("aud", "authenticated"),
                new Claim("iss", "https://umwycxfebintkenehqlj.supabase.co/auth/v1")
            };

            if (claims != null) {
                foreach (var claim in claims) {
                    claimsList.Add(new Claim(claim.Key, claim.Value?.ToString() ?? ""));
                }
            }

            var token = new JwtSecurityToken(
                issuer: "https://umwycxfebintkenehqlj.supabase.co/auth/v1",
                audience: "authenticated",
                claims: claimsList,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
