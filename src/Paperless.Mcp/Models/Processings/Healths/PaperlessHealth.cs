namespace Paperless.Mcp.Models.Processings.Healths
{
    // Virtuelles Modell der Processing-Schicht: fasst zwei primitive Abfragen zu einer
    // Aussage zusammen. Es gibt keine Paperless-Antwort, die genau so aussieht.
    public class PaperlessHealth
    {
        // Antwortet die Instanz ueberhaupt?
        public bool IsReachable { get; set; }

        // Traegt das konfigurierte API-Token?
        public bool IsAuthenticated { get; set; }

        public string? Version { get; set; }

        public bool UpdateAvailable { get; set; }

        // Im Klartext, was fehlt -- die eigentliche Auskunft bei einem Fehlschlag.
        public string? Detail { get; set; }
    }
}
