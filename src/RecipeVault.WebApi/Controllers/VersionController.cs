using System;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RecipeVault.WebApi.Controllers {
    /// <summary>
    /// Version information endpoint
    /// </summary>
    [ApiVersion("1")]
    [ApiController]
    [Route("api/v{version:apiVersion}/version")]
    public class VersionController : ControllerBase {
        /// <summary>
        /// Gets the current application version
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetVersion() {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "1.0.0.0";
            var informationalVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? version;

            return Ok(new {
                version = informationalVersion,
                assemblyVersion = version,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }
    }
}
