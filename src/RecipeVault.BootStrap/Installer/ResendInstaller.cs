using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.DomainService;
using Resend;

namespace RecipeVault.BootStrap.Installer {
    public class ResendInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddOptions();
            services.AddHttpClient<ResendClient>();
            services.Configure<ResendClientOptions>(o => {
                o.ApiToken = configuration["Resend:ApiKey"]!;
            });
            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<IEmailService, ResendEmailService>();
        }
    }
}
