using System.Reflection;
using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Delays;
using Paperless.Mcp.Brokers.DownloadLinks;
using Paperless.Mcp.Brokers.Files;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Brokers.UploadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Services.Foundations.Correspondents;
using Paperless.Mcp.Services.Foundations.CustomFields;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.DocumentTypes;
using Paperless.Mcp.Services.Foundations.DownloadLinks;
using Paperless.Mcp.Services.Foundations.GlobalSearches;
using Paperless.Mcp.Services.Foundations.Healths;
using Paperless.Mcp.Services.Foundations.Notes;
using Paperless.Mcp.Services.Foundations.Statistics;
using Paperless.Mcp.Services.Foundations.StoragePaths;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Foundations.Tasks;
using Paperless.Mcp.Services.Foundations.UploadLinks;
using Paperless.Mcp.Services.Orchestrations.Capabilities;
using Paperless.Mcp.Services.Orchestrations.Documents;
using Paperless.Mcp.Services.Processings.Correspondents;
using Paperless.Mcp.Services.Processings.CustomFields;
using Paperless.Mcp.Services.Processings.Documents;
using Paperless.Mcp.Services.Processings.DocumentTypes;
using Paperless.Mcp.Services.Processings.Healths;
using Paperless.Mcp.Services.Processings.Notes;
using Paperless.Mcp.Services.Processings.StoragePaths;
using Paperless.Mcp.Services.Processings.Tags;

namespace Paperless.Mcp.Exposers.Startups
{
    // Explizite Registrierung statt Assembly-Scanning: was man sieht, ist was man bekommt.
    internal static class StartupRegistrations
    {
        public static void RegisterBrokers(IServiceCollection services)
        {
            services.AddLogging();
            services.AddTransient<ILoggingBroker, LoggingBroker>();
            services.AddTransient<IDateTimeBroker, DateTimeBroker>();

            // Warten ist Technologie wie die Uhrzeit: als Broker in Tests ersetzbar, sonst
            // dauerte jeder Test der Upload-Orchestrierung so lange wie das Warten selbst.
            services.AddTransient<IDelayBroker, DelayBroker>();

            // In beiden Transporten registriert, damit der PaperlessAuthHandler dieselbe
            // Abhaengigkeit bekommt. Im stdio-Modus gibt es schlicht keinen HttpContext.
            services.AddHttpContextAccessor();
            services.AddTransient<IFileBroker, FileBroker>();

            // Einzige Ausnahme von Transient: der PaperlessBroker baut seinen eigenen
            // HttpClient, und der muss langlebig sein. Ein Transient-Broker wuerde pro Aufruf
            // einen neuen HttpClient erzeugen und damit Sockets erschoepfen. Das Token haengt
            // nicht am Client, sondern wird je Anfrage im PaperlessAuthHandler aufgeloest.
            services.AddSingleton<IPaperlessBroker, PaperlessBroker>();
        }

        public static void RegisterFoundationServices(IServiceCollection services)
        {
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<ICorrespondentService, CorrespondentService>();
            services.AddTransient<IDocumentTypeService, DocumentTypeService>();
            services.AddTransient<IStoragePathService, StoragePathService>();
            services.AddTransient<ICustomFieldService, CustomFieldService>();
            services.AddTransient<IDocumentService, DocumentService>();
            services.AddTransient<IDocumentNoteService, DocumentNoteService>();
            services.AddTransient<IGlobalSearchService, GlobalSearchService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<IHealthService, HealthService>();
            services.AddTransient<IStatisticService, StatisticService>();
        }

        public static void RegisterProcessingServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTransient<IHealthProcessingService, HealthProcessingService>();
            services.AddTransient<ICorrespondentProcessingService, CorrespondentProcessingService>();
            services.AddTransient<IDocumentTypeProcessingService, DocumentTypeProcessingService>();
            services.AddTransient<IStoragePathProcessingService, StoragePathProcessingService>();
            services.AddTransient<ICustomFieldProcessingService, CustomFieldProcessingService>();
            services.AddTransient<ITagProcessingService, TagProcessingService>();
            services.AddTransient<IDocumentNoteProcessingService, DocumentNoteProcessingService>();

            // Configuration Model: der Wert wird hier gelesen und hineingereicht, die Service
            // greift nicht selbst auf die Konfiguration zu.
            services.AddSingleton(new DocumentProcessingConfigurations
            {
                MaxPageSize = ResolveMaxPageSize(configuration),
            });

            services.AddTransient<IDocumentProcessingService, DocumentProcessingService>();

            services.AddSingleton(new DocumentFileConfigurations
            {
                AllowLocalFileAccess = configuration.GetValue<bool>(
                    DocumentFileConfigurations.AllowLocalFileAccessKey),
            });

            services.AddTransient<IDocumentFileProcessingService, DocumentFileProcessingService>();
            services.AddTransient<IDocumentBulkProcessingService, DocumentBulkProcessingService>();
        }

        // Mehrere Entities in einem Ablauf: Dokumente und Auftraege.
        public static void RegisterOrchestrationServices(IServiceCollection services) =>
            services.AddTransient<IDocumentOrchestrationService, DocumentOrchestrationService>();

