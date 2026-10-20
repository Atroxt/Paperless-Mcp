using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Services.Orchestrations.Capabilities;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    [McpServerToolType]
    public class CapabilityTools
    {
        private readonly ICapabilityOrchestrationService capabilityOrchestrationService;

        public CapabilityTools(
            ICapabilityOrchestrationService capabilityOrchestrationService) =>
            this.capabilityOrchestrationService = capabilityOrchestrationService;

        [McpServerTool(Name = "paperless_capabilities")]
        [Description(
            "Reports what this server can do: its version and transport, which "
            + "Paperless-ngx instance it talks to, whether it can touch local files or "
            + "hand out download links, and the limits that apply to paging, bulk "
            + "operations and uploads. Call this once before planning a larger task.")]
        public async ValueTask<ServerCapabilities> RetrieveCapabilitiesAsync()
        {
            try
            {
                return await this.capabilityOrchestrationService.RetrieveCapabilitiesAsync();
            }
            catch (Xeption)
            {
                // Dieser Bericht hat keine Eingabe, die falsch sein koennte -- was hier
                // ankommt, ist ein Fehler des Servers selbst.
                throw new McpException(
                    "The capabilities could not be determined. See the server log.");
            }
        }
    }
}
