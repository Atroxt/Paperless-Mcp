namespace Paperless.Mcp.Brokers.Paperlesses
{
    // Der Broker besitzt seine Konfiguration selbst: niemand sonst muss wissen, woher
    // Basisadresse und Token kommen.
    public class PaperlessBrokerConfigurations
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string ApiToken { get; set; } = string.Empty;

        public int HttpTimeoutSeconds { get; set; } = DefaultHttpTimeoutSeconds;

        public int UploadTimeoutSeconds { get; set; } = DefaultUploadTimeoutSeconds;

        // Im HTTP-Transport muss jede MCP-Anfrage ihr eigenes Token mitbringen, siehe
        // PaperlessAuthHandler. Bewusst nur aus der IConfiguration gelesen und nicht aus
        // einer Umgebungsvariablen: der Transport entscheidet das, nicht die Umgebung --
        // sonst liesse sich der Schutz von aussen abschalten.
        public bool RequireRequestToken { get; set; }

        public const int DefaultHttpTimeoutSeconds = 30;

        public const int DefaultUploadTimeoutSeconds = 600;

        public const string UploadTimeoutSecondsVariable = "UPLOAD_TIMEOUT_SECONDS";

        public const string RequireRequestTokenKey = "Mcp:RequireRequestToken";
    }
}
