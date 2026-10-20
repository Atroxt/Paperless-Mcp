namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Die Datei eines Dokuments samt der beiden Angaben, die ein Browser braucht:
    // Inhaltstyp und Dateiname. Beide stehen in den Antwortkopfzeilen von Paperless,
    // nicht im Rumpf.
    public class DocumentFile
    {
        public int DocumentId { get; set; }

        public byte[] Content { get; set; } = [];

        public string ContentType { get; set; } = DefaultContentType;

        public string FileName { get; set; } = string.Empty;

        public const string DefaultContentType = "application/octet-stream";
    }
}
