using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto;

namespace RecipeVault.Facade {
    public class PremiumWaitlistFacade : IPremiumWaitlistFacade {
        private readonly IPremiumWaitlistService service;

        public PremiumWaitlistFacade(IPremiumWaitlistService service) {
            this.service = service;
        }

        public async Task<PremiumWaitlistDto> JoinWaitlistAsync(string email, string source) {
            var entry = await service.JoinWaitlistAsync(email, source);
            return MapToDto(entry);
        }

        public async Task<List<PremiumWaitlistDto>> GetAllAsync() {
            var entries = await service.GetAllAsync();
            return entries.Select(MapToDto).ToList();
        }

        private static PremiumWaitlistDto MapToDto(PremiumWaitlist entity) {
            return new PremiumWaitlistDto {
                PremiumWaitlistResourceId = entity.PremiumWaitlistResourceId,
                Email = entity.Email,
                Source = entity.Source,
                CreatedDate = entity.CreatedDate
            };
        }
    }
}