        // Der Bericht ueber den Server selbst. Was hier hineingereicht wird, IST die Antwort --
        // deshalb steht es genau dort, wo der Transport auch entschieden wird, und wird nicht
        // irgendwo nachgelesen.
        public static void RegisterCapabilities(
            IServiceCollection services,
            IConfiguration configuration,
            string transport)
        {
            bool isStdio = transport == CapabilityConfigurations.StdioTransport;

            services.AddSingleton(new CapabilityConfigurations
            {
                ServerVersion = ResolveServerVersion(),
                Transport = transport,

                LocalFileAccess = configuration.GetValue<bool>(
                    DocumentFileConfigurations.AllowLocalFileAccessKey),

                // Einmal-Links braucht und bedient nur der HTTP-Zweig.
                DownloadLinks = transport == CapabilityConfigurations.HttpTransport,

                RequiresRequestToken = configuration.GetValue<bool>(
                    PaperlessBrokerConfigurations.RequireRequestTokenKey),

                MaxPageSize = ResolveMaxPageSize(configuration),

                DownloadLinkLifetimeSeconds = isStdio
                    ? 0
                    : ResolveLinkLifetimeSeconds(configuration),

                // Upload-Links gibt es wie Download-Links nur im HTTP-Zweig.
                UploadLinks = transport == CapabilityConfigurations.HttpTransport,

                MaxUploadLinkBytes = isStdio
                    ? 0
                    : ResolveMaxUploadBytes(configuration),
            });

            services.AddTransient<ICapabilityOrchestrationService, CapabilityOrchestrationService>();
        }

        public static void RegisterDownloadLinks(
            IServiceCollection services,
            IConfiguration configuration)
        {
            // Singleton: der Vorrat ausgestellter Links gehoert dem Prozess. Ein Transient-Broker
            // bekaeme bei jedem Aufruf einen leeren, und kein Link waere je einloesbar.
            services.AddSingleton<IDownloadLinkBroker, DownloadLinkBroker>();

            services.AddSingleton(new DownloadLinkConfigurations
            {
                LifetimeSeconds = ResolveLinkLifetimeSeconds(configuration),

                PublicUrl = Environment.GetEnvironmentVariable(
                    DownloadLinkConfigurations.PublicUrlVariable)
                    ?? configuration.GetValue<string>("Mcp:PublicUrl")
                    ?? string.Empty,
            });

            services.AddTransient<IDownloadLinkService, DownloadLinkService>();
        }

        public static void RegisterUploadLinks(
            IServiceCollection services,
            IConfiguration configuration)
        {
            // Singleton aus demselben Grund wie beim DownloadLinkBroker.
            services.AddSingleton<IUploadLinkBroker, UploadLinkBroker>();

            var configured = Environment.GetEnvironmentVariable(
                UploadLinkConfigurations.LifetimeSecondsVariable)
                ?? configuration.GetValue<string>("Mcp:UploadLinkLifetimeSeconds");

            services.AddSingleton(new UploadLinkConfigurations
            {
                LifetimeSeconds = int.TryParse(configured, out var parsed) && parsed > 0
                    ? parsed
                    : UploadLinkConfigurations.DefaultLifetimeSeconds,

                MaxUploadBytes = ResolveMaxUploadBytes(configuration),

                PublicUrl = Environment.GetEnvironmentVariable(
                    DownloadLinkConfigurations.PublicUrlVariable)
                    ?? configuration.GetValue<string>("Mcp:PublicUrl")
                    ?? string.Empty,
            });

            services.AddTransient<IUploadLinkService, UploadLinkService>();
        }

        private static long ResolveMaxUploadBytes(IConfiguration configuration)
        {
            var configured = Environment.GetEnvironmentVariable(
                UploadLinkConfigurations.MaxUploadMegabytesVariable)
                ?? configuration.GetValue<string>("Mcp:MaxUploadMegabytes");

            int megabytes = int.TryParse(configured, out var parsed) && parsed > 0
                ? parsed
                : UploadLinkConfigurations.DefaultMaxUploadMegabytes;

            return megabytes * 1024L * 1024L;
        }

        // Die Version, die der Build hineingeschrieben hat -- siehe Directory.Build.props.
        //
        // Alles ab "+" wird abgeschnitten: dort haengt der Commit-Hash, und der gehoert nicht in
        // einen Satz, den ein Assistent vorliest.
        public static string ResolveServerVersion()
        {
            string version = typeof(StartupRegistrations).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? "0.0.0-local";

            int metadata = version.IndexOf('+', StringComparison.Ordinal);

            return metadata < 0 ? version : version[..metadata];
        }

        public static int ResolvePort(IConfiguration configuration)
        {
            const int defaultPort = 5005;

            var configured = Environment.GetEnvironmentVariable("MCP_PORT")
                             ?? configuration.GetValue<string>("Mcp:Port");

            return int.TryParse(configured, out var parsed) && parsed > 0
                ? parsed
                : defaultPort;
        }

        private static int ResolveLinkLifetimeSeconds(IConfiguration configuration)
        {
            var configured = Environment.GetEnvironmentVariable(
                DownloadLinkConfigurations.LifetimeSecondsVariable)
                ?? configuration.GetValue<string>("Mcp:DownloadLinkLifetimeSeconds");

            return int.TryParse(configured, out var parsed) && parsed > 0
                ? parsed
                : DownloadLinkConfigurations.DefaultLifetimeSeconds;
        }

        private static int ResolveMaxPageSize(IConfiguration configuration)
        {
            var configured = Environment.GetEnvironmentVariable(
                DocumentProcessingConfigurations.MaxPageSizeVariable)
                ?? configuration.GetValue<string>(DocumentProcessingConfigurations.MaxPageSizeKey);

            return int.TryParse(configured, out var parsed) && parsed > 0
                ? parsed
                : DocumentProcessingConfigurations.DefaultMaxPageSize;
        }
    }
}
