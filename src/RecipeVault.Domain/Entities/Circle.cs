using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(CircleResourceId), IsUnique = true)]
    [Table("Circle")]
    public class Circle : AuditableEntity {
        protected Circle() {
        }

        public Circle(string name, string description, Guid ownerSubjectId) {
            CircleResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            members = new List<CircleMember>();
            sharedRecipes = new List<CircleRecipe>();
            invites = new List<CircleInvite>();
            OwnerSubjectId = ownerSubjectId;
            Update(name, description);

            // Automatically add owner as a member
            var ownerMember = new CircleMember(CircleId, ownerSubjectId, CircleRole.Owner, MemberStatus.Active);
            members.Add(ownerMember);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleId { get; private set; }

        public Guid CircleResourceId { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; }

        [StringLength(500)]
        public string Description { get; private set; }

        public Guid OwnerSubjectId { get; private set; }

        private readonly List<CircleMember> members = new();
        public virtual IReadOnlyList<CircleMember> Members => members;

        private readonly List<CircleRecipe> sharedRecipes = new();
        public virtual IReadOnlyList<CircleRecipe> SharedRecipes => sharedRecipes;

        private readonly List<CircleInvite> invites = new();
        public virtual IReadOnlyList<CircleInvite> Invites => invites;

        public void Update(string name, string description) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(name), () => new InvalidValueError(nameof(name), name));
            messages.ThrowIfAny<ValidationListException>();

            Name = name;
            Description = description;
        }

        public CircleMember AddMember(Guid subjectId, CircleRole role, MemberStatus status) {
            var member = new CircleMember(CircleId, subjectId, role, status);
            members.Add(member);
            return member;
        }

        public void RemoveMember(CircleMember member) {
            members.Remove(member);
        }

        public CircleRecipe ShareRecipe(int recipeId, Guid sharedBySubjectId) {
            // Check if already shared
            if (sharedRecipes.Exists(sr => sr.RecipeId == recipeId)) {
                throw new InvalidOperationException("Recipe is already shared to this circle");
            }

            var circleRecipe = new CircleRecipe(CircleId, recipeId, sharedBySubjectId);
            sharedRecipes.Add(circleRecipe);
            return circleRecipe;
        }

        public void UnshareRecipe(CircleRecipe circleRecipe) {
            sharedRecipes.Remove(circleRecipe);
        }

        public CircleInvite CreateInvite(string inviteeEmail, Guid invitedBySubjectId, DateTime expiresDate) {
            var invite = new CircleInvite(CircleId, inviteeEmail, invitedBySubjectId, expiresDate);
            invites.Add(invite);
            return invite;
        }
    }
}
