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
    [Index(nameof(CookingLogResourceId), IsUnique = true)]
    [Index(nameof(RecipeId))]
    [Index(nameof(CookedDate))]
    [Table("CookingLog")]
    public class CookingLog : AuditableEntity {
        protected CookingLog() {
        }

        public CookingLog(int recipeId, DateTime cookedDate, decimal? scaleFactor, int? servingsMade, string notes, int? rating) {
            CookingLogResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            photos = new List<CookingLogPhoto>();
            Update(recipeId, cookedDate, scaleFactor, servingsMade, notes, rating);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CookingLogId { get; private set; }

        public Guid CookingLogResourceId { get; private set; }

        public int RecipeId { get; private set; }
        public virtual Recipe Recipe { get; private set; }

        public DateTime CookedDate { get; private set; }

        public decimal? ScaleFactor { get; private set; }

        public int? ServingsMade { get; private set; }

        [StringLength(2000)]
        public string Notes { get; private set; }

        public int? Rating { get; private set; }

        private readonly List<CookingLogPhoto> photos = new();
        public virtual IReadOnlyList<CookingLogPhoto> Photos => photos;

        public void Update(int recipeId, DateTime cookedDate, decimal? scaleFactor, int? servingsMade, string notes, int? rating) {
            var messages = new MessageList();
            messages.Aggregate(() => recipeId < 1, () => new InvalidValueError(nameof(recipeId), recipeId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.Aggregate(() => cookedDate > DateTime.UtcNow, () => new InvalidValueError(nameof(cookedDate), "CookedDate cannot be in the future"));
            messages.Aggregate(() => scaleFactor.HasValue && scaleFactor.Value <= 0, () => new InvalidValueError(nameof(scaleFactor), "ScaleFactor must be positive"));
            messages.Aggregate(() => servingsMade.HasValue && servingsMade.Value < 1, () => new InvalidValueError(nameof(servingsMade), "ServingsMade must be at least 1"));
            messages.Aggregate(() => rating.HasValue && (rating.Value < 1 || rating.Value > 5), () => new InvalidValueError(nameof(rating), "Rating must be between 1 and 5"));
            messages.ThrowIfAny<ValidationListException>();

            RecipeId = recipeId;
            CookedDate = cookedDate;
            ScaleFactor = scaleFactor;
            ServingsMade = servingsMade;
            Notes = notes;
            Rating = rating;
        }

        public void AddPhoto(CookingLogPhoto photo) {
            photos.Add(photo);
        }

        public void RemovePhoto(CookingLogPhoto photo) {
            photos.Remove(photo);
        }

        public void SetPhotos(List<CookingLogPhoto> newPhotos) {
            photos.Clear();
            photos.AddRange(newPhotos);
        }
    }
}
