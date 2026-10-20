namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Primary Model. Form nach GET /api/documents/{id}/ an einer echten Instanz.
    //
    // Bewusst nicht alle Felder der Antwort: duplicate_documents, root_document,
    // versions, is_shared_by_requester und deleted_at gehoeren zu Funktionen, die dieses
    // Projekt (noch) nicht bedient. Modelliert wird nur, was der Zweck verlangt.
    public class Document
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        // Der von Paperless extrahierte Text. Bei grossen Dokumenten entsprechend lang.
        public string Content { get; set; } = string.Empty;

        // Verweise auf die anderen Entities, jeweils optional
        public int? Correspondent { get; set; }

        public int? DocumentType { get; set; }

        public int? StoragePath { get; set; }

        public List<int> Tags { get; set; } = [];

        // Nur das Datum, ohne Uhrzeit -- Paperless liefert hier "2026-10-19"
        public DateOnly? Created { get; set; }

        public DateTimeOffset? Added { get; set; }

        public DateTimeOffset? Modified { get; set; }

        public int? ArchiveSerialNumber { get; set; }

        public string? OriginalFileName { get; set; }

        public string? ArchivedFileName { get; set; }

        public string? MimeType { get; set; }

        public int? PageCount { get; set; }

        public int? Owner { get; set; }

        public bool UserCanChange { get; set; }

        public List<DocumentCustomFieldValue> CustomFields { get; set; } = [];
    }
}
