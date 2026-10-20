namespace Paperless.Mcp.Models.Foundations.Healths
{
    // Antwort von GET /api/remote_version/. Dieser Endpunkt beantwortet Anfragen ohne
    // Token und taugt deshalb als Erreichbarkeitstest, der auch dann noch etwas aussagt,
    // wenn das Token falsch ist.
    public class RemoteVersion
    {
        public string Version { get; set; } = string.Empty;

        public bool UpdateAvailable { get; set; }
    }
}
