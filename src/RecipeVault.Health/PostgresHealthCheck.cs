using System;
using System.Threading.Tasks;
using Cortside.Health;
using Cortside.Health.Checks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace RecipeVault.Health {
    public class PostgresHealthCheck : Check {
        private readonly string connectionString;

        public PostgresHealthCheck(IMemoryCache cache, ILogger<PostgresHealthCheck> logger, IAvailabilityRecorder recorder, IConfiguration configuration) : base(cache, logger, recorder) {
            connectionString = configuration["Database:ConnectionString"];
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            try {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT version()";
                var version = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                return new ServiceStatusModel {
                    Healthy = true,
                    Status = ServiceStatus.Ok,
                    StatusDetail = version?.ToString(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex) {
                return new ServiceStatusModel {
                    Healthy = false,
                    Status = ServiceStatus.Failure,
                    StatusDetail = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
