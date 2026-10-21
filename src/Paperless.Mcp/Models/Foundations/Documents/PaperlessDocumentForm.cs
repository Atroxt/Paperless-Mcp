namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Drahtform des Multipart-Rumpfs fuer POST /api/documents/post_document/.
    //
    // Bewusst ohne die Formular-Attribute von RESTFulSense: deren Formularaufbau weist
    // nicht gesetzte Felder als ungueltige Argumente ab, auch leere Zeichenketten. Der
    // Broker baut das Formular deshalb selbst und laesst weg, was null ist -- die
    // Statuscode-Uebersetzung von RESTFulSense bleibt ueber ExecuteHttpCallAsync
    // trotzdem erhalten.
    public class PaperlessDocumentForm
    {
        public byte[] Document { get; set; } = [];

        public Stream? DocumentStream { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Correspondent { get; set; }

        public string? DocumentType { get; set; }

        public string? StoragePath { get; set; }

        public string? Created { get; set; }
    }
}
