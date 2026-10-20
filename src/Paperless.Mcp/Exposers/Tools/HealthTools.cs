using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Processings.Healths;
using Paperless.Mcp.Models.Processings.Healths.Exceptions;
using Paperless.Mcp.Services.Processings.Healths;

namespace Paperless.Mcp.Exposers.Tools
{
    [McpServerToolType]
    public class HealthTools
    {
        private readonly IHealthProcessingService healthProcessingService;

        public HealthTools(IHealthProcessingService healthProcessingService) =>
            this.healthProcessingService = healthProcessingService;

        [McpServerTool(Name = "paperless_ping")]
        [Description(
            "Checks whether the Paperless instance answers and whether the configured "
            + "API token is accepted. Returns the diagnosis instead of failing, so it "
            + "also works when the connection is broken.")]
        public async ValueTask<PaperlessHealth> PingAsync()
        {
            try
            {
                return await this.healthProcessingService.RetrieveHealthAsync();
            }
            catch (HealthProcessingServiceException)
            {
                throw new McpException(
                    "The health check itself failed unexpectedly. See the server log.");
            }
        }
    }
}
