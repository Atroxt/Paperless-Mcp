namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Die Vorschaugrafik eines Dokuments. Paperless liefert sie als image/webp.
    public class DocumentThumbnail
    {
        public int DocumentId { get; set; }

        public byte[] Content { get; set; } = [];

        public string MimeType { get; set; } = DefaultMimeType;

        public const string DefaultMimeType = "image/webp";
    }
}
