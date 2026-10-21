namespace Paperless.Mcp.Models.Foundations.UploadLinks
{
    public class UploadLinkConfigurations
    {
        // Laenger als beim Download: der Nutzer muss die Datei erst heraussuchen.
        public int LifetimeSeconds { get; set; } = DefaultLifetimeSeconds;

        // Gilt nur fuer die Upload-Route. Der MCP-Endpunkt behaelt das kleine
        // Standardlimit von Kestrel.
        public long MaxUploadBytes { get; set; } = DefaultMaxUploadMegabytes * 1024L * 1024L;

        public string PublicUrl { get; set; } = string.Empty;

        public const int DefaultLifetimeSeconds = 900;

        public const int DefaultMaxUploadMegabytes = 200;

        public const string LifetimeSecondsVariable = "UPLOAD_LINK_LIFETIME_SECONDS";

        public const string MaxUploadMegabytesVariable = "MAX_UPLOAD_MEGABYTES";

        public const string RoutePrefix = "/uploads";
    }
}
