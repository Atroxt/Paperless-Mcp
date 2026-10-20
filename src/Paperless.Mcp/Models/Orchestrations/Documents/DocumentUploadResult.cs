using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Models.Orchestrations.Documents
{
    // Das Ergebnis eines Uploads, nachdem gewartet wurde.
    //
    // Es gibt drei Ausgaenge, und alle drei sind Auskunft, kein Fehler: fertig,
    // fehlgeschlagen, oder noch unterwegs. Besonders der dritte -- ein Upload, der
    // laenger dauert als die Wartezeit, ist nicht verloren, sondern laeuft weiter. Die
    // TaskId ist dann der Faden, an dem man ihn wiederfindet.
    public class DocumentUploadResult
    {
        public string TaskId { get; set; } = string.Empty;

        // Der Rohwert von Paperless, klein geschrieben, etwa "success".
        public string Status { get; set; } = string.Empty;

        public bool Completed { get; set; }

        public int? DocumentId { get; set; }

        // Nur gefuellt, wenn die Verarbeitung durch ist.
        public Document? Document { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
