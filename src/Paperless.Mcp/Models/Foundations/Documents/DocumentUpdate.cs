namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Virtuelles Modell fuer die teilweise Aenderung eines Dokuments.
    //
    // Paperless nimmt hier PATCH: was nicht gesendet wird, bleibt unveraendert --
    // einschliesslich des extrahierten Textes. Deshalb ein eigenes Modell statt des
    // vollstaendigen Document: wer den Korrespondenten setzt, soll nicht den Titel
    // mitschicken muessen und schon gar nicht den Inhalt.
    //
    // null heisst durchweg "nicht aendern". Eine Referenz zu LEEREN ist damit nicht
    // moeglich; im Bund geht es, siehe DocumentReferenceUpdate.
    public class DocumentUpdate
    {
        public int DocumentId { get; set; }

        public string? Title { get; set; }

        public int? CorrespondentId { get; set; }

        public int? DocumentTypeId { get; set; }

        public int? StoragePathId { get; set; }

        // Ersetzt die Tags vollstaendig. Eine leere Liste entfernt alle, null laesst sie
        // unveraendert -- bei einer Liste ist das eindeutig.
        public List<int>? TagIds { get; set; }

        public DateOnly? Created { get; set; }

        public int? ArchiveSerialNumber { get; set; }

        // Ersetzt die Custom-Field-Werte vollstaendig, gleiche Regel wie bei den Tags.
        public List<DocumentCustomFieldValue>? CustomFields { get; set; }
    }
}
