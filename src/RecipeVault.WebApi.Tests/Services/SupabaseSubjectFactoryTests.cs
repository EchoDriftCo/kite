using System;
using System.Collections.Generic;
using System.Security.Claims;
using Cortside.Common.Security;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.Services;

namespace RecipeVault.WebApi.Tests.Services {
    public class SupabaseSubjectFactoryTests {
        private static SubjectPrincipal Principal(string sub) =>
            new SubjectPrincipal(new List<Claim> { new Claim("sub", sub), new Claim("upn", "cook@example.com") });

        [Fact]
        public void CreateSubject_UsesTheSubClaim() {
            var id = Guid.NewGuid();
            var subject = new SupabaseSubjectFactory().CreateSubject(Principal(id.ToString()));
            subject.SubjectId.ShouldBe(id);
            subject.UserPrincipalName.ShouldBe("cook@example.com");
        }

        [Fact]
        public void CreateSubject_NonGuidSub_Throws_InsteadOfFallingBackToGuidEmpty() {
            Should.Throw<InvalidOperationException>(() => new SupabaseSubjectFactory().CreateSubject(Principal("not-a-guid")));
        }
    }
}
