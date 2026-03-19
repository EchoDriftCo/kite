using System;
using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class MealPlanSearch : Search, IMealPlanSearch {
        public Guid? CreatedSubjectId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }

        public IQueryable<MealPlan> Build(IQueryable<MealPlan> entities) {
            if (CreatedSubjectId.HasValue) {
                entities = entities.Where(x => x.CreatedSubject.SubjectId == CreatedSubjectId);
            }

            if (StartDateFrom.HasValue) {
                entities = entities.Where(x => x.StartDate >= StartDateFrom.Value);
            }

            if (StartDateTo.HasValue) {
                entities = entities.Where(x => x.StartDate <= StartDateTo.Value);
            }

            return entities;
        }
    }
}
