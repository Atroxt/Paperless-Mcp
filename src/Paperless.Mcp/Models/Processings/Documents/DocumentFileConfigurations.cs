namespace Paperless.Mcp.Models.Processings.Documents
{
    // Configuration Model fuer den Zugriff auf das lokale Dateisystem.
    //
    // Der Schalter wird NUR von Program.cs im stdio-Zweig gesetzt und bewusst nicht aus
    // einer Umgebungsvariablen gelesen -- genau wie Mcp:RequireRequestToken. Grund: im
    // HTTP-Transport laeuft der Server irgendwo, der Aufrufer sitzt woanders. Ein
    // Werkzeug, das "eine Datei vom Pfad liest", liest dann auf dem SERVER. Ein
    // entfernter Aufrufer koennte damit beliebige Serverdateien auslesen oder
    // ueberschreiben.
    public class DocumentFileConfigurations
    {
        public bool AllowLocalFileAccess { get; set; }

        public const string AllowLocalFileAccessKey = "Mcp:AllowLocalFileAccess";
    }
}
