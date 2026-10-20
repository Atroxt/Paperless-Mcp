namespace Paperless.Mcp.Models.Foundations.DownloadLinks
{
    public class DownloadLinkConfigurations
    {
        public int LifetimeSeconds { get; set; } = DefaultLifetimeSeconds;

        // Absolute Basis-URL, unter der der Server von aussen erreichbar ist. Hinter
        // einem Reverse Proxy stimmt die Adresse aus dem Request nicht mit der ueberein,
        // die der Aufrufer kennt -- dann muss sie hier gesetzt werden.
        public string PublicUrl { get; set; } = string.Empty;

        public const int DefaultLifetimeSeconds = 300;

        public const string LifetimeSecondsVariable = "DOWNLOAD_LINK_LIFETIME_SECONDS";

        public const string PublicUrlVariable = "MCP_PUBLIC_URL";

        public const string RoutePrefix = "/files";
    }
}
