using System;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class CircleSearch : Search, ICircleSearch {
        public Guid? SubjectId { get; set; }  // Find circles where user is a member
        public bool? OwnedOnly { get; set; }  // Only circles owned by user

        public IQueryable<Circle> Build(IQueryable<Circle> entities) {
            if (SubjectId.HasValue) {
                // CircleMember.SubjectId is now Guid, matching Subject.SubjectId
                entities = entities.Where(x => x.Members.Any(m => m.SubjectId == SubjectId.Value));
            }

            if (OwnedOnly.HasValue && OwnedOnly.Value && SubjectId.HasValue) {
                entities = entities.Where(x => x.CreatedSubject.SubjectId == SubjectId.Value);
            }

            return entities;
        }
    }

    public interface ICircleSearch {
        IQueryable<Circle> Build(IQueryable<Circle> entities);
    }
}
