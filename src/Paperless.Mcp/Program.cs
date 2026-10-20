using ModelContextProtocol.AspNetCore;
using Paperless.Mcp.Exposers.Middlewares;
using Paperless.Mcp.Exposers.Startups;
using Paperless.Mcp.Exposers.Tools;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Brokers.Paperlesses;
using static Paperless.Mcp.Exposers.Startups.StartupEndpoints;
using static Paperless.Mcp.Exposers.Startups.StartupRegistrations;

// Komposition: setzt das System zusammen und enthaelt selbst keine Geschaeftslogik.
// Zwei Transporte, derselbe Satz Abhaengigkeiten.
var useStdio = args.Contains("--stdio");
var serverVersion = ResolveServerVersion();

if (useStdio)
{
    // stdio-Transport fuer lokale Nutzung (Claude Desktop).
    //
    // ContentRootPath wird auf das Installationsverzeichnis gepinnt: Claude Desktop
    // startet MCP-Server mit cwd "/", und der Reload-Watcher der Host-Konfiguration
    // wuerde sonst das gesamte Dateisystem beobachten.
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory,
    });

    // Nur hier erlaubt: im stdio-Transport laeuft der Server auf dem Rechner des
    // Benutzers, "ein Pfad" meint also seinen eigenen.
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        [DocumentFileConfigurations.AllowLocalFileAccessKey] = "true",
    });

    RegisterBrokers(builder.Services);
    RegisterFoundationServices(builder.Services);
    RegisterProcessingServices(builder.Services, builder.Configuration);
    RegisterOrchestrationServices(builder.Services);

    RegisterCapabilities(
        builder.Services,
        builder.Configuration,
        CapabilityConfigurations.StdioTransport);

    // stdout transportiert das Protokoll selbst -- jede Logzeile muss nach stderr.
    builder.Logging.AddConsole(options =>
        options.LogToStandardErrorThreshold = LogLevel.Trace);

    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithRequestFilters(filters => filters.AddCallToolFilter(ToolCallLogging.LogToolCalls))
        .WithTools<TagTools>()
        .WithTools<HealthTools>()
        .WithTools<StatisticTools>()
        .WithTools<CorrespondentTools>()
        .WithTools<DocumentTypeTools>()
        .WithTools<StoragePathTools>()
        .WithTools<CustomFieldTools>()
        .WithTools<DocumentTools>()
        .WithTools<GlobalSearchTools>()
        .WithTools<TaskTools>()
        .WithTools<DocumentBulkTools>()
        .WithTools<DocumentUploadTools>()
        .WithTools<CapabilityTools>()

        // Erste Verteidigungslinie beim Dateizugriff: diese Werkzeuge gibt es nur hier.
        // Im HTTP-Zweig werden sie nicht angemeldet und erscheinen gar nicht erst in
        // tools/list.
        .WithTools<DocumentFileTools>();

    var host = builder.Build();

    ILogger startupLogger = host.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Paperless.Mcp");

    StartupLog.StartedStdio(startupLogger, serverVersion);

    await host.RunAsync();
}
else
{
    // HTTP-Transport fuer entfernte Nutzung.
    var builder = WebApplication.CreateBuilder(args);

    // Jede MCP-Anfrage muss ihr eigenes API-Token mitbringen. Der Broker faellt damit
    // NICHT auf ein konfiguriertes Token -- sonst wuerde ein oeffentlich
    // erreichbarer Server jedem Aufrufer die Dokumente des Betreibers zeigen.
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        [PaperlessBrokerConfigurations.RequireRequestTokenKey] = "true",
    });

    RegisterBrokers(builder.Services);
    RegisterFoundationServices(builder.Services);
    RegisterProcessingServices(builder.Services, builder.Configuration);
    RegisterOrchestrationServices(builder.Services);
    RegisterDownloadLinks(builder.Services, builder.Configuration);

    RegisterCapabilities(
        builder.Services,
        builder.Configuration,
        CapabilityConfigurations.HttpTransport);

    builder.Services
        .AddMcpServer()
        .WithHttpTransport(options =>
            // Stateless: jeder Request bringt sein eigenes Token mit, serverseitige
            // Sitzungen gibt es damit nicht.
            options.SessionMode = HttpServerSessionMode.Stateless)
        .WithRequestFilters(filters => filters.AddCallToolFilter(ToolCallLogging.LogToolCalls))
        .WithTools<TagTools>()
        .WithTools<HealthTools>()
        .WithTools<StatisticTools>()
        .WithTools<CorrespondentTools>()
        .WithTools<DocumentTypeTools>()
        .WithTools<StoragePathTools>()
        .WithTools<CustomFieldTools>()
        .WithTools<DocumentTools>()
        .WithTools<GlobalSearchTools>()
        .WithTools<TaskTools>()
        .WithTools<DocumentBulkTools>()
        .WithTools<DocumentUploadTools>()
        .WithTools<CapabilityTools>()

        // Der Einmal-Link ist im HTTP-Betrieb die Antwort auf "gib mir die Datei".
        // Im stdio-Zweig gibt es keinen Server, der ihn bedienen koennte -- dort
        // schreibt DocumentFileTools direkt auf die Platte.
        .WithTools<DownloadLinkTools>();

    var app = builder.Build();

    // Vor allem anderen: einfache Clients bedienbar machen, die nur application/json
    // akzeptieren. Siehe AcceptCompatibilityMiddleware -- sie ergaenzt den Header und
    // packt die Antwort danach wieder aus.
    app.UseMiddleware<AcceptCompatibilityMiddleware>();

    // Erste Verteidigungslinie: ohne Token kommt die Anfrage gar nicht bis zu einem Tool.
    // Die zweite sitzt im PaperlessAuthHandler, der ohne Request-Token keinen
    // Authorization-Header setzt.
    app.Use(RequireRequestTokenAsync);

    app.MapMcp("/mcp");

    // Der Einmal-Link. Bewusst OHNE Token-Pflicht, und das ist kein Loch, sondern der
    // Zweck: ein Browser kann keinen Authorization-Header mitschicken. Der Link selbst
    // ist der Ausweis -- unraetbar (32 Zufallsbytes), kurzlebig und genau einmal
    // einloesbar -- und er traegt das Token dessen, der ihn angefordert hat.
    app.MapGet(DownloadLinkConfigurations.RoutePrefix + "/{id}", RedeemDownloadLinkAsync);

    var port = ResolvePort(app.Configuration);

    StartupLog.Listening(app.Logger, serverVersion, port);

    await app.RunAsync($"http://0.0.0.0:{port}");
}

