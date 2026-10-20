namespace Paperless.Mcp.Models.Orchestrations.Capabilities
{
    // Configuration Model: Program.cs liest die Werte und reicht sie herein, der Dienst
    // greift nicht selbst auf die Konfiguration zu. Nur so kann der Bericht sagen, was
    // tatsaechlich gilt, statt was in einer Umgebungsvariablen stehen koennte.
    public class CapabilityConfigurations
    {
        public string ServerVersion { get; set; } = string.Empty;

        public string Transport { get; set; } = string.Empty;

        public bool LocalFileAccess { get; set; }

        public bool DownloadLinks { get; set; }

        public bool RequiresRequestToken { get; set; }

        public int MaxPageSize { get; set; }

        public int DownloadLinkLifetimeSeconds { get; set; }

        public const string StdioTransport = "stdio";

        public const string HttpTransport = "http";
    }
}
