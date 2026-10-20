namespace Paperless.Mcp.Models.Foundations.DocumentTypes
{
    // Primary Model, anemisch und flach. Felder nach der echten Antwort von
    // GET /api/document_types/{id}/ -- wie ein Korrespondent, nur ohne das Datum
    // der letzten Korrespondenz.
    public class DocumentType
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Match { get; set; }

        public DocumentTypeMatchingAlgorithm MatchingAlgorithm { get; set; }

        public bool IsInsensitive { get; set; }

        public int? Owner { get; set; }

        // Von Paperless vergeben, beim Schreiben ignoriert
        public string Slug { get; set; } = string.Empty;

        public int DocumentCount { get; set; }

        public bool UserCanChange { get; set; }
    }
}
