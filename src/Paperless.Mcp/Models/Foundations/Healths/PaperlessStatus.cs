namespace Paperless.Mcp.Models.Foundations.Healths
{
    // Antwort von GET /api/status/. Der Endpunkt verlangt ein gueltiges Token und dient
    // hier genau dazu: er beantwortet die Frage, ob das Token traegt.
    //
    // Bewusst nur die Felder, die wir auswerten. System.Text.Json ignoriert unbekannte
    // Eigenschaften, der Rest der Antwort darf also wachsen, ohne uns zu brechen.
    public class PaperlessStatus
    {
        public string PngxVersion { get; set; } = string.Empty;

        public string ServerOs { get; set; } = string.Empty;

        public string InstallType { get; set; } = string.Empty;
    }
}
