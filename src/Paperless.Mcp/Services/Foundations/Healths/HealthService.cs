using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Healths;

namespace Paperless.Mcp.Services.Foundations.Healths
{
    public partial class HealthService : IHealthService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public HealthService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        // Keine Eingaben, also nichts zu validieren: beide Methoden sind reine
        // Abfragen und brauchen nur die Exception-Lokalisierung.
        public ValueTask<RemoteVersion> RetrieveRemoteVersionAsync() =>
            TryCatch(async () =>
                await this.paperlessBroker.GetRemoteVersionAsync());

        public ValueTask<PaperlessStatus> RetrieveStatusAsync() =>
            TryCatch(async () =>
                await this.paperlessBroker.GetStatusAsync());
    }
}
