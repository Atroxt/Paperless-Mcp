using Paperless.Mcp.Models.Orchestrations.Capabilities;

namespace Paperless.Mcp.Services.Orchestrations.Capabilities
{
    public interface ICapabilityOrchestrationService
    {
        // Antwortet auch dann, wenn die Paperless-Instanz nicht erreichbar ist -- die
        // Konfiguration dieses Servers gilt unabhaengig davon, und gerade dann ist der
        // Bericht nuetzlich.
        ValueTask<ServerCapabilities> RetrieveCapabilitiesAsync();
    }
}
