using System;
using Cortside.AspNetCore.Auditable;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Security;

namespace RecipeVault.WebApi.Services {
    /// <summary>
    /// Custom SubjectFactory that maps Supabase JWT claims to Subject entity,
    /// including email as UserPrincipalName.
    /// </summary>
    public class SupabaseSubjectFactory : ISubjectFactory<Subject> {
        public Subject CreateSubject(ISubjectPrincipal subjectPrincipal) {
            // Never fall back to Guid.Empty: that is the shared "service" subject, and
            // writes attributed to it can't be told apart. Bearer tokens with a bad sub are
            // already rejected in OnTokenValidated; this is the backstop.
            if (!Guid.TryParse(subjectPrincipal.SubjectId, out var subjectId)) {
                throw new InvalidOperationException($"Subject id '{subjectPrincipal.SubjectId}' is not a GUID.");
            }

            // Create Subject with all properties including UserPrincipalName
            // Subject constructor: (Guid subjectId, string name, string givenName, string familyName, string userPrincipalName)
            return new Subject(
                subjectId,
                subjectPrincipal.Name,
                subjectPrincipal.GivenName,
                subjectPrincipal.FamilyName,
                subjectPrincipal.UserPrincipalName  // This maps email from JWT
            );
        }
    }
}
