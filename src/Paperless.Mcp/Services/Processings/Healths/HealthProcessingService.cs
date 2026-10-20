using Paperless.Mcp.Models.Foundations.Healths;
using Paperless.Mcp.Models.Foundations.Healths.Exceptions;
using Paperless.Mcp.Models.Processings.Healths;
using Paperless.Mcp.Services.Foundations.Healths;

namespace Paperless.Mcp.Services.Processings.Healths
{
    // Hoeherwertige Logik einer Entity: kombiniert die beiden primitiven Abfragen der
    // Foundation Service zu einer Aussage. Genau eine Foundation Service als
    // Abhaengigkeit, kein Entity-Broker.
    public partial class HealthProcessingService : IHealthProcessingService
    {
        private readonly IHealthService healthService;

        public HealthProcessingService(IHealthService healthService) =>
            this.healthService = healthService;

        // Ein Health-Check, der bei einem Fehler wirft, ist nutzlos: die Fehlschlaege
        // SIND hier die Auskunft. Erwartbare Abhaengigkeitsfehler werden deshalb zu
        // Feldern des Berichts, nicht zu Exceptions.
        public ValueTask<PaperlessHealth> RetrieveHealthAsync() =>
            TryCatch(async () =>
            {
                var health = new PaperlessHealth();

                if (await TryReadRemoteVersionAsync(health) is false)
                {
                    return health;
                }

                await TryReadStatusAsync(health);

                return health;
            });

        private async ValueTask<bool> TryReadRemoteVersionAsync(PaperlessHealth health)
        {
            try
            {
                RemoteVersion remoteVersion =
                    await this.healthService.RetrieveRemoteVersionAsync();

                health.IsReachable = true;
                health.Version = remoteVersion.Version;
                health.UpdateAvailable = remoteVersion.UpdateAvailable;

                return true;
            }
            catch (HealthDependencyException)
            {
                health.IsReachable = false;
                health.Detail =
                    "The Paperless instance did not answer. "
                    + "Check PAPERLESS_BASE_URL and whether the server is running.";

                return false;
            }
        }

        private async ValueTask TryReadStatusAsync(PaperlessHealth health)
        {
            try
            {
                await this.healthService.RetrieveStatusAsync();

                health.IsAuthenticated = true;
            }
            catch (HealthDependencyValidationException)
            {
                health.IsAuthenticated = false;
                health.Detail =
                    "The Paperless instance answered, but rejected the API token. "
                    + "Check PAPERLESS_API_TOKEN.";
            }
            catch (HealthDependencyException)
            {
                health.IsAuthenticated = false;
                health.Detail =
                    "The Paperless instance answered, but its status endpoint failed. "
                    + "The server may still be starting up.";
            }
        }
    }
}
