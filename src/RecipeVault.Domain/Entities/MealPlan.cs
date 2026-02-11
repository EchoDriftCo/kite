using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(MealPlanResourceId), IsUnique = true)]
    [Table("MealPlan")]
    public class MealPlan : AuditableEntity {
        protected MealPlan() {
        }

        public MealPlan(string name, DateTime startDate, DateTime endDate) {
            MealPlanResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            entries = new List<MealPlanEntry>();
            Update(name, startDate, endDate);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealPlanId { get; private set; }

        public Guid MealPlanResourceId { get; private set; }

        [Required]
        [StringLength(250)]
        public string Name { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        private readonly List<MealPlanEntry> entries = new();
        public virtual IReadOnlyList<MealPlanEntry> Entries => entries;

        public void Update(string name, DateTime startDate, DateTime endDate) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(name), () => new InvalidValueError(nameof(name), name));
            messages.Aggregate(() => endDate < startDate, () => new InvalidValueError(nameof(endDate), "End date must be on or after start date"));
            messages.ThrowIfAny<ValidationListException>();

            Name = name;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void SetEntries(List<MealPlanEntry> newEntries) {
            entries.Clear();
            entries.AddRange(newEntries);
        }

        public void AddEntry(MealPlanEntry entry) {
            entries.Add(entry);
        }

        public void RemoveEntry(MealPlanEntry entry) {
            entries.Remove(entry);
        }
    }
}
