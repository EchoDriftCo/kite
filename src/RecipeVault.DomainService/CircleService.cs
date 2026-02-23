using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class CircleService : ICircleService {
        private readonly ILogger<CircleService> logger;
        private readonly ICircleRepository circleRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public CircleService(ICircleRepository circleRepository, IRecipeRepository recipeRepository, ILogger<CircleService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.circleRepository = circleRepository;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<Circle> CreateCircleAsync(UpdateCircleDto dto) {
            var entity = new Circle(dto.Name, dto.Description, CurrentSubjectId);

            using (logger.PushProperty("CircleResourceId", entity.CircleResourceId)) {
                await circleRepository.AddAsync(entity);
                logger.LogInformation("Created new circle");
                return entity;
            }
        }

        public async Task<Circle> GetCircleAsync(Guid circleResourceId) {
            var entity = await circleRepository.GetAsync(circleResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new CircleNotFoundException($"Circle with id {circleResourceId} not found");
            }

            // Check if user is a member
            if (!entity.Members.Any(m => m.SubjectId == CurrentSubjectId && m.Status == MemberStatus.Active)) {
                throw new CircleNotFoundException($"Circle with id {circleResourceId} not found");
            }

            return entity;
        }

        public Task<PagedList<Circle>> SearchCirclesAsync(CircleSearch search) {
            // Ensure users only see their own circles
            search.SubjectId = CurrentSubjectId;
            return circleRepository.SearchAsync(search);
        }

        public async Task<Circle> UpdateCircleAsync(Guid resourceId, UpdateCircleDto dto) {
            var entity = await GetCircleAsync(resourceId).ConfigureAwait(false);
            
            // Check if user is owner or admin
            var member = entity.Members.FirstOrDefault(m => m.SubjectId == CurrentSubjectId);
            if (member == null || (member.Role != CircleRole.Owner && member.Role != CircleRole.Admin)) {
                throw new UnauthorizedAccessException("Only circle owners and admins can update circle details");
            }

            using (logger.PushProperty("CircleResourceId", entity.CircleResourceId)) {
                entity.Update(dto.Name, dto.Description);
                logger.LogInformation("Updated circle");
                return entity;
            }
        }

        public async Task DeleteCircleAsync(Guid resourceId) {
            var entity = await GetCircleAsync(resourceId).ConfigureAwait(false);
            
            // Check if user is owner
            if (entity.OwnerSubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("Only circle owner can delete the circle");
            }

            using (logger.PushProperty("CircleResourceId", entity.CircleResourceId)) {
                await circleRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted circle");
            }
        }

        public async Task<CircleInvite> InviteToCircleAsync(Guid circleResourceId, InviteToCircleDto dto) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            
            // Check if user can invite (owner or admin)
            var member = circle.Members.FirstOrDefault(m => m.SubjectId == CurrentSubjectId);
            if (member == null || member.Role == CircleRole.Member) {
                throw new UnauthorizedAccessException("Only circle owners and admins can invite members");
            }

            // Set expiration (default 7 days)
            var expiresDate = dto.ExpiresDate ?? DateTime.UtcNow.AddDays(7);
            
            var invite = circle.CreateInvite(dto.InviteeEmail, CurrentSubjectId, expiresDate);
            
            using (logger.PushProperty("CircleResourceId", circle.CircleResourceId)) {
                using (logger.PushProperty("InviteToken", invite.InviteToken)) {
                    logger.LogInformation("Created circle invite");
                }
            }

            return invite;
        }

        public async Task<Circle> AcceptInviteAsync(Guid inviteToken) {
            var invite = await circleRepository.GetInviteByTokenAsync(inviteToken).ConfigureAwait(false);
            if (invite == null) {
                throw new CircleInviteNotFoundException($"Invite with token {inviteToken} not found");
            }

            invite.Accept();  // This will throw if expired or already used

            // Check if already a member
            var existingMember = await circleRepository.GetMemberAsync(invite.CircleId, CurrentSubjectId).ConfigureAwait(false);
            if (existingMember != null) {
                if (existingMember.Status == MemberStatus.Active) {
                    throw new InvalidOperationException("You are already a member of this circle");
                }
                // Reactivate if they previously left
                existingMember.Activate();
            } else {
                // Add new member
                invite.Circle.AddMember(CurrentSubjectId, CircleRole.Member, MemberStatus.Active);
            }

            using (logger.PushProperty("CircleResourceId", invite.Circle.CircleResourceId)) {
                logger.LogInformation("User accepted circle invite");
            }

            return invite.Circle;
        }

        public async Task<CircleInvite> GetInviteDetailsAsync(Guid inviteToken) {
            var invite = await circleRepository.GetInviteByTokenAsync(inviteToken).ConfigureAwait(false);
            if (invite == null) {
                throw new CircleInviteNotFoundException($"Invite with token {inviteToken} not found");
            }
            return invite;
        }

        public async Task RemoveMemberAsync(Guid circleResourceId, Guid subjectId) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            
            // Check if user can remove members (owner or admin)
            var currentMember = circle.Members.FirstOrDefault(m => m.SubjectId == CurrentSubjectId);
            if (currentMember == null || currentMember.Role == CircleRole.Member) {
                throw new UnauthorizedAccessException("Only circle owners and admins can remove members");
            }

            var memberToRemove = circle.Members.FirstOrDefault(m => m.SubjectId == subjectId);
            if (memberToRemove == null) {
                throw new CircleMemberNotFoundException($"Member not found in circle");
            }

            // Can't remove the owner
            if (memberToRemove.Role == CircleRole.Owner) {
                throw new InvalidOperationException("Cannot remove circle owner");
            }

            circle.RemoveMember(memberToRemove);
            
            using (logger.PushProperty("CircleResourceId", circle.CircleResourceId)) {
                logger.LogInformation("Removed member from circle");
            }
        }

        public async Task LeaveCircleAsync(Guid circleResourceId) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            
            var member = circle.Members.FirstOrDefault(m => m.SubjectId == CurrentSubjectId);
            
            if (member == null) {
                throw new CircleMemberNotFoundException("You are not a member of this circle");
            }

            // Owner can't leave, must delete circle or transfer ownership
            if (member.Role == CircleRole.Owner) {
                throw new InvalidOperationException("Circle owner cannot leave. Transfer ownership or delete the circle.");
            }

            member.Leave();
            
            using (logger.PushProperty("CircleResourceId", circle.CircleResourceId)) {
                logger.LogInformation("User left circle");
            }
        }

        public async Task<Circle> ShareRecipeToCircleAsync(Guid circleResourceId, ShareRecipeToCircleDto dto) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            var recipe = await recipeRepository.GetAsync(dto.RecipeResourceId).ConfigureAwait(false);
            
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {dto.RecipeResourceId} not found");
            }

            // Check if user owns the recipe or it's public
            if (recipe.CreatedSubject?.SubjectId != CurrentSubjectId && !recipe.IsPublic) {
                throw new UnauthorizedAccessException("You can only share your own recipes or public recipes");
            }

            circle.ShareRecipe(recipe.RecipeId, CurrentSubjectId);
            
            using (logger.PushProperty("CircleResourceId", circle.CircleResourceId)) {
                using (logger.PushProperty("RecipeResourceId", recipe.RecipeResourceId)) {
                    logger.LogInformation("Shared recipe to circle");
                }
            }

            return circle;
        }

        public async Task UnshareRecipeFromCircleAsync(Guid circleResourceId, Guid recipeResourceId) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            var circleRecipe = await circleRepository.GetCircleRecipeAsync(circle.CircleId, recipe.RecipeId).ConfigureAwait(false);
            if (circleRecipe == null) {
                throw new InvalidOperationException("Recipe is not shared in this circle");
            }

            // Check if user shared it or is admin/owner
            var member = circle.Members.FirstOrDefault(m => m.SubjectId == CurrentSubjectId);
            
            if (circleRecipe.SharedBySubjectId != CurrentSubjectId && 
                (member == null || member.Role == CircleRole.Member)) {
                throw new UnauthorizedAccessException("You can only unshare recipes you shared, unless you're an admin or owner");
            }

            circle.UnshareRecipe(circleRecipe);
            
            using (logger.PushProperty("CircleResourceId", circle.CircleResourceId)) {
                using (logger.PushProperty("RecipeResourceId", recipe.RecipeResourceId)) {
                    logger.LogInformation("Unshared recipe from circle");
                }
            }
        }

        public async Task<PagedList<Recipe>> GetCircleRecipesAsync(Guid circleResourceId, int pageNumber = 1, int pageSize = 20) {
            var circle = await GetCircleAsync(circleResourceId).ConfigureAwait(false);
            
            var recipes = circle.SharedRecipes.Select(sr => sr.Recipe).ToList();
            
            var result = new PagedList<Recipe> {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = recipes.Count,
                Items = recipes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };

            return result;
        }
    }
}
