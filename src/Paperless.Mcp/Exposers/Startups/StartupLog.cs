namespace Paperless.Mcp.Exposers.Startups
{
    // Quellgeneriertes Logging: kein Boxing, keine Formatierung, wenn das Level aus ist.
    internal static partial class StartupLog
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Paperless.Mcp {Version} lauscht auf http://0.0.0.0:{Port}/mcp")]
        public static partial void Listening(ILogger logger, string version, int port);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Paperless.Mcp {Version} gestartet (stdio)")]
        public static partial void StartedStdio(ILogger logger, string version);
    }
}
