using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeVault.Data;
using RecipeVault.Domain.Entities;

namespace RecipeVault.DomainService {
    public class PremiumWaitlistService : IPremiumWaitlistService {
        private readonly IRecipeVaultDbContext db;
        private readonly ILogger<PremiumWaitlistService> logger;

        public PremiumWaitlistService(IRecipeVaultDbContext db, ILogger<PremiumWaitlistService> logger) {
            this.db = db;
            this.logger = logger;
        }

        public async Task<PremiumWaitlist> JoinWaitlistAsync(string email, string source) {
            email = email?.Trim().ToLowerInvariant();

            // Check for duplicate
            var existing = await db.PremiumWaitlists.FirstOrDefaultAsync(w => w.Email == email);
            if (existing != null) {
                logger.LogInformation("Duplicate waitlist signup for {Email}", email);
                return existing; // Return existing — don't error, just idempotent
            }

            var entry = new PremiumWaitlist(email, source);
            await db.PremiumWaitlists.AddAsync(entry);
            await ((RecipeVaultDbContext)db).SaveChangesAsync();

            logger.LogInformation("New waitlist signup: {Email} from {Source}", email, source);
            return entry;
        }
    }
}
