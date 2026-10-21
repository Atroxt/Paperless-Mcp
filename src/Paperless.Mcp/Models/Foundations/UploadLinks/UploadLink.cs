namespace Paperless.Mcp.Models.Foundations.UploadLinks
{
    // Ein ausgestellter Einmal-Link zum Hochladen.
    //
    // Gegenstueck zum DownloadLink: grosse Dateien gehen nicht durch das Sprachmodell
    // (Base64 in einem Tool-Argument), sondern direkt vom Aufrufer an diesen Server,
    // der sie ungepuffert an Paperless weiterreicht. Die Metadaten werden beim
    // Ausstellen festgelegt, die Datei kommt beim Einloesen.
    //
    // Wie beim Download ist der Link selbst der Ausweis: unraetbar, kurzlebig, genau
    // einmal einloesbar, und er traegt das Token DES AUFRUFERS.
    public class UploadLink
    {
        public string Id { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string? Title { get; set; }

        public int? CorrespondentId { get; set; }

        public int? DocumentTypeId { get; set; }

        public int? StoragePathId { get; set; }

        public DateOnly? Created { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
