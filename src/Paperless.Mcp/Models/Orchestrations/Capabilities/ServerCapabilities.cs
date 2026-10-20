using Paperless.Mcp.Models.Processings.Healths;

namespace Paperless.Mcp.Models.Orchestrations.Capabilities
{
    // Was dieser Server kann, in einer Antwort. Gedacht fuer die Frage, die ein
    // Assistent am Anfang stellt: womit habe ich es hier zu tun?
    //
    // tools/list sagt bereits, WELCHE Werkzeuge es gibt. Hier steht, was man daraus
    // nicht ablesen kann: welche Instanz dahinter haengt, welche Grenzen gelten und
    // warum manche Werkzeuge fehlen.
    public class ServerCapabilities
    {
        public string ServerVersion { get; set; } = string.Empty;

        // "stdio" oder "http". Entscheidet, welche Werkzeuge es ueberhaupt gibt.
        public string Transport { get; set; } = string.Empty;

        // Erreichbarkeit, Token und Version der Instanz -- derselbe Bericht wie bei
        // paperless_ping, und wie dort auch dann gefuellt, wenn nichts antwortet.
        public PaperlessHealth Paperless { get; set; } = new();

        // Darf der Server auf das lokale Dateisystem? Nur im stdio-Betrieb.
        public bool LocalFileAccess { get; set; }

        // Gibt es Einmal-Links zum Herunterladen? Nur im HTTP-Betrieb.
        public bool DownloadLinks { get; set; }

        // Muss jede Anfrage ihr eigenes API-Token mitbringen?
        public bool RequiresRequestToken { get; set; }

        public int MaxPageSize { get; set; }

        public int MaxDocumentsPerBulkCall { get; set; }

        public int MaxUploadWaitSeconds { get; set; }

        public int DownloadLinkLifetimeSeconds { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
