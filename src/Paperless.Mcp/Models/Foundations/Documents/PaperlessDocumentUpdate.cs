namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Drahtform des PATCH-Rumpfs.
    //
    // Die Feldnamen weichen von DocumentUpdate ab: Paperless erwartet "correspondent",
    // nicht "correspondent_id". Die Serializer-Optionen des Brokers lassen null-Felder
    // weg, sodass genau das gesendet wird, was gesetzt wurde.
    public class PaperlessDocumentUpdate
    {
        public string? Title { get; set; }

        public int? Correspondent { get; set; }

        public int? DocumentType { get; set; }

        public int? StoragePath { get; set; }

        public List<int>? Tags { get; set; }

        public DateOnly? Created { get; set; }

        public int? ArchiveSerialNumber { get; set; }

        public List<DocumentCustomFieldValue>? CustomFields { get; set; }
    }
}
