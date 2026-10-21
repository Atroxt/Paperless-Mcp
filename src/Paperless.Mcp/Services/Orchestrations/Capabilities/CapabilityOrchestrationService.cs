using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Models.Processings.Healths;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Orchestrations.Documents;
using Paperless.Mcp.Services.Processings.Healths;

namespace Paperless.Mcp.Services.Orchestrations.Capabilities
{
    // Orchestration, weil der Bericht zwei Quellen zusammenfuehrt: die Gesundheit der
    // Paperless-Instanz und die Konfiguration dieses Servers.
    //
    // Die Gesundheit kommt bewusst aus der HealthProcessingService und wird nicht noch
    // einmal nachgebaut: sie ist die einzige Stelle, an der entschieden wird, was
    // "erreichbar" und "angemeldet" heisst, und zwei Stellen wuerden frueher oder
    // spaeter auseinanderlaufen. Dass es nur eine Abhaengigkeit ist, macht sie nicht zu
    // einer Processing Service -- die duerfte diese Schicht gar nicht aufrufen.
    public partial class CapabilityOrchestrationService : ICapabilityOrchestrationService
    {
        private readonly IHealthProcessingService healthProcessingService;
        private readonly CapabilityConfigurations configurations;

        public CapabilityOrchestrationService(
            IHealthProcessingService healthProcessingService,
            CapabilityConfigurations configurations)
        {
            this.healthProcessingService = healthProcessingService;
            this.configurations = configurations;
        }

        public ValueTask<ServerCapabilities> RetrieveCapabilitiesAsync() =>
            TryCatch(async () =>
            {
                PaperlessHealth health =
                    await this.healthProcessingService.RetrieveHealthAsync();

                var capabilities = new ServerCapabilities
                {
                    ServerVersion = this.configurations.ServerVersion,
                    Transport = this.configurations.Transport,
                    Paperless = health,
                    LocalFileAccess = this.configurations.LocalFileAccess,
                    DownloadLinks = this.configurations.DownloadLinks,
                    RequiresRequestToken = this.configurations.RequiresRequestToken,
                    MaxPageSize = this.configurations.MaxPageSize,
                    MaxDocumentsPerBulkCall = DocumentService.MaxDocumentsPerBulkCall,
                    MaxUploadWaitSeconds = DocumentOrchestrationService.MaxTimeoutSeconds,

                    DownloadLinkLifetimeSeconds =
                        this.configurations.DownloadLinkLifetimeSeconds,

                    UploadLinks = this.configurations.UploadLinks,
                    MaxUploadLinkBytes = this.configurations.MaxUploadLinkBytes,
                };

                capabilities.Summary = Describe(capabilities);

                return capabilities;
            });

        // Der Satz, den ein Assistent liest, bevor er die Zahlen liest.
        private static string Describe(ServerCapabilities capabilities)
        {
            if (capabilities.Paperless.IsReachable is false)
            {
                return
                    $"Paperless.Mcp {capabilities.ServerVersion} over "
                    + $"{capabilities.Transport}, but the Paperless instance is not "
                    + "reachable. Only this report works until that is fixed.";
            }

            string files = capabilities.LocalFileAccess
                ? "files can be read from and written to local paths"
                : capabilities.DownloadLinks
                    ? "files are handed out as temporary download links"
                    : "files cannot leave the server";

            return
                $"Paperless.Mcp {capabilities.ServerVersion} over "
                + $"{capabilities.Transport}, talking to Paperless-ngx "
                + $"{capabilities.Paperless.Version}; {files}.";
        }
    }
}
