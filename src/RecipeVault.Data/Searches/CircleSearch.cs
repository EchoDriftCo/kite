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
                var subjectIdInt = int.Parse(SubjectId.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                entities = entities.Where(x => x.Members.Any(m => m.SubjectId == subjectIdInt));
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
