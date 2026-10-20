namespace Paperless.Mcp.Models.Foundations.Correspondents
{
    // Primary Model, anemisch und flach. Felder nach der echten Antwort von
    // GET /api/correspondents/{id}/ -- Korrespondenten haben weder Farbe noch
    // Posteingangs-Kennzeichen, dafuer das Datum der letzten Korrespondenz.
    public class Correspondent
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Match { get; set; }

        public CorrespondentMatchingAlgorithm MatchingAlgorithm { get; set; }

        public bool IsInsensitive { get; set; }

        public int? Owner { get; set; }

        // Von Paperless vergeben, beim Schreiben ignoriert
        public string Slug { get; set; } = string.Empty;

        public int DocumentCount { get; set; }

        public DateTimeOffset? LastCorrespondence { get; set; }

        public bool UserCanChange { get; set; }
    }
}
