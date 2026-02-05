using WireMock.Server;

namespace RecipeVault.Integrations.Gemini.Tests.Mocks {
    /// <summary>
    /// Interface for mock HTTP servers following the Cortside pattern
    /// </summary>
    public interface IMockServer {
        /// <summary>
        /// Configure the mock server with stubs
        /// </summary>
        void Configure(WireMockServer server);
    }
}
