using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class UnitFacade : IUnitFacade {
        private readonly IUnitOfWork uow;
        private readonly IUnitService unitService;
        private readonly UnitMapper mapper;
        private readonly ILogger<UnitFacade> logger;

        public UnitFacade(ILogger<UnitFacade> logger, IUnitOfWork uow, IUnitService unitService, UnitMapper mapper) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IReadOnlyList<UnitDto>> GetAllAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var units = await unitService.GetAllAsync().ConfigureAwait(false);
                return units.Select(u => mapper.MapToDto(u)).ToList();
            }
        }

        public async Task<UnitDto> GetByIdAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var unit = await unitService.GetByIdAsync(resourceId).ConfigureAwait(false);
                return unit != null ? mapper.MapToDto(unit) : null;
            }
        }

        public async Task<UnitMatchResultDto> MatchAsync(string input) {
            await using (var tx = uow.BeginNoTracking()) {
                var result = await unitService.MatchAsync(input).ConfigureAwait(false);
                return mapper.MapToDto(result);
            }
        }
    }
}
