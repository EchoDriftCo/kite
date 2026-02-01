using System;

namespace RecipeVault.Domain.Events {
    public class RecipeCreatedEvent {
        public Guid RecipeResourceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
