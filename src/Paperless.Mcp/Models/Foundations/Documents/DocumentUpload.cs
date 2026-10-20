namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Virtuelles Modell fuer das Hochladen.
    //
    // Paperless nimmt die Datei entgegen und antwortet mit der Kennung eines
    // Verarbeitungsauftrags, nicht mit einem Dokument: Texterkennung und Ablage laufen
    // asynchron. Das Dokument gibt es erst, wenn der Auftrag fertig ist -- siehe
    // DocumentTask.
    public class DocumentUpload
    {
        public string FileName { get; set; } = string.Empty;

        public byte[] Content { get; set; } = [];

        public string? Title { get; set; }

        public int? CorrespondentId { get; set; }

        public int? DocumentTypeId { get; set; }

        public int? StoragePathId { get; set; }

        public DateOnly? Created { get; set; }
    }
}
