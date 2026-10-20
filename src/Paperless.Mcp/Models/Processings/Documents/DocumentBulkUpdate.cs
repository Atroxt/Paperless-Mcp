namespace Paperless.Mcp.Models.Processings.Documents
{
    // Was an einer Menge von Dokumenten geaendert werden soll, in Geschaeftssprache.
    //
    // Bewusst fest typisiert statt als Methode-plus-Parameter-Verzeichnis wie die API:
    // bulk_edit kennt sechzehn Methoden, darunter solche, die Dateien umschreiben oder
    // Rechte setzen. Hier stehen nur die, die Zuordnungen aendern.
    public class DocumentBulkUpdate
    {
        public List<int> DocumentIds { get; set; } = [];

        // null = unberuehrt lassen. Siehe DocumentReferenceUpdate.
        public DocumentReferenceUpdate? Correspondent { get; set; }

        public DocumentReferenceUpdate? DocumentType { get; set; }

        public DocumentReferenceUpdate? StoragePath { get; set; }

        public List<int> AddTagIds { get; set; } = [];

        public List<int> RemoveTagIds { get; set; } = [];
    }
}
