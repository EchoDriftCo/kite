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
            // Parse SubjectId from the "sub" claim - default to empty if parse fails
            _ = Guid.TryParse(subjectPrincipal.SubjectId, out var subjectId);

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
